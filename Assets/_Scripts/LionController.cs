using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LionController : MonoBehaviour
{
    public Transform[] Lionbone;
    static Transform[] Bones;

    public static Transform[] GetLionBones { get { return Bones; } }

    JointAngle jointAngle;
    static JointAngleArray jointAngleArray;

    float elapsedTime;

    private void Start()
    {
        Bones = Lionbone;
        jointAngleArray = null;
    }

    void Update()
    {
        if (StateUpdater.isRecording)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > 0.1)
            {
                MotionRecoding();
                elapsedTime = 0.0f;
            }
        }
    }

    public static void NewJointAngleArray()
    {
        jointAngleArray = new JointAngleArray();
    }

    public static JointAngleArray EditJointAngleArray
    {
        get { return jointAngleArray; }
        set { jointAngleArray = value; }
    }

    void MotionRecoding()
    {
        jointAngle = new JointAngle();
        for(int i = 0; i< Lionbone.Length; i++)
        {
            jointAngle.Add(Lionbone[i].localRotation);
        }
        jointAngleArray.Add(jointAngle);
    }

    public void SetZeroPose()
    {
        JointAngle jointAngle = new JointAngle();
        Vector3 vector = new Vector3();
        for (int i = 0; i < 8; i++)
        {
            switch (i)
            {
                case 1:
                    vector = new Vector3(0, 90, 0);
                    break;
                case 0:
                case 3:
                case 2:
                case 5:
                case 6:
                case 7:
                case 8:
                    vector = new Vector3(0, 0, 0);
                    break;
                case 4:
                    vector = new Vector3(0, -90, 0);
                    break;

            }
            jointAngle.Add(Quaternion.Euler(vector));
        }
        jointAngleArray.Add(jointAngle);

    }
}
