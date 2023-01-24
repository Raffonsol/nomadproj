using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerkeleySpawner : MonoBehaviour
{
    [SerializeField]
    public List<BerkeleySpawnable> spawnables;

    public GameObject door;
    // Start is called before the first frame update
    void Start()
    {
        // for(int i = 0; i <spawnables.Count; i++){
        //     spawnables[i].spawnTimer = spawnables[i].spawnTime;
        // }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i <spawnables.Count; i++){
            if (spawnables[i].spawnTimer > 0) {
                spawnables[i].spawnTimer -= Time.deltaTime;
            } else {
                if ((spawnables[i].berkeleyType == BerkeleyType.Rsrc && !BerkeleyManager.Instance.rsrcCapped)
                  ||(spawnables[i].berkeleyType == BerkeleyType.Monster && !BerkeleyManager.Instance.monsterCapped)
                  ||(spawnables[i].berkeleyType == BerkeleyType.Npc && !BerkeleyManager.Instance.npcCapped)
                  )
                  if (spawnables[i].berkeleyType == BerkeleyType.Npc) {
                    VillageManager.Instance.SpawnNpc(transform.position.x,transform.position.y);
                  } else {
                    Spawn(spawnables[i].obj);
                  }
                  spawnables[i].spawnTimer = spawnables[i].spawnTime;
            }
        }
    }
    void Spawn(GameObject obj) {
        GameObject inst = Instantiate(obj, new Vector2(door.transform.position.x, door.transform.position.y), Quaternion.Euler(0,0,UnityEngine.Random.Range(0,360)));
        inst.name = inst.name + BerkeleyManager.Instance.monsterGoingId++;
    }
}
