using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;


//[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{
    // Bool that has the characters (facing the player) actions become mirrored. Default false.
    public static Transform Neck;
    public static Vector3 NeckOri;
    public bool mirroredMovement = true;

    // Bool that determines whether the avatar is allowed to move in vertical direction.
    //public bool verticalMovement = false;

    // Rate at which avatar will move through the scene. The rate multiplies the movement speed (.001f, i.e dividing by 1000, unity's framerate).
    protected int moveRate = 1;

    // Slerp smooth factor
    public float smoothFactor = 5f;

    // Whether the offset node must be repositioned to the user's coordinates, as reported by the sensor or not.
    public bool offsetRelativeToSensor = false;

    public static Transform[] GetBones { get { return bones; } }
    public static Transform GetNeck { get { return Neck; } }
    public static Vector3 GetNeckOri { get { return NeckOri; } }
    // The body root node
    protected Transform bodyRoot;

    // A required variable if you want to rotate the model in space.
    protected GameObject offsetNode;

    // Variable to hold all them bones. It will initialize the same size as initialRotations.
    protected static Transform[] bones;
    //protected static Transform[] mirroredBones;

    // Rotations of the bones when the Kinect tracking starts.
    protected Quaternion[] initialRotations;
    protected Quaternion[] initialLocalRotations;

    // Initial position and rotation of the transform
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;

    // Calibration Offset Variables for Character Position.
    protected bool offsetCalibrated = false;
    protected float xOffset, yOffset, zOffset;

    // private instance of the KinectManager
    protected KinectManager kinectManager;
    protected FacetrackingManager faceTrackingManger;


    //int posenum = 0;

    // transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
    private Transform _transformCache;

    public new Transform transform
    {
        get
        {
            if (!_transformCache)
                _transformCache = base.transform;

            return _transformCache;
        }
    }

    public void Awake()
    {
        // check for double start
        if (bones != null)
            return;

        // inits the bones array
        bones = new Transform[7];

        // Initial rotations and directions of the bones.
        initialRotations = new Quaternion[bones.Length];
        initialLocalRotations = new Quaternion[bones.Length];



        // Map bones to the points the Kinect tracks
        MapBones(); //make bones array 

        Neck = GameObject.Find("Neck").transform;

        // Get initial bone rotations
        GetInitialRotations();

    }



    //private void Update()
    //{
    //    GetNeckAngle(faceTrackingManger, Vector3.zero);
    //}
    // Update the avatar each frame.

    public void UpdateAvatar(uint UserID)
    {
        NeckOri = Vector3.zero;


        if (!transform.gameObject.activeInHierarchy)
            return;

        // Get the KinectManager instance
        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;

        }

        // move the avatar to its Kinect position
        MoveAvatar(UserID);

        for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!bones[boneIndex])
                continue;

            if (boneIndex2JointMap.ContainsKey(boneIndex))
            {
                KinectWrapper.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];

                TransformBone(UserID, joint, boneIndex, !mirroredMovement);

            }
            else if (specIndex2JointMap.ContainsKey(boneIndex))
            {
                // special bones (clavicles)
                List<KinectWrapper.NuiSkeletonPositionIndex> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];

                if (alJoints.Count >= 2)
                {
                    //Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                    //TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
                }
            }
        }

        GetNeckAngle(faceTrackingManger, ref NeckOri);

        Neck.localRotation = Quaternion.Euler(NeckOri);



    }

    // Set bones to their initial positions and rotations
    public void ResetToInitialPosition()
    {
        if (bones == null)
            return;

        if (offsetNode != null)
        {
            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

        // For each bone that was defined, reset to initial position.
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                bones[i].rotation = initialRotations[i];
            }
        }

        Neck.localRotation = Quaternion.Euler(Vector3.zero);

        if (bodyRoot != null)
        {
            bodyRoot.localPosition = Vector3.zero;
            bodyRoot.localRotation = Quaternion.identity;
        }

        // Restore the offset's position and rotation
        if (offsetNode != null)
        {
            offsetNode.transform.position = initialPosition;
            offsetNode.transform.rotation = initialRotation;
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    // Invoked on the successful calibration of a player.
    public void SuccessfulCalibration(uint userId)
    {
        // reset the models position
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation;
        }

        // re-calibrate the position offset
        offsetCalibrated = false;
    }

    // Apply the rotations tracked by kinect to the joints.
    protected void TransformBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {


        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null || kinectManager == null)
            return;

        int iJoint = (int)joint;
        if (iJoint < 0)
            return;

        // Get Kinect joint orientation
        Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
        //if (jointRotation == Quaternion.identity)
        //    return;

        // Smoothly transition to the new rotation
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);



        if (smoothFactor != 0f)
            boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        else
            boneTransform.rotation = newRotation;



    }

    // Apply the rotations tracked by kinect to a special joint
    protected void TransformSpecialBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, KinectWrapper.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip)
    {
        Transform boneTransform = bones[boneIndex];
        if (boneTransform == null || kinectManager == null)
            return;

        if (!kinectManager.IsJointTracked(userId, (int)joint) ||
           !kinectManager.IsJointTracked(userId, (int)jointParent))
        {
            return;
        }

        Vector3 jointDir = kinectManager.GetDirectionBetweenJoints(userId, (int)jointParent, (int)joint, false, true);
        Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;

        //      if(!flip)
        //      {
        //         Vector3 mirroredAngles = jointRotation.eulerAngles;
        //         mirroredAngles.y = -mirroredAngles.y;
        //         mirroredAngles.z = -mirroredAngles.z;
        //         
        //         jointRotation = Quaternion.Euler(mirroredAngles);
        //      }

        if (jointRotation != Quaternion.identity)
        {
            // Smoothly transition to the new rotation
            Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

            if (smoothFactor != 0f)
                boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
            else
                boneTransform.rotation = newRotation;
        }

    }

    // Moves the avatar in 3D space - pulls the tracked position of the spine and applies it to root.
    // Only pulls positional, not rotational.
    protected void MoveAvatar(uint UserID)
    {
        if (bodyRoot == null || kinectManager == null)
            return;
        if (!kinectManager.IsJointTracked(UserID, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter))
            return;

        // Get the position of the body and store it.
        Vector3 trans = kinectManager.GetUserPosition(UserID);

        // If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
        if (!offsetCalibrated)
        {
            offsetCalibrated = true;

            xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
            yOffset = trans.y * moveRate;
            zOffset = -trans.z * moveRate;

            if (offsetRelativeToSensor)
            {
                Vector3 cameraPos = Camera.main.transform.position;

                float yRelToAvatar = (offsetNode != null ? offsetNode.transform.position.y : transform.position.y) - cameraPos.y;
                Vector3 relativePos = new Vector3(trans.x * moveRate, yRelToAvatar, trans.z * moveRate);
                Vector3 offsetPos = cameraPos + relativePos;

                if (offsetNode != null)
                {
                    offsetNode.transform.position = offsetPos;
                }
                else
                {
                    transform.position = offsetPos;
                }
            }
        }

        // Smoothly transition to the new position
        Vector3 targetPos = Kinect2AvatarPos(trans);

        if (smoothFactor != 0f)
            bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime);
        else
            bodyRoot.localPosition = targetPos;
    }

    // If the bones to be mapped have been declared, map that bone to the model.
    protected virtual void MapBones()
    {
        // make OffsetNode as a parent of model transform.
        offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
        offsetNode.transform.position = transform.position;
        offsetNode.transform.rotation = transform.rotation;
        offsetNode.transform.parent = transform.parent;

        transform.parent = offsetNode.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // take model transform as body root
        bodyRoot = transform;

        // get bone transforms from the animator component
        var animatorComponent = GetComponent<Animator>();

        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)  //22 bones
        {
            if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                continue;

            bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]);
            //Debug.Log("bones �迭 : "+ bones[boneIndex]);
            //mirroredBones[boneIndex] = animatorComponent.GetBoneTransform(mirroredBoneIndex2MecanimMap[boneIndex]);
        }
    }

    // Capture the initial rotations of the bones
    protected void GetInitialRotations()
    {
        // save the initial rotation
        if (offsetNode != null)
        {
            initialPosition = offsetNode.transform.position;
            initialRotation = offsetNode.transform.rotation;

            offsetNode.transform.rotation = Quaternion.identity;
        }
        else
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            transform.rotation = Quaternion.identity;
        }

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
            {
                initialRotations[i] = bones[i].rotation; // * Quaternion.Inverse(initialRotation);
                initialLocalRotations[i] = bones[i].localRotation;
            }
        }

        // Restore the initial rotation
        if (offsetNode != null)
        {
            offsetNode.transform.rotation = initialRotation;
        }
        else
        {
            transform.rotation = initialRotation;
        }
    }

    // Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
    protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
    {
        // Apply the new rotation.
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];

        //If an offset node is specified, combine the transform with its
        //orientation to essentially make the skeleton relative to the node
        if (offsetNode != null)
        {
            // Grab the total rotation by adding the Euler and offset's Euler.
            Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
            //if (boneIndex == 2){
            //    totalRotation.y = Mathf.Clamp(totalRotation.y, -150, 0);
            //    }

            //if (boneIndex == 0)
            //{
            //    Mathf.Clamp(totalRotation.y, 0, 150);
            //}
            //// Grab our new rotation.
            newRotation = Quaternion.Euler(totalRotation);
        }

        return newRotation;
    }

    // Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
    protected Vector3 Kinect2AvatarPos(Vector3 jointPosition)
    {
        //float xPos;
        //float yPos;
        //float zPos;

        //// If movement is mirrored, reverse it.
        //if (!mirroredMovement)
        //    xPos = jointPosition.x * moveRate - xOffset;
        //else
        //    xPos = -jointPosition.x * moveRate - xOffset;

        //yPos = jointPosition.y * moveRate - yOffset;
        //zPos = -jointPosition.z * moveRate - zOffset;

        // If we are tracking vertical movement, update the y. Otherwise leave it alone.
        Vector3 avatarJointPos = new Vector3(0, 0, 0);

        return avatarJointPos;
    }

    // dictionaries to speed up bones' processing
    // the author of the terrific idea for kinect-joints to mecanim-bones mapping
    // along with its initial implementation, including following dictionary is
    // Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
    private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
    {
        {0, HumanBodyBones.Spine},
        {1, HumanBodyBones.RightShoulder},
        {2, HumanBodyBones.RightLowerArm},
        {3, HumanBodyBones.RightHand},
        //{3, HumanBodyBones.RightUpperLeg},
        //{4, HumanBodyBones.RightLowerLeg},
        //{5, HumanBodyBones.RightFoot},
        {4, HumanBodyBones.LeftShoulder},
        {5, HumanBodyBones.LeftLowerArm},
        {6, HumanBodyBones.LeftHand},       
        //{9, HumanBodyBones.LeftUpperLeg},
        //{10, HumanBodyBones.LeftLowerLeg},
        //{11, HumanBodyBones.LeftFoot},

    };

    private readonly Dictionary<int, HumanBodyBones> mirroredBoneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
    {
        {0, HumanBodyBones.Spine},
        {1, HumanBodyBones.LeftShoulder},
        {2, HumanBodyBones.LeftLowerArm},
        {3, HumanBodyBones.LeftHand},
        //{3, HumanBodyBones.LeftUpperLeg},
        //{4, HumanBodyBones.LeftLowerLeg},
        //{5, HumanBodyBones.LeftFoot},
        {4, HumanBodyBones.RightShoulder},
        {5, HumanBodyBones.RightLowerArm},
        {6, HumanBodyBones.RightHand},
        //{9, HumanBodyBones.RightUpperLeg},
        //{10, HumanBodyBones.RightLowerLeg},
        //{11, HumanBodyBones.RightFoot},

    };

    protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2JointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
    {
        {0, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
        {1, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
        {2, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
        {3, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
        //{3, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
        //{4, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
        //{5, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
        {4, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
        {5, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
        {6, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
        //{9, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
        //{10, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
        //{11, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},

    };

    protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
    {
        {4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
        {9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
    };

    protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2MirrorJointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
    {
        {0, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
        {1, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
        {2, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
        {3, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
        //{3, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
        //{4, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
        //{5, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
        {4, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
        {5, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
        {6, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
        //{9, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
        //{10, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
        //{11, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
    };

    protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
    {
        {4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
        {9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
    };

    public static void GetNeckAngle(FacetrackingManager facetrackingManager, ref Vector3 Neck)
    {

        float angleX = 0;
        float angleY = 0;
        float angleZ = 0;

        if (facetrackingManager == null)
        {
            facetrackingManager = FacetrackingManager.Instance;
        }

        if (facetrackingManager && facetrackingManager.IsTrackingFace())
        {


            angleX = facetrackingManager.GetHeadRotation().eulerAngles.x;
            angleY = facetrackingManager.GetHeadRotation().eulerAngles.y;
            angleZ = facetrackingManager.GetHeadRotation().eulerAngles.z;

            Neck = new Vector3(-angleX, -angleZ, -angleY);

        }
    }

    public void Add(ref Transform[] bone, Transform joint)
    {
        Transform[] tempArray = new Transform[bone.Length];
        for (int ix = 0; ix < tempArray.Length; ++ix)
        {
            tempArray[ix] = bone[ix];
        }

        bone = new Transform[bone.Length + 1];
        for (int ix = 0; ix < tempArray.Length; ++ix)
        {
            bone[ix] = tempArray[ix];
        }

        bone[bone.Length - 1] = joint;
    }
}