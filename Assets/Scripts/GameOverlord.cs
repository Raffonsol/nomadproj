using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameOverlord : MonoBehaviour
{
    public static GameOverlord Instance { get; private set; }
    public GameObject damagePrefab;
    public GameObject itemDropPrefab;
    public GameObject deathPrefab;
    public List<GameObject> nearbyMonsters;
    public Sprite maleChest;
    public Sprite femaleChest;

    private string[] excludeTags = new string[]{"Character", "Monster", "Hitbox"};

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
    }
    void Start()
    {
        
        // Player.Instance.AddPart(900001);
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
        Player.Instance.AddPart(900002);
        Player.Instance.AddPart(900003);
        Player.Instance.AddPart(900001);
        Player.Instance.AddPart(900000);
        // Player.Instance.AddPart(900002);
        // Player.Instance.AddPart(900003);
    }
    public void Log(string log) {
        Debug.Log(log);
    }
    public Vector2 Pathfind(Vector2 from, Vector2 target, bool capDist = true) {
        float dist = Vector2.Distance(from, target);
        if (capDist && dist > 150) dist = 150;
        Vector2 dir  = target - from;
        dir.Normalize();
        
        bool canGo = TestForward(from, dir, dist);
        if (canGo) return target;

        float degrees = AngleFromVector2(dir);
        
        Vector2 dif = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        dif -= from;
        dif.Normalize();


        // while (canGo == false) {
            dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        // }
        if (canGo) return from + dir;

        degrees += 20f;
        // while (canGo == false) {
            dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        // }
        if (canGo) return from + dir;



        degrees += -40f;
        dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        // }
        if (canGo) return from + dir;



        degrees += 80f;
        dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        if (canGo) return from + dir;

        degrees -= 120f;
        dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
        if (canGo) return from + dir;
        // degrees -= 40f;
        // dir = Vector2FromAngle(degrees );
        //     canGo = TestForward(from, dir, dist);
        // if (canGo) return from + dir;

        degrees -= 120f;
        dir = Vector2FromAngle(degrees );
            canGo = TestForward(from, dir, dist);
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
}
