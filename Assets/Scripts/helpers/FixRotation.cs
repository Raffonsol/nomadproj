using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRotation : MonoBehaviour
{
    Quaternion rotation;
    void Awake()
    {
        rotation = transform.rotation;
    }
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        // transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}
