using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameLib : MonoBehaviour
{
    public static GameLib Instance { get; private set; }
    [SerializeField]
    public Equipment[] allEquipments;
    [SerializeField]
    public Weapon[] allWeapons;
    [SerializeField]
    public Consumable[] allConsumables;
    [SerializeField]
    public Part[] allParts;


    [SerializeField]
    public BodyLook[] allBodyParts;
    public Color[] skinColorPresets;
    
    [SerializeField]
    public MonsterNPC[] allMonsters;

    
    
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
    public T GetItemById<T>(int id) {
        switch (typeof(T)){
            case
                var cls when cls == typeof(Equipment):
                {
                    return (T) Convert.ChangeType(GetEquipmentById(id), typeof(T)); 
                }
            case
                var cls when cls == typeof(Weapon):
                {
                    return (T) Convert.ChangeType(GetWeaponById(id), typeof(T)); 
                }
            case
                var cls when cls == typeof(Part):
                {
                    return (T) Convert.ChangeType(GetPartById(id), typeof(T)); 
                }
        }
        return default(T);
    }
     public Item GetItemByType(int thisId, ItemType thisType) {
        switch (thisType){
            case (ItemType.Equipment):
                {
                    return (Item) GetEquipmentById(thisId); 
                }
            case (ItemType.Weapon):
                {
                    return (Item) Convert.ChangeType(GetWeaponById(thisId), typeof(Item)); 
                }
            case (ItemType.Part):
                {
                    return (Item) GetPartById(thisId); 
                }
        }
        return default(Item);
    }
    public Equipment GetEquipmentById(int id) {
        return Array.Find(allEquipments, equipment => equipment.id == id);
    }
    public Weapon GetWeaponById(int id) {
        return Array.Find(allWeapons, weapon => weapon.id == id);
    }
    public Part GetPartById(int id) {
        return Array.Find(allParts, part => part.id == id);
    }
    public Consumable GetConsumableById(int id) {
        return Array.Find(allConsumables, co => co.id == id);
    }


    public BodyLook GetBodyPartById(int id) {
        return Array.Find(allBodyParts, bp => bp.id == id);
    }

}
