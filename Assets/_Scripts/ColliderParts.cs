using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderParts : MonoBehaviour
{
    public bool collision = false;

    private void OnTriggerEnter(Collider other)
    {
        collision = true;
    }

    private void OnTriggerStay(Collider other)
    {
        collision = true;
    }

    private void OnTriggerExit(Collider other)
    {
        collision = false;
    }
}
