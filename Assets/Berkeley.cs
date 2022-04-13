using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berkeley : MonoBehaviour
{
    private float disappearDistance;
    // Start is called before the first frame update
    void Awake()
    {
        // TODO replace with global setting
        disappearDistance = 100f;
    }

    // Update is called once per frame
    void Update()
    {   
        // If we are too far, delete us. Get it? Berkeley..
        if (disappearDistance != null && Vector3.Distance(transform.position, Camera.main.transform.position) > disappearDistance) {
            Destroy(gameObject);
        }
    }
}
