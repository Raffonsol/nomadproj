using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Equipped
{
    public Equipment head;
    public Equipment rightShoulder;
    public Equipment leftShoulder;
    public Equipment chest;
    public Equipment rightFoot;
    public Equipment leftFoot;
    public Equipment rightHand;
    public Equipment leftHand;

    public Weapon primaryWeapon;
    public Weapon secondaryWeapon;

    public List<Part> partsBeingUsed;

    public Equipped()
    {
        head = null;
        rightShoulder = null;
        leftShoulder = null;
        chest = null;
        rightFoot = null;
        leftFoot = null;
        rightHand = null;
        leftHand = null;

        partsBeingUsed = new List<Part>();
    }
}

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public ZombieController controller;

    public float maxLife;
    public float maxStamina;
    public float maxMana;

    public float maxWeight;

    public float life;
    public float stamina;
    public float mana;

    public float carryingWeight;
    public float invincibilityTime = 0.3f;

    public int level = 0;
    public int experience = 0;
    public int experienceToFirstLevel = 10;
    public int experienceIncrement = 5;
    

    [HideInInspector]
    public Equipped equipped;
    [HideInInspector]
    public HitBox hitbox;
    [HideInInspector]
    public float invincibleTimer;
    [HideInInspector]
    public int experienceToNextLevel;

    public List<Equipment> equipments;
    public List<Weapon> weapons;
    public List<Part> parts;

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
		ResetStats();
        CalculateNextLevel();
        equipped = new Equipped();
	}
    void Update()
    {
        if (invincibleTimer > -1f) {
            invincibleTimer -= Time.deltaTime;
        }
    }
    public void CalculateNextLevel()
    {
        experienceToNextLevel = experienceToFirstLevel + (experienceIncrement * level);
    }
    public void ResetStats()
    {
        invincibleTimer = invincibilityTime;
        life = maxLife;
        stamina = maxStamina;
        mana = maxMana;
        
    }
    public void EquipArmor(int itemId, string slotName)
    {
        Equipment value = GameLib.Instance.GetEqupmentById(itemId);
        // settings
        var propInfo = equipped.GetType().GetProperty(slotName);
        if (propInfo != null)
        {
            propInfo.SetValue(equipped, value, null);
        }
    }
    public void EquipWeapon(int itemId, List<Part> partsUsed)
    {
        Weapon value = GameLib.Instance.GetWeaponById(itemId);

        // settings
        if (equipped is null)        
        equipped = new Equipped();
        equipped.primaryWeapon = value;
        equipped.partsBeingUsed = partsUsed;
        
        /* graphics */
        // -- Weapon
        Destroy(transform.Find("Body/Instance/PrimaryWeapon").gameObject);
        GameObject newWeapon = Instantiate(value.visual);
        newWeapon.name = "PrimaryWeapon";
        newWeapon.transform.parent = transform.Find("Body/Instance");
        newWeapon.transform.localPosition = new Vector3(value.instance.weaponPos.x, value.instance.weaponPos.y, -9.3f);
        newWeapon.transform.localRotation =  Quaternion.Euler(0, 0, value.instance.weaponPos.z);
        GameObject.FindGameObjectsWithTag("Character")[0].GetComponent<ZombieController>().weaponObject = newWeapon;
        // -- Parts
        WeaponGraphicsUpdater.UpdateWeaponGraphic(value, partsUsed, newWeapon);
        // -- RightHand
        GameObject rHand = transform.Find("Body/Instance/RHand").gameObject;
        rHand.transform.localPosition = new Vector3(value.instance.rightHandPos.x, value.instance.rightHandPos.y, -0.2f);
        rHand.transform.localRotation = Quaternion.Euler(0, 0, value.instance.rightHandPos.z);
        // -- LeftHand
        GameObject lHand = transform.Find("Body/Instance/LHand").gameObject;
        lHand.transform.localPosition = new Vector3(value.instance.leftHandPos.x, value.instance.leftHandPos.y, -0.2f);
        lHand.transform.localRotation = Quaternion.Euler(0, 0, value.instance.leftHandPos.z);
        // hide hidden
        // TODO: ME

        // settings on graphics
        hitbox = newWeapon.transform.Find(value.collidablePart.ToString()).GetComponent<HitBox>();

        controller.Reset();
    }
    public void TakeDamage(float damage) {
        if (invincibleTimer < 0) {
           life -= damage;
            invincibleTimer = invincibilityTime; 
            GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, transform);
            DamageText.GetComponent<DamageText>().textToDisplay = damage.ToString("0.00");
        }
        
    }
    public void PickupItem(ItemType itemType, int itemId)
    {   
        // carryingItems.Concat(new int[] { itemId }).ToArray();
        switch (itemType) {
            case ItemType.Equipment:
                AddEquipment(itemId);
                break;
            case ItemType.Weapon:
                AddWeapon(itemId);
                break;
            case ItemType.Part:
                AddPart(itemId);
                break;

        }
        
    }
    public void RemoveItem(ItemType itemType, int itemId) {
        switch (itemType) {
            case ItemType.Equipment:
                RemoveEquipment(itemId);
                break;
            case ItemType.Weapon:
                RemoveWeapon(itemId);
                break;
            case ItemType.Part:
                RemovePart(itemId);
                break;

        }
    }
    public void AddEquipment(int id)
    {
        UIManager.Instance.tabRefresh = true;
    }
    public void AddWeapon(int id)
    {
        UIManager.Instance.tabRefresh = true;
    }
    public void AddPart(int id)
    {
        Part part = GameLib.Instance.GetPartById(id);
        carryingWeight += part.weight;
        parts.Add(part);
        UIManager.Instance.tabRefresh = true;
    }
     public void RemoveEquipment(int id)
    {
        UIManager.Instance.tabRefresh = true;
    }
    public void RemoveWeapon(int id)
    {
        UIManager.Instance.tabRefresh = true;
    }
    public void RemovePart(int id)
    {
        Part part = GameLib.Instance.GetPartById(id);
        carryingWeight -= part.weight;
        for(int i = 0; i <parts.Count; i++){
            if (parts[i].id == id) {
                parts.RemoveAt(i);
                break;
            }
        }
        
        UIManager.Instance.tabRefresh = true;
    }
    public void RemovePartByIndex(int i)
    {
        Part part = parts[i];
        carryingWeight -= part.weight;
        parts.RemoveAt(i);
        UIManager.Instance.tabRefresh = true;
    }
}
