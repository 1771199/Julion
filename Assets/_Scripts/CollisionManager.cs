using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public GameObject[] objects;
  
    public bool[] collisions;       //shoulderleft,elbowleft,shoulderright,elbowright


    private void Start()
    {
        collisions = new bool[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].AddComponent<ColliderParts>();

        }
    }

    private void Update()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            collisions[i] = objects[i].GetComponent<ColliderParts>().collision;
        }
    }

    public bool GetCollsion(int index)
    {
        return collisions[index];
    }

}
