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

    public bool hasSpawn = false;
    public float checkTimer = 2f;

    private bool randomValuesGenerated = false;
    private List<int> randomValues;
    // Start is called before the first frame update
    void Start() {
        randomValues = new List<int>();
    }
    void Spawn()
    {
        for(int i = 0; i <spawnList.Length; i++){
            int randomValue =0; 
            if (randomValuesGenerated) {
                randomValue = randomValues[i];
            } else {
                randomValue = UnityEngine.Random.Range(0,101);
                randomValues.Add(randomValue);
            }
            if (randomValue <= spawnList[i].chance) {
                GameObject spawn = Instantiate(spawnList[i].spawn, transform.position, transform.rotation);
                spawn.GetComponent<Berkeley>().indistructible = true;
                spawn.transform.parent = MapMaker.Instance.transform;
                return;
            }
        }
        randomValuesGenerated = true;
        // Debug.Log("Village Spawned");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasSpawn) {
            if (checkTimer > 0) {
                checkTimer-=Time.deltaTime;
            } else {
                checkTimer=2f;
                float disappearDistance = BerkeleyManager.Instance.disappearDistance;
                if (Vector3.Distance(transform.position, Camera.main.transform.position) < disappearDistance) {
                    hasSpawn=true;
                    Spawn();
                }
            }
        } else {
            if (checkTimer > 0) {
                checkTimer-=Time.deltaTime;
            } else {
                checkTimer=2f;
                float disappearDistance = BerkeleyManager.Instance.disappearDistance;
                if (Vector3.Distance(transform.position, Camera.main.transform.position) > disappearDistance) {
                    hasSpawn=false;
                }
            }
        }
    }
}
