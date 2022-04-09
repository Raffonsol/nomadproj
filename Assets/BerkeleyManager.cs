using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerkeleyManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // GameObject.FindGameObjectsWithTag("Character");
        // Debug.Log(GameObject.FindGameObjectsWithTag("Berkeley").Length);
        GameObject[] berkeleys = GameObject.FindGameObjectsWithTag("Berkeley");
        for(int i = 0; i <berkeleys.Length; i++){
            Debug.Log(berkeleys[i].name);
        }
        
    }
}
