using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerkeleyManager : MonoBehaviour
{
    public static BerkeleyManager Instance { get; private set; }
    public bool berkeleyCapped;
    public bool monsterCapped;
    public bool rsrcCapped;
    public bool npcCapped;

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
    }

    // Update is called once per frame
    void Update()
    {
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
}
