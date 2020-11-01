using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LionController3 : MonoBehaviour
{
    public Transform[] bones;
    static Transform[] lion3Bones;
    public static Transform[] GetLion3Bones { get { return lion3Bones; } }

    // Start is called before the first frame update
    void Start()
    {
        lion3Bones = bones;
    }
}
