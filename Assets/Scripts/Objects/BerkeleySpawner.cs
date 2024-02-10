using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerkeleySpawner : MonoBehaviour
{
    [SerializeField]
    public List<int> spawnies;

    public GameObject door;
    private List<BerkeleySpawnable> spawnables;
    // Start is called before the first frame update
    void Awake()
    {
        spawnables = new List<BerkeleySpawnable>();
        for (int i = 0; i<spawnies.Count; i++) {
            spawnables.Add(BerkeleyManager.Instance.spawnables[spawnies[i]]);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i <spawnables.Count; i++){
            if (spawnables[i].spawnTimer > 0) {
                spawnables[i].spawnTimer -= Time.deltaTime;
            } else {
                if (((spawnables[i].berkeleyType == BerkeleyType.Rsrc && !BerkeleyManager.Instance.rsrcCapped)
                  ||(spawnables[i].berkeleyType == BerkeleyType.Monster && !BerkeleyManager.Instance.monsterCapped)
                  ||(spawnables[i].berkeleyType == BerkeleyType.Npc && !BerkeleyManager.Instance.npcCapped))
                  && spawnables[i].limit > spawnables[i].currentQuantity){
                  spawnables[i].currentQuantity++;
                  if (spawnables[i].berkeleyType == BerkeleyType.Npc) {
                    VillageManager.Instance.SpawnNpc(transform.position.x,transform.position.y);
                  } else {
                    Spawn(spawnables[i].obj, spawnables[i].id);
                  }
                  spawnables[i].spawnTimer = spawnables[i].spawnTime;
                }
            }
        }
    }
    void Spawn(GameObject obj, int spawnableId) {
        GameObject inst = Instantiate(obj, new Vector2(door.transform.position.x, door.transform.position.y), Quaternion.Euler(0,0,UnityEngine.Random.Range(0,360)));
        inst.name = inst.name + BerkeleyManager.Instance.monsterGoingId++;
        inst.GetComponent<Berkeley>().spawnableId = spawnableId;
    }
}
