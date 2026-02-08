using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum BerkeleyType
{
    Rsrc,
    Monster,
    Npc,
}
[Serializable]
public class BerkeleySpawnable {
    public int id;
    public bool globalSpawn = false;
    public GameObject obj;
    public float spawnTime;
    public float spawnTimer;
    public BerkeleyType berkeleyType;
    public int limit;
    public int currentQuantity;
    public int minLevel;
    public int cluster = 1; 
    public bool notOnRiverOrRoad=false;
    public int[] regions;
}
public class BerkeleyManager : MonoBehaviour
{
    public static BerkeleyManager Instance { get; private set; }
    public float mapBounds = 0;
    public float disappearDistance = 75f;
    public bool berkeleyCapped;
    public bool monsterCapped;
    public bool rsrcCapped;
    public bool npcCapped;
    public int monsterGoingId = 0;
    public int friendlyGoingId = 4;

    [SerializeField]
    public List<BerkeleySpawnable> spawnables;

    private int berkeleyMax;
    private int monsterMax;
    private int rsrcMax;
    private int npcMax;

    private float checkTime = 0.6f;
    private float checkTimer;
    private void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }

    
    // Start is called before the first frame update
    public void DoStart()
    {
        // TODO replace with glboal settings
        berkeleyMax = 250;
        monsterMax = 10;
        rsrcMax = 200;
        npcMax = 5;

        checkTimer = checkTime;
        for(int i = 0; i <spawnables.Count; i++){
            spawnables[i].spawnTimer = spawnables[i].spawnTime;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i <spawnables.Count; i++){
            if (!spawnables[i].globalSpawn || (spawnables[i].regions.Length>0 && !spawnables[i].regions.Contains(GameOverlord.Instance.currentRegion))) continue;
            if (spawnables[i].spawnTimer > 0) {
                spawnables[i].spawnTimer -= Time.deltaTime;
            } else {
                if (((spawnables[i].berkeleyType == BerkeleyType.Rsrc && !rsrcCapped)
                  ||(spawnables[i].berkeleyType == BerkeleyType.Monster && !monsterCapped)
                  ||(spawnables[i].berkeleyType == BerkeleyType.Npc && !npcCapped))
                  && spawnables[i].limit > spawnables[i].currentQuantity
                  && Player.Instance.playerLevel >=  spawnables[i].minLevel
                  ) {
                        if (i==24)// HARDOCDED zombie script
                        {   
                            VillageManager.Instance.SpawnNpc(transform.position.x,transform.position.y,spawnables[i].obj);
                        }
                        else
                            Spawn(spawnables[i], spawnables[i].id);
                    }
                spawnables[i].spawnTimer = spawnables[i].spawnTime;
            }
        }
        if (checkTimer > 0) {
            checkTimer -= Time.deltaTime;
        } else {
            Check();
            checkTimer = checkTime;
        }
        
    }
    void Check()
    {
        int monsterL = GameObject.FindGameObjectsWithTag("Monster").Length;
        int rsrcL = GameObject.FindGameObjectsWithTag("Rsrc").Length;
        int npcL = GameObject.FindGameObjectsWithTag("Npc").Length;

        berkeleyCapped = GameObject.FindGameObjectsWithTag("Berkeley").Length + monsterL + rsrcL + npcL > berkeleyMax;
        monsterCapped = berkeleyCapped || monsterL > monsterMax;

        rsrcCapped = berkeleyCapped || rsrcL > rsrcMax;
        npcCapped = berkeleyCapped || npcL > npcMax;
    }
    // spawning also happens in possibleSpawn system
    void Spawn(BerkeleySpawnable spawn, int spawnableId, int attempt=0) {
        Vector2 CamPos = Camera.main.transform.position;
        float x = UnityEngine.Random.Range(CamPos.x-disappearDistance, CamPos.x+disappearDistance);
        float y = UnityEngine.Random.Range(CamPos.y-disappearDistance, CamPos.y+disappearDistance);
        while (Math.Abs(x - Camera.main.transform.position.x) < 7 && Math.Abs(y - Camera.main.transform.position.y) < 7) {
            x = UnityEngine.Random.Range(CamPos.x-disappearDistance, CamPos.x+disappearDistance);
            y = UnityEngine.Random.Range(CamPos.y-disappearDistance, CamPos.y+disappearDistance);
        }
        
        Tile tile = MapMaker.Instance.GetTileAtCoordinates(x,y);
        if (spawn.berkeleyType!=BerkeleyType.Monster &&
            (tile == null || tile.controller==null || tile.controller.contentCurrent>=tile.controller.contentLimit)) {
            if (attempt>5)return;
            Spawn(spawn, spawn.id, 1+attempt); // Retry
        } else {
            // actually spawning
            if (spawn.berkeleyType!=BerkeleyType.Monster) tile.controller.contentCurrent++;
            GameObject inst = Instantiate(spawn.obj, new Vector2(x, y), Quaternion.Euler(0,0,UnityEngine.Random.Range(0,360)));
             inst.transform.parent = MapMaker.Instance.transform;
            // Debug.Log("Spawning tree at " +x +","+y);
            // inst.GetComponent<Berkeley>().spawnableId = spawnableId;
        }
    }

    public int LatestFriendId() {
        friendlyGoingId++;
        return friendlyGoingId;
    }
}
