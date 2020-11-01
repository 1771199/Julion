using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LionController2 : MonoBehaviour
{
    public Transform[] Bones;
    static Transform[] lion2Bones;
    public static Transform[] GetLion2Bones { get { return lion2Bones; } }

    // Start is called before the first frame update
    void Start()
    {
        lion2Bones = Bones;
    }

    
}
