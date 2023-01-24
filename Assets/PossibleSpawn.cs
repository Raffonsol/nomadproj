using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SpawnObject {
    public GameObject spawn;
    public int chance;
}
public class PossibleSpawn : MonoBehaviour
{
    [SerializeField]
    public SpawnObject[] spawnList;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i <spawnList.Length; i++){
            if (UnityEngine.Random.Range(0,101) <= spawnList[i].chance) {
                Instantiate(spawnList[i].spawn, transform.position, transform.rotation);
                return;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
