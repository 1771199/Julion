using UnityEngine;

[System.Serializable]
public class JointAngle
{
    private float size;

    [SerializeField]
    private Quaternion[] angle;


    public void SetSize()
    {
        size = angle.Length;
    }

    public int Length
    {
        get { return angle == null ? -1 : angle.Length; }
    }

    public Quaternion Angle(int index)
    {
        return angle[index];
    }


    public Quaternion this[int index]
    {
        get { return angle[index]; }
        set { angle[index] = value; }
    }

    public void Add(Quaternion jointAngle)
    {
        if (angle == null)
        {

            angle = new Quaternion[1];
            angle[0] = jointAngle;

            return;
        }

        Quaternion[] tempArray = new Quaternion[angle.Length];
        for (int ix = 0; ix < tempArray.Length; ++ix)
        {
            tempArray[ix] = angle[ix];
        }

        angle = new Quaternion[angle.Length + 1];
        for (int ix = 0; ix < tempArray.Length; ++ix)
        {
            angle[ix] = tempArray[ix];
        }

        angle[angle.Length - 1] = jointAngle;
    }

    public void Clear()
    {
        System.Array.Clear(angle, 0, angle.Length);
    }




}