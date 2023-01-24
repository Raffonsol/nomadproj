using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public List<GameObject> peopleInDetection;
    // Start is called before the first frame update
    void Start()
    {
        peopleInDetection = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collided)
    {
        if (collided.gameObject.CompareTag("Character")) {
            peopleInDetection.Add(collided.gameObject);
        }
    }
    void OnTriggerExit2D(Collider2D collided)
    {
        if (collided.gameObject.CompareTag("Character")) {
            peopleInDetection.Remove(collided.gameObject);
        }
        
    }
	
}
