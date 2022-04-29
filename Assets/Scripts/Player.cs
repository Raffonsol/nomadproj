using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public ZombieController controller;
    public int activeCharId = 0;
    public List<FriendlyChar> characters;

    public FriendlyChar activePerson;

    public List<Equipment> equipments;
    public List<Weapon> weapons;
    public List<Part> parts;

    public float carryingWeight;

    public GameObject engagedMonster;
    private float engagementTime = 6f;
    public float engagementTimer;

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
        ResetContol();
        CalculateNextLevel();
        for(int i = 0; i <characters.Count; i++){
            characters[i].equipped = new Equipped();
            ResetStats(characters[i]);
        }
    
	}
    public void ResetContol() {
        activePerson = characters[activeCharId];
        controller = activePerson.controller;
    }
    public void LeaderDied() {
        if (characters.Count > 0) {
            characters[0].controller.BecomeLeader();
        } else {
            // TODO: Gameover
        }
    }
    public void GainExperience(int exp) {
        int remainingExp = exp;
        while (remainingExp >0) {
            for(int i = 0; i <characters.Count; i++){
                characters[i].experienceToNextLevel = characters[i].experienceToFirstLevel + characters[i].level* characters[i].experienceIncrement;
                characters[i].experience+=1;
                remainingExp -=1;
                if (characters[i].controller.leader) {
                        characters[i].experience+=Random.Range(0,1);
                }

                if (characters[i].experience >= characters[i].experienceToNextLevel) {
                    LevelUpChar(i);
                    characters[i].experienceToNextLevel = characters[i].experienceToFirstLevel + characters[i].level* characters[i].experienceIncrement;
                }
            }
        }
    }
    public void LevelUpChar(int charId) {
        characters[charId].level++;
        characters[charId].experience = 0;
        characters[charId].life = characters[charId].maxLife;
        characters[charId].controller.Reset();
        GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, characters[charId].controller.transform);
        DamageText.GetComponent<DamageText>().textToDisplay = "^";
        // TODO:  Play some fun animation here
    }
    void Update()
    {
        for(int i = 0; i <characters.Count; i++){
            if (characters[i].invincibleTimer >= 0) {
                characters[i].invincibleTimer -= Time.deltaTime;
            }
        }
        
        
        if (engagementTimer > 0) {
            engagementTimer -= Time.deltaTime;
        } else if (engagedMonster !=null) engagedMonster = null;
    }
    public void CalculateNextLevel()
    {
        activePerson.experienceToNextLevel = activePerson.experienceToFirstLevel + (activePerson.experienceIncrement * activePerson.level);
    }
    public void ResetStats(FriendlyChar person)
    {
        person.invincibleTimer = activePerson.invincibilityTime;
        person.life = activePerson.maxLife;
        person.stamina = activePerson.maxStamina;
        person.mana = activePerson.maxMana;
        
    }
    public void EquipArmor(Equipment equipment, bool left = false)
    {

        if (equipment.slot == Slot.Pauldron ) {
            if (left) activePerson.equipped.leftPauldron = equipment; else activePerson.equipped.rightPauldron  = equipment;
        } else if (equipment.slot == Slot.Foot) {
            if (left) activePerson.equipped.leftFoot = equipment; else activePerson.equipped.rightFoot  = equipment;
        } else if (equipment.slot == Slot.Hand) {
            if (left) activePerson.equipped.leftHand = equipment; else activePerson.equipped.rightHand  = equipment;
        } else if (equipment.slot == Slot.Chest) {
            activePerson.equipped.chest = equipment;
        } else if (equipment.slot == Slot.Head) {
            activePerson.equipped.head = equipment;
        }
        // graphs
        GameObject current = controller.gameObject.transform.Find("Player/Body/" +SlotToBodyPosition(equipment.slot, left)).gameObject;
        current.GetComponent<SpriteRenderer>().sprite = equipment.visual.GetComponent<SpriteRenderer>().sprite;
        current.GetComponent<SpriteRenderer>().color = equipment.visual.GetComponent<SpriteRenderer>().color;
        current.transform.localScale = equipment.visual.transform.localScale;
 
    }
    public void Unequip(Slot slot, bool left = false) {
        GameObject bod = controller.gameObject.transform.Find("Player/Body/" +SlotToBodyPosition(slot, left)).gameObject;
        switch (slot) {
            case (Slot.Head):
                AddEquipment(activePerson.equipped.head.id);
                activePerson.equipped.head = null;
                
                bod.GetComponent<SpriteRenderer>().sprite = activePerson.appearance.head.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = activePerson.appearance.head.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Chest):
                AddEquipment(activePerson.equipped.chest.id);
                activePerson.equipped.chest = null;
                bod.GetComponent<SpriteRenderer>().sprite = activePerson.appearance.chest.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = activePerson.appearance.chest.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Pauldron):
                if (left) {
                    AddEquipment(activePerson.equipped.leftPauldron.id);
                    activePerson.equipped.leftPauldron = null;
                }else {
                     AddEquipment(activePerson.equipped.rightPauldron.id);
                    activePerson.equipped.rightPauldron = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = null;
                
                break;
            case (Slot.Foot):
                if (left) {
                    AddEquipment(activePerson.equipped.leftFoot.id);
                    activePerson.equipped.leftFoot = null;
                }else {
                     AddEquipment(activePerson.equipped.rightFoot.id);
                    activePerson.equipped.rightFoot = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = activePerson.appearance.foot.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = activePerson.appearance.foot.GetComponent<SpriteRenderer>().color;
                
                break;
            case (Slot.Hand):
                if (left) {
                    AddEquipment(activePerson.equipped.leftHand.id);
                    activePerson.equipped.leftHand = null;
                }else {
                     AddEquipment(activePerson.equipped.rightHand.id);
                    activePerson.equipped.rightHand = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = activePerson.appearance.hand.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = activePerson.appearance.hand.GetComponent<SpriteRenderer>().color;
                
                break;
        }
    }
    public void EquipWeapon(int itemId, List<Part> partsUsed, int charId = -1)
    {
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = characters[charId];
        Weapon value = GameLib.Instance.GetWeaponById(itemId);

        // settings
        if (person.equipped is null)        
        person.equipped = new Equipped();
        person.equipped.primaryWeapon = value;
        person.equipped.partsBeingUsed = partsUsed;
        
        /* graphics */
        // -- Weapon
        Destroy(person.controller.gameObject.transform.Find("Player/Body/Instance/PrimaryWeapon").gameObject);
        GameObject newWeapon = Instantiate(value.visual);
        newWeapon.name = "PrimaryWeapon";
        newWeapon.transform.parent = person.controller.gameObject.transform.Find("Player/Body/Instance");
        newWeapon.transform.localPosition = new Vector3(value.instance.weaponPos.x, value.instance.weaponPos.y, -9.3f);
        newWeapon.transform.localRotation =  Quaternion.Euler(0, 0, value.instance.weaponPos.z);
        person.controller.gameObject.GetComponent<ZombieController>().weaponObject = newWeapon;
        // -- Parts
        WeaponGraphicsUpdater.UpdateWeaponGraphic(value, partsUsed, newWeapon);
        // -- RightHand
        GameObject rHand = person.controller.gameObject.transform.Find("Player/Body/Instance/RHand").gameObject;
        rHand.transform.localPosition = new Vector3(value.instance.rightHandPos.x, value.instance.rightHandPos.y, -0.2f);
        rHand.transform.localRotation = Quaternion.Euler(0, 0, value.instance.rightHandPos.z);
        // -- LeftHand
        GameObject lHand = person.controller.gameObject.transform.Find("Player/Body/Instance/LHand").gameObject;
        lHand.transform.localPosition = new Vector3(value.instance.leftHandPos.x, value.instance.leftHandPos.y, -0.2f);
        lHand.transform.localRotation = Quaternion.Euler(0, 0, value.instance.leftHandPos.z);
        // hide hidden

        // settings on graphics
        person.hitbox = newWeapon.transform.Find(value.collidablePart.ToString()).GetComponent<HitBox>();
        person.hitbox.damageRsrcType = value.damageRsrcType;

        person.controller.Reset();
    }
    public void TakeDamage(float damage, int tarId = -1) {
        if (tarId == -1) tarId = activeCharId;
        FriendlyChar person = characters[tarId];
        if (person.invincibleTimer < 0) {
            person.life -= damage;
            person.invincibleTimer = person.invincibilityTime; 
            GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, person.controller.gameObject.transform);
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
        Equipment eqpmt = GameLib.Instance.GetEquipmentById(id);
        carryingWeight += eqpmt.weight;
        equipments.Add(eqpmt);
        
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
        Equipment equip = GameLib.Instance.GetEquipmentById(id);
        carryingWeight -= equip.weight;
        for(int i = 0; i <equipments.Count; i++){
            if (equipments[i].id == id) {
                equipments.RemoveAt(i);
                break;
            }
        }
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
    public string SlotToBodyPosition(Slot slot, bool left) {
        string pos = "";
        if (slot == Slot.Pauldron )
        {
            pos = left? "L" :"R";
        } else if (slot == Slot.Foot) {
            pos = left? "L" :"R";
        } else if (slot == Slot.Hand) {
            pos = left? "Instance/L" :"Instance/R";
        } else if (slot == Slot.Chest) {
            pos = "Chest/";
        } else if (slot == Slot.Head) {
            
        }
        pos = pos + slot.ToString();
        return pos;
    }

    public void Engage(GameObject enemy) {
        engagedMonster = enemy;
        engagementTimer = engagementTime;
    }
}
