using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleMessenger : MonoBehaviour
{
    [SerializeField]
    private Transform[] cdBone;
    private Transform cdNeck;
    [SerializeField]
    private static Transform[] lionBone, lion3Bone, lion2Bone;
    Vector3 beforeAngle, gapAngle, presentAngle, neckVector, beforeVector, spine = Vector3.zero;
    public GameObject lion2, lion3;
    int axis = 0;
    int speedvalue = 0;
    float motionRange = 1.0f;
    static JointAngleArray jointAngleArray;
    public CollisionManager collisionManager;
    private AvatarController avatarController = new AvatarController();


    // Start is called before the first frame update
    void Start()
    {
        cdBone = AvatarController.GetBones;
        cdNeck = AvatarController.GetNeck;
        avatarController.Add(ref cdBone, cdNeck);
        lionBone = LionController.GetLionBones;
        lion3Bone = LionController3.GetLion3Bones;
        lion2Bone = LionController2.GetLion2Bones;

    }

    // Update is called once per frame
    void Update()
    {
        SendAngle();
    }

    void SendAngle()
    {
        for (int i = 0; i < 7; i++)
        {
            if (i == 0) //프레임마다 shouldercenter 값 전달
            {
                float shouldercenter = cdBone[i].localRotation.eulerAngles.z;
                spine = new Vector3(lionBone[i].localRotation.eulerAngles.x, lionBone[i].localRotation.eulerAngles.y, shouldercenter);
                if (!StateUpdater.PlaySong2 && !StateUpdater.PlaySong3)
                    lionBone[i].localRotation = Quaternion.Euler(spine);
                if (StateUpdater.PlaySong2)
                {
                    lion2Bone[i].localRotation = Quaternion.Euler(spine);
                }
                if (StateUpdater.PlaySong3)
                {
                    lion3Bone[i].localRotation = Quaternion.Euler(spine);
                }


            }

            if (!collisionManager.collisions[i])
            {
                switch (i)
                {
                    case 0:
                        if (!StateUpdater.PlaySong2 && !StateUpdater.PlaySong3)
                            lionBone[1].localRotation = cdBone[1].localRotation;
                        if (StateUpdater.PlaySong2)
                        {
                            lion2Bone[1].localRotation = cdBone[1].localRotation;
                        }
                        if (StateUpdater.PlaySong3)
                        {
                            lion3Bone[1].localRotation = cdBone[1].localRotation;
                        }
                        break;
                    case 1:
                        if (!collisionManager.collisions[6] && !(collisionManager.collisions[7] || collisionManager.collisions[8] || collisionManager.collisions[9] || collisionManager.collisions[10]))
                        {
                            if (!StateUpdater.PlaySong2 && !StateUpdater.PlaySong3)
                                lionBone[2].localRotation = cdBone[2].localRotation;
                            if (StateUpdater.PlaySong2)
                            {
                                lion2Bone[2].localRotation = cdBone[2].localRotation;
                            }
                            if (StateUpdater.PlaySong3)
                            {
                                lion3Bone[2].localRotation = cdBone[2].localRotation;
                            }
                        }
                        break;
                    case 2:
                        if (!StateUpdater.PlaySong2 && !StateUpdater.PlaySong3)
                            lionBone[3].localRotation = cdBone[3].localRotation;
                        if (StateUpdater.PlaySong2)
                        {
                            lion2Bone[3].localRotation = cdBone[3].localRotation;
                        }
                        if (StateUpdater.PlaySong3)
                        {
                            lion3Bone[3].localRotation = cdBone[3].localRotation;
                        }
                        break;
                    case 3:
                        if (!StateUpdater.PlaySong2 && !StateUpdater.PlaySong3)
                            lionBone[4].localRotation = cdBone[4].localRotation;
                        if (StateUpdater.PlaySong2)
                        {
                            lion2Bone[4].localRotation = cdBone[4].localRotation;
                        }
                        if (StateUpdater.PlaySong3)
                        {
                            lion3Bone[4].localRotation = cdBone[4].localRotation;
                        }
                        break;
                    case 4:
                        if (!collisionManager.collisions[6] && !(collisionManager.collisions[11] || collisionManager.collisions[12] || collisionManager.collisions[13] || collisionManager.collisions[14]))
                        {
                            if (!StateUpdater.PlaySong2 && !StateUpdater.PlaySong3)
                                lionBone[5].localRotation = cdBone[5].localRotation;
                            if (StateUpdater.PlaySong2)
                            {
                                lion2Bone[5].localRotation = cdBone[5].localRotation;
                            }
                            if (StateUpdater.PlaySong3)
                            {
                                lion3Bone[5].localRotation = cdBone[5].localRotation;
                            }

                        }
                        break;
                    case 5:
                        if (!StateUpdater.PlaySong2 && !StateUpdater.PlaySong3)
                            lionBone[6].localRotation = cdBone[6].localRotation;
                        if (StateUpdater.PlaySong2)
                        {
                            lion2Bone[6].localRotation = cdBone[6].localRotation;
                        }
                        if (StateUpdater.PlaySong3)
                        {
                            lion3Bone[6].localRotation = cdBone[6].localRotation;
                        }
                        break;
                    case 6:
                        if (!StateUpdater.PlaySong2 && !StateUpdater.PlaySong3)
                            lionBone[7].localRotation = cdBone[7].localRotation;
                        if (StateUpdater.PlaySong2)
                        {
                            lion2Bone[7].localRotation = cdBone[7].localRotation;
                        }
                        if (StateUpdater.PlaySong3)
                        {
                            lion3Bone[7].localRotation = cdBone[7].localRotation;
                        }
                        break;

                }


            }


        }
    }
}