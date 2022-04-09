using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverlord : MonoBehaviour
{
    public static GameOverlord Instance { get; private set; }
    public GameObject damagePrefab;
    public GameObject deathPrefab;
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
        Player.Instance.EquipWeapon(100000, new List<Part>());
        Player.Instance.AddPart(900001);
        Player.Instance.AddPart(900000);
        Player.Instance.AddPart(900002);
        Player.Instance.AddPart(900003);
        Player.Instance.AddPart(900001);
        Player.Instance.AddPart(900000);
        Player.Instance.AddPart(900002);
        Player.Instance.AddPart(900003);
    }
}
