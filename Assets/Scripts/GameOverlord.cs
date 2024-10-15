using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameOverlord : MonoBehaviour
{
    public static GameOverlord Instance { get; private set; }
    public bool gameOver = false;
    public GameObject damagePrefab;
    public GameObject itemDropPrefab;
    public GameObject deathPrefab;
    public GameObject lvlUpPrefab;
    public GameObject namePlate;
    public GameObject soundBox;
    public List<GameObject> nearbyMonsters;
    public List<GameObject> nearbyDrops;
    public List<GameObject> nearbyRsrc;
    public Sprite maleChest;
    public Sprite femaleChest;
    public List<CurrentCharStat> defaultCharacterStats;
    
    public int progress = 0;
    public int currentRegion = 0;
    
    /* Factions
    0 - PlayerParty  |  1 - GoblinInvaders
    2 - NeutralNPCS  |  3 - AggressiveNPCs
    4 - Regional     |  5 - Mimics
    */
    [SerializeField]
    public int[] factionsThatSeekOutFight;


    private string[] excludeTags = new string[]{"Character", "Monster", "Hitbox", "Projectile", "Mimic", "Regional", "Marauder", "Npc"};
    public string[] fightingBerkeleyTags = new string[]{"Monster", "Mimic", "Regional", "Marauder"};

    // Singleton stuff
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
        nearbyMonsters = new List<GameObject>();
        nearbyDrops = new List<GameObject>();
        nearbyRsrc = new List<GameObject>();
    }
    void Start()
    {
        
        // Player.Instance.AddEquipment(4);
       
        // Player.Instance.AddPart(900002);
        // Player.Instance.AddPart(900007);
        
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            UIManager.Instance.OpenMenu(Menu.Inventory);
            UIManager.Instance.SetOpenTab(Tab.Armor);
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            UIManager.Instance.OpenMenu(Menu.Inventory);
            UIManager.Instance.SetOpenTab(Tab.Weapons);
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            UIManager.Instance.OpenMenu(Menu.Inventory);
            UIManager.Instance.SetOpenTab(Tab.Crafting);
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            UIManager.Instance.OpenMenu(Menu.Inventory);
            UIManager.Instance.SetOpenTab(Tab.Skills);
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            UIManager.Instance.OpenMenu(Menu.System);
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            Debug.Log(GameLib.Instance.GenerateName(true));
            Debug.Log(GameLib.Instance.GenerateName(false));
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            Player.Instance.GainExperience(20);
             Player.Instance.AddEquipment(1);
            Player.Instance.AddEquipment(1);
            Player.Instance.AddEquipment(2);
            Player.Instance.AddEquipment(3);
            Player.Instance.AddEquipment(3);
            Player.Instance.AddEquipment(4);
            Player.Instance.AddEquipment(4);
            Player.Instance.AddEquipment(5);
            Player.Instance.AddEquipment(5);
            Player.Instance.AddPart(900000);
            Player.Instance.AddPart(900001);
            Player.Instance.AddPart(900003);
            Player.Instance.AddPart(900002);
            // Player.Instance.AddPart(900000);
            Player.Instance.AddPart(900004);
            Player.Instance.AddPart(900006);
            Player.Instance.AddPart(900008);
            Player.Instance.AddPart(900010);
            Player.Instance.AddPart(900012);
            Player.Instance.AddPart(900009);Player.Instance.AddPart(900009);Player.Instance.AddPart(900009);Player.Instance.AddPart(900009);Player.Instance.AddPart(900009);Player.Instance.AddPart(900009);
            Player.Instance.AddConsumable(800000);Player.Instance.AddConsumable(800000);Player.Instance.AddConsumable(800000);Player.Instance.AddConsumable(800000);Player.Instance.AddConsumable(800000);Player.Instance.AddConsumable(800000);Player.Instance.AddConsumable(800000);
        }
    }
    public Vector2 Pathfind(Vector2 from, Vector2 target, bool capDist = true) {
        float dist = Vector2.Distance(from, target);
        // if (capDist && dist > 950f) dist = 950f;
        // dist*=1.1f;
        Vector2 dir  = target - from;
        dir.Normalize();
        
        bool canGo = TestForward(from, dir, dist);
        if (canGo) return target;

        float angle = AngleFromVector2(dir);
        float degrees = angle;

        if (canGo) return from + dir;

        degrees += 20f;
        // while (canGo == false) {
            dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        // }
        if (canGo) return from + dir;



        degrees += -60f; // -40
        dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        // }
        if (canGo) return from + dir;



        degrees += 120f; // 80
        dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        if (canGo) return from + dir;

        degrees = angle-120f;
        dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        if (canGo) return from + dir;
        degrees = angle+150f; // = 90
        dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        if (canGo) return from + dir;

        degrees = angle-180f; // = -90
        dir = Vector2FromAngle(degrees );

        return from + dir;
    }
    public bool TestForward(Vector2 from, Vector2 dir, float dist) {
        RaycastHit2D hit = Physics2D.Raycast(from, dir, dist);
        Debug.DrawRay(from, dir, Color.red);
        if (hit.transform) {
            
            if (hit.transform.gameObject.name != "road"
                && Array.IndexOf(excludeTags, hit.transform.gameObject.tag) == -1 ) {
                return false;
            }
        }
        // can go forward
        return true;
    }
    public Vector2 Vector2FromAngle(float a)
     {
         a *= Mathf.Deg2Rad;
         return new Vector2(Mathf.Sin(a), Mathf.Cos(a));
     }
      public float AngleFromVector2(Vector2 v)
     {
         return Mathf.Rad2Deg *Mathf.Atan2(v.x, v.y);
     }
     public void GameOver() {
        gameOver = true;
        Destroy(MapMaker.Instance);
        Destroy(BerkeleyManager.Instance);
        Destroy(Player.Instance);
        // Save system catches tihs
     }

     public void ChangeRegion(int regionId) {
        Region region = GameLib.Instance.regions[regionId];
        Debug.Log("\n CHANGING TO "+region.name);
        currentRegion = regionId;
        
        for (int i = 0; i < Player.Instance.characters.Count; i++)
        {
            Player.Instance.characters[i].controller.transform.position = new Vector2(843.5f + i*10f,840f);
        }
        foreach(Transform child in MapMaker.Instance.transform)
        {
            if (child.gameObject.tag != "Finish")
            Destroy(child.gameObject);
        }
        UIManager.Instance.villageLocations = new List<Vector2>();
        MapMaker.Instance.DoStart();
        Camera.main.GetComponent<Camera>().backgroundColor  = region.floorColor;
        UIManager.Instance.HideRegionTransition();
        
        foreach(Transform child in MapMaker.Instance.transform.Find("grass"))
        {
            child.gameObject.GetComponent<SpriteRenderer>().sprite = region.groundTexture;
        }
     }
}
