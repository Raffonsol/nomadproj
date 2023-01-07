using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BerkeleyType
{
    Rsrc,
    Monster,
    Npc,
}
[Serializable]
public class BerkeleySpawnable {
    public GameObject obj;
    public float spawnTime;
    public float spawnTimer;
    public BerkeleyType berkeleyType;
}
public class BerkeleyManager : MonoBehaviour
{
    public static BerkeleyManager Instance { get; private set; }
    public float mapBounds = 0;
    public float disappearDistance = 80f;
    public bool berkeleyCapped;
    public bool monsterCapped;
    public bool rsrcCapped;
    public bool npcCapped;
    public int monsterGoingId = 0;
    public int friendlyGoingId = 4;

    [SerializeField]
    List<BerkeleySpawnable> spawnables;

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
    void Start()
    {
        // TODO replace with glboal settings
        berkeleyMax = 600;
        monsterMax = 100;
        rsrcMax = 250;
        npcMax = 50;

        checkTimer = checkTime;
        for(int i = 0; i <spawnables.Count; i++){
            spawnables[i].spawnTimer = spawnables[i].spawnTime;
        }
        
        for(int i = 0; i <50; i++){
            // hardcoded add of first spawnable 50 time. Use for trees
            Spawn(spawnables[0].obj);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i <spawnables.Count; i++){
            if (spawnables[i].spawnTimer > 0) {
                spawnables[i].spawnTimer -= Time.deltaTime;
            } else {
                if ((spawnables[i].berkeleyType == BerkeleyType.Rsrc && !rsrcCapped)
                  ||(spawnables[i].berkeleyType == BerkeleyType.Monster && !monsterCapped)
                  ||(spawnables[i].berkeleyType == BerkeleyType.Npc && !npcCapped)
                  )
                Spawn(spawnables[i].obj);
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
    void Spawn(GameObject obj) {
        Vector2 CamPos = Camera.main.transform.position;
        float x = UnityEngine.Random.Range(CamPos.x-disappearDistance, CamPos.x+disappearDistance);
        while (Math.Abs(x - Camera.main.transform.position.x) < 10) 
            x = UnityEngine.Random.Range(CamPos.x-disappearDistance, CamPos.x+disappearDistance);
        
        float y = UnityEngine.Random.Range(CamPos.y-disappearDistance, CamPos.y+disappearDistance);
        while (Math.Abs(y - Camera.main.transform.position.y) < 10) 
            y = UnityEngine.Random.Range(CamPos.y-disappearDistance, CamPos.y+disappearDistance);

        Instantiate(obj, new Vector2(x, y), Quaternion.Euler(0,0,UnityEngine.Random.Range(0,360)));
        // Debug.Log("Spawning tree at " +x +","+y);
    }

    public int LatestFriendId() {
        friendlyGoingId++;
        return friendlyGoingId;
    }
}
