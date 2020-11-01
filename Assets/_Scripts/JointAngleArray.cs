using UnityEngine;

[System.Serializable]
public class JointAngleArray
{
    [SerializeField]
    public string commend;

    [SerializeField]
    private JointAngle[] JointAngles;

    public int Length
    {
        get { return JointAngles == null ? -1 : JointAngles.Length; }
    }

    public JointAngle this[int index]
    {
        get { return JointAngles == null ? null : JointAngles[index]; }
        set { JointAngles[index] = value; }
    }

    public void Add(JointAngle newData)
    {
        if (JointAngles == null)
        {
            JointAngles = new JointAngle[1];
            JointAngles[0] = newData;

            return;
        }

        JointAngle[] tempJointAngles = new JointAngle[JointAngles.Length];
        for (int ix = 0; ix < tempJointAngles.Length; ++ix)
        {
            tempJointAngles[ix] = JointAngles[ix];
        }

        JointAngles = new JointAngle[JointAngles.Length + 1];
        for (int ix = 0; ix < tempJointAngles.Length; ++ix)
        {
            JointAngles[ix] = tempJointAngles[ix];
        }

        JointAngles[JointAngles.Length - 1] = newData;
    }

    public void Clear()
    {
        System.Array.Clear(JointAngles, 0, JointAngles.Length);
    }


}
