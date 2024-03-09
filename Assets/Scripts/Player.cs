using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using TMPro;

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
    public List<Consumable> consumables;

    public float carryingWeight;
    
    public int playerLevel = 0;

    public List<GameObject> engagedMonster = new List<GameObject>();
    public float engagementTime = 2f;
    public float engagementTimer;
    public float engagementFor;

    public int maxPartySize = 4;

    [HideInInspector]
    public bool isTeamHovered = false;

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
    public void DoStart()
	{
        ResetContol();
        CalculateNextLevel();
        activePerson.stats = GameOverlord.Instance.defaultCharacterStats.ConvertAll(stat => stat.Clone());
        for(int i = 0; i <characters.Count; i++){
            characters[i].equipped = new Equipped();
            characters[i].stats = GameOverlord.Instance.defaultCharacterStats.ConvertAll(stat => stat.Clone());
            ResetStats(characters[i]);
        }
    
	}
    public void ResetContol() {
        int index = Array.FindIndex(characters.ToArray(), c => c.id == activeCharId);
        activePerson = characters[index];

        controller = activePerson.controller;
    }
    public void LeaderDied() {
        if (characters.Count > 0) {
            characters[0].controller.BecomeLeader();
        } else {
            GameOverlord.Instance.GameOver();
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
                        characters[i].experience+=UnityEngine.Random.Range(0,2);
                }

                if (characters[i].experience >= characters[i].experienceToNextLevel) {
                    LevelUpChar(i);
                    characters[i].experienceToNextLevel = characters[i].experienceToFirstLevel + characters[i].level* characters[i].experienceIncrement;
                }
            }
        }
    }
    public void LevelUpChar(int ind) {
        characters[ind].level++;
        characters[ind].experience = 0;
        characters[ind].life = characters[ind].stats[0].value;
        characters[ind].controller.Reset();
        GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, characters[ind].controller.transform);
        DamageText.GetComponent<DamageText>().textToDisplay = "^";

        PlayerLevelProgression(ind);
        // TODO:  Play some fun animation here

        // trigger UI to show bonus prompt
        UIManager.Instance.lvlUpQueue.Add(characters[ind].id);

        if (characters[ind].level == 1) {
            if (characters[ind].skills==null)characters[ind].skills = new List<CharSkill>() ;
            // weapon skill
            characters[ind].skills.Add(GameLib.Instance.allCharSkills[0].Clone());
        }
        UIManager.Instance.UpdateSkillSquare();
    }
    void Update()
    {
        
        if (engagementTimer > 0) {
            engagementTimer -= Time.deltaTime;
            engagementFor+= Time.deltaTime;
        } 
        else 
        for (int i = 0; i < engagedMonster.Count; i++)
        {
            if (engagedMonster[i] == null) {
                engagedMonster.RemoveAt(i);
            } else if (Vector3.Distance(engagedMonster[i].transform.position, Camera.main.transform.position) > (BerkeleyManager.Instance.disappearDistance/6f)) {
                engagedMonster.RemoveAt(i);
            }
        }
        
        if (engagedMonster.Count < 1) {
            engagementTimer = 0;
            engagementFor = 0;
        }    
    }
    public float EngagedFor() {
        return engagementFor;
    }
    public void CalculateNextLevel()
    {
        activePerson.experienceToNextLevel = activePerson.experienceToFirstLevel + (activePerson.experienceIncrement * activePerson.level);
    }
    public void ResetStats(FriendlyChar person)
    {
        person.life = activePerson.stats[0].value;
        person.stamina = activePerson.stats[1].value;
        person.mana = activePerson.stats[2].value;
        
    }
    public void EquipArmor(Equipment equipment, bool left = false, int charId = -1)
    {// THIS DOES NOT REMOVE FROM INVENTORY. HAVE TO CALL REMOVE EQUIP
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = GetCharById(charId);

        if (equipment.slot == Slot.Pauldron ) {
            if (left) person.equipped.leftPauldron = equipment; else person.equipped.rightPauldron  = equipment;
        } else if (equipment.slot == Slot.Foot) {
            if (left) person.equipped.leftFoot = equipment; else person.equipped.rightFoot  = equipment;
        } else if (equipment.slot == Slot.Hand) {
            if (left) person.equipped.leftHand = equipment; else person.equipped.rightHand  = equipment;
        } else if (equipment.slot == Slot.Chest) {
            person.equipped.chest = equipment;
        } else if (equipment.slot == Slot.Head) {
            person.equipped.head = equipment;
        }
        // graphs
		// Debug.Log("AKA Player/Body/" +Util.SlotToBodyPosition(equipment.slot, left) + " \n "+ equipment.visual.GetComponent<SpriteRenderer>().sprite.name + " - "+person.name);
        GameObject current = person.controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(equipment.slot, left)).gameObject;
        SpriteRenderer sr = equipment.visual.GetComponent<SpriteRenderer>();
        current.GetComponent<SpriteRenderer>().sprite=sr.sprite;
        current.GetComponent<SpriteRenderer>().color=sr.color;
        current.transform.localScale = equipment.visual.transform.localScale;

        //stats
        for(int i = 0; i <equipment.modifiers.Length; i++){
            person.stats[(int)equipment.modifiers[i].affectedStat].value += equipment.modifiers[i].offset;
        }
        ApplyStats(charId);
    }
    public void Unequip(Slot slot, bool left = false, int charId = -1) {
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = GetCharById(charId);
        GameObject bod = controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(slot, left)).gameObject;
        Equipment equipment = person.equipped.chest;
        switch (slot) {
            case (Slot.Head):
                if (person.equipped.head == null) return;
                equipment = person.equipped.head;            
                AddEquipment(person.equipped.head.id);
                person.equipped.head = null;
                
                // Debug.Log("AKA Unequip "+ person.appearance.head.GetComponent<SpriteRenderer>().sprite.name + "\n - "+person.name);
                bod.GetComponent<SpriteRenderer>().sprite = person.appearance.head.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = person.appearance.head.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Chest):
                if (person.equipped.chest == null) return;
                equipment = person.equipped.chest; 
                AddEquipment(person.equipped.chest.id);
                person.equipped.chest = null;
                // Debug.Log("AKA Unequip "+ person.appearance.clothing.GetComponent<SpriteRenderer>().sprite.name + "\n - "+person.name);
                // if (person.appearance.clothing.GetComponent<SpriteRenderer>().sprite.name.Contains("eps_17")) MapMaker.Instance.debugMarker.transform.position = person.controller.transform.position;
                bod.GetComponent<SpriteRenderer>().sprite = person.appearance.clothing.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = person.appearance.chest.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Pauldron):
                if (left) {
                    if (person.equipped.leftPauldron == null) return;
                    equipment = person.equipped.leftPauldron; 
                    AddEquipment(person.equipped.leftPauldron.id);
                    person.equipped.leftPauldron = null;
                }else {
                    if (person.equipped.rightPauldron == null) return;
                    equipment = person.equipped.rightPauldron; 
                    AddEquipment(person.equipped.rightPauldron.id);
                    person.equipped.rightPauldron = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = null;
                
                break;
            case (Slot.Foot):
                if (left) {
                    if (person.equipped.leftFoot == null) return;
                    equipment = person.equipped.leftFoot; 
                    AddEquipment(person.equipped.leftFoot.id);
                    person.equipped.leftFoot = null;
                }else {
                    if (person.equipped.rightFoot == null) return;
                    equipment = person.equipped.rightFoot; 
                     AddEquipment(person.equipped.rightFoot.id);
                    person.equipped.rightFoot = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = person.appearance.foot.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = person.appearance.foot.GetComponent<SpriteRenderer>().color;
                
                break;
            case (Slot.Hand):
                if (left) {
                    if (person.equipped.leftHand == null) return;
                    equipment = person.equipped.leftHand; 
                    AddEquipment(person.equipped.leftHand.id);
                    person.equipped.leftHand = null;
                }else {
                    if (person.equipped.rightHand == null) return;
                    equipment = person.equipped.rightHand; 
                     AddEquipment(person.equipped.rightHand.id);
                    person.equipped.rightHand = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = person.appearance.hand.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = person.appearance.hand.GetComponent<SpriteRenderer>().color;
                
                break;
        }

        //stats
        for(int i = 0; i <equipment.modifiers.Length; i++){
            person.stats[(int)equipment.modifiers[i].affectedStat].value -= equipment.modifiers[i].offset;
        }
        ApplyStats(person.id);
    }
    public void EquipWeapon(int itemId, List<Part> partsUsed, int charId = -1)
    {
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = GetCharById(charId);
        Weapon value = GameLib.Instance.GetWeaponById(itemId).Clone();

        // settings
        if (person.equipped is null)        
        person.equipped = new Equipped();
        person.equipped.primaryWeapon = value;
        person.equipped.partsBeingUsed = partsUsed;
        
        /* graphics */
        // -- Weapon
        try {
            Destroy(person.controller.gameObject.transform.Find("Player/Body/Instance/PrimaryWeapon").gameObject);
                    // looks weird but keep it
                    person.controller.gameObject.transform.Find("Player/Body/Instance/PrimaryWeapon").gameObject.name = "urgh";
        } catch (NullReferenceException) {
            // sweat not, there is no weapon
        }
        GameObject newWeapon = Instantiate(value.visual);
        newWeapon.name = "PrimaryWeapon";
        newWeapon.transform.parent = person.controller.gameObject.transform.Find("Player/Body/Instance");
        newWeapon.transform.localPosition = new Vector3(value.instance.weaponPos.x, value.instance.weaponPos.y, -9.3f);
        newWeapon.transform.localRotation =  Quaternion.Euler(0, 0, value.instance.weaponPos.z);
        person.controller.gameObject.GetComponent<ZombieController>().weaponObject = newWeapon;
        person.equipped.primaryWeapon.visual = newWeapon;
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

        // Skill bar
        if (activeCharId == person.id)
		UIManager.Instance.UpdateSkillSquare();

        // calculate stats
        for (int i = 0; i < partsUsed.Count; i++)
        {
            for (int j = 0; j < person.equipped.partsBeingUsed[i].modifiers.Length; j++)
            {
                PowerUp mod = person.equipped.partsBeingUsed[i].modifiers[j];
                person.stats[(int)mod.affectedStat].value += mod.offset;
                Debug.Log("+ "+mod.affectedStat+" "+mod.offset);
            }
        }
        ApplyStats(charId);
    }
    // Adds parts back to inventory and equips disarmed
    public void UnequipWeapon(int charId = -1) {
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = GetCharById(charId);
        for(int i = 0; i <person.equipped.partsBeingUsed.Count; i++){
            AddPart(person.equipped.partsBeingUsed[i].id);
            for (int j = 0; j < person.equipped.partsBeingUsed[i].modifiers.Length; j++)
            {
                PowerUp mod = person.equipped.partsBeingUsed[i].modifiers[j];
                person.stats[(int)mod.affectedStat].value -= mod.offset;
            }
        }

        EquipWeapon(100000, new List<Part>(), charId);
    }
    public void ResetAllArmorLooks() {
        for (int i = 0; i < characters.Count; i++)
        {
            try{
            GameObject bod =characters[i].controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(Slot.Head, false)).gameObject;
            bod.GetComponent<SpriteRenderer>().sprite = characters[i].equipped.head.visual.GetComponent<SpriteRenderer>().sprite;
            bod.GetComponent<SpriteRenderer>().color = characters[i].equipped.head.visual.GetComponent<SpriteRenderer>().color;
            }catch(NullReferenceException e){}
            try{
            GameObject bod =characters[i].controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(Slot.Chest, false)).gameObject;
            bod.GetComponent<SpriteRenderer>().sprite = characters[i].equipped.chest.visual.GetComponent<SpriteRenderer>().sprite;
            bod.GetComponent<SpriteRenderer>().color = characters[i].equipped.chest.visual.GetComponent<SpriteRenderer>().color;
            }catch(NullReferenceException e){}
            try{
            GameObject bod =characters[i].controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(Slot.Pauldron, false)).gameObject;
            bod.GetComponent<SpriteRenderer>().sprite = characters[i].equipped.rightPauldron.visual.GetComponent<SpriteRenderer>().sprite;
            bod.GetComponent<SpriteRenderer>().color = characters[i].equipped.rightPauldron.visual.GetComponent<SpriteRenderer>().color;
            }catch(NullReferenceException e){}
            try{
            GameObject bod =characters[i].controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(Slot.Pauldron, true)).gameObject;
            bod.GetComponent<SpriteRenderer>().sprite = characters[i].equipped.leftPauldron.visual.GetComponent<SpriteRenderer>().sprite;
            bod.GetComponent<SpriteRenderer>().color = characters[i].equipped.leftPauldron.visual.GetComponent<SpriteRenderer>().color;
            }catch(NullReferenceException e){}
            try{
            GameObject bod =characters[i].controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(Slot.Hand, false)).gameObject;
            bod.GetComponent<SpriteRenderer>().sprite = characters[i].equipped.rightHand.visual.GetComponent<SpriteRenderer>().sprite;
            bod.GetComponent<SpriteRenderer>().color = characters[i].equipped.rightHand.visual.GetComponent<SpriteRenderer>().color;
            }catch(NullReferenceException e){}
            try{
            GameObject bod =characters[i].controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(Slot.Hand, true)).gameObject;
            bod.GetComponent<SpriteRenderer>().sprite = characters[i].equipped.leftHand.visual.GetComponent<SpriteRenderer>().sprite;
            bod.GetComponent<SpriteRenderer>().color = characters[i].equipped.leftHand.visual.GetComponent<SpriteRenderer>().color;
            }catch(NullReferenceException e){}
            try{
            GameObject bod =characters[i].controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(Slot.Foot, false)).gameObject;
            bod.GetComponent<SpriteRenderer>().sprite = characters[i].equipped.rightFoot.visual.GetComponent<SpriteRenderer>().sprite;
            bod.GetComponent<SpriteRenderer>().color = characters[i].equipped.rightFoot.visual.GetComponent<SpriteRenderer>().color;
            }catch(NullReferenceException e){}
            try{
            GameObject bod =characters[i].controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(Slot.Foot, true)).gameObject;
            bod.GetComponent<SpriteRenderer>().sprite = characters[i].equipped.leftFoot.visual.GetComponent<SpriteRenderer>().sprite;
            bod.GetComponent<SpriteRenderer>().color = characters[i].equipped.leftFoot.visual.GetComponent<SpriteRenderer>().color;
            }catch(NullReferenceException e){}
        }
    }
    public void ApplyStats(int charId = -1) {
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = GetCharById(charId);
        Weapon value = person.equipped.primaryWeapon;

        // damage
        float dmg = CalculateDamage(person.id);

        // settings on graphics
        if (value.damageType == DamageType.Melee) {
            person.hitbox = value.visual.transform.Find(value.collidablePart.ToString()).GetComponent<HitBox>();
            person.hitbox.damageRsrcType = value.damageRsrcType;
            person.hitbox.damageMin = dmg;
            person.hitbox.damageMax = dmg * 1.5f;
            person.hitbox.friendlyOwner = person.controller;
            // CHEAT (this is supposed to only go on when hitting but that is not whats happening)
            // person.hitbox.hitting=true;
        }
        // 12 = armor stat
        person.controller.armorDefense = person.stats[12].value;
        person.controller.Reset();

    }
    public float CalculateDamage(int charId = -1) {
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = GetCharById(charId);
        List<Part> partsUsed = person.equipped.partsBeingUsed;
        Weapon value = person.equipped.primaryWeapon;
        
        float dmg = 0; int slot = 3; // slot  is for the damage type, using hardcoded indexes
        // Add stat bonuses
        if (value.damageType == DamageType.Melee) {
            dmg += person.stats[3].value; // 3 is stat for melee damage
        } else if (value.damageType == DamageType.Ranged) {
            dmg += person.stats[4].value; // 4 is stat for ranged damage
        } else if (value.damageType == DamageType.Magic) {
            dmg += person.stats[5].value; // 5 is stat for magic damage
        }
        dmg += person.stats[14].value; // 14 is stat for all damages

        for(int i = 0; i <partsUsed.Count; i++){
            for(int j = 0; j <partsUsed[i].modifiers.Length; j++){
                // melee
                if (partsUsed[i].modifiers[j].affectedStat == CharacterStat.MeleeDamage && value.damageType == DamageType.Melee) {
                    dmg += partsUsed[i].modifiers[j].offset; slot = 3;
                }
                // ranged
                if (partsUsed[i].modifiers[j].affectedStat == CharacterStat.RangedDamage && value.damageType == DamageType.Ranged) {
                    dmg += partsUsed[i].modifiers[j].offset; slot = 4;
                }
                // magic
                if (partsUsed[i].modifiers[j].affectedStat == CharacterStat.MagicDamage && value.damageType == DamageType.Magic) {
                    dmg += partsUsed[i].modifiers[j].offset; slot = 5;
                }
            }
        }
        if (dmg == 0) dmg = 1;
        // TODO: add modifiers
        // STATSET
        // person.stats[slot].value = (int)System.Math.Floor(dmg);

        return dmg;
    }
    public void PickupItem(ItemType itemType, int itemId)
    {   
        // carryingItems.Concat(new int[] { itemId }).ToArray();
        switch (itemType) {
            case ItemType.Equipment:
                AddEquipment(itemId);
                UIManager.Instance.AutoEquipSingleArmor(itemId);
                break;
            case ItemType.Weapon:
                AddWeapon(itemId);
                break;
            case ItemType.Part:
                AddPart(itemId);
                break;

        }
    }
    public void PickupItemId(int itemId)
    {   
        // this is not exactly dynamic but its fine
        if (itemId.ToString().Length < 6) {
            AddEquipment(itemId);
            UIManager.Instance.AutoEquipSingleArmor(itemId);
        }
        else if(Int32.Parse(itemId.ToString().Substring(0,1)) == 1) {
            AddWeapon(itemId);
        } else if(Int32.Parse(itemId.ToString().Substring(0,1)) == 9) {
            AddPart(itemId);
        } else if(Int32.Parse(itemId.ToString().Substring(0,1)) == 8) {
            AddConsumable(itemId);
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
        UIManager.Instance.ShowItemPickedUp(part.name, part.icon);
    }
    public void AddConsumable(int id)
    {
        Consumable co = GameLib.Instance.GetConsumableById(id);
        carryingWeight += co.weight;
        consumables.Add(co);
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


    public void Engage(GameObject enemy) {
        for (int i = 0; i < engagedMonster.Count; i++)
        {
            if(engagedMonster[i].GetInstanceID() == enemy.GetInstanceID()) {
                return;
            }
        }
        engagedMonster.Add(enemy);
        // Debug.Log("add " + engagedMonster.Count);
        engagementTimer = engagementTime;
    }
    public void Unengage(GameObject enemy) {
        // Debug.Log("Remove " + engagedMonster.Count);
        engagedMonster.Remove(enemy);
        if (engagedMonster.Count < 1) {
            engagementTimer = 0;
            engagementFor = 0;
        }    
    }
    public List<Consumable> GetConsumablesByType(ConsumableType type) {
        List<Consumable> list = new List<Consumable>();
        
        for(int i = 0; i <consumables.Count; i++){
            if (consumables[i].consumableType == type) {
                list.Add(consumables[i]);
            }
        }
        
        return list;
    }
    public List<Part> GetPartsByType(ConsumableType type) {
        List<Part> list = new List<Part>();
        
        for(int i = 0; i <parts.Count; i++){
            if (parts[i].consumableType == type) {
                list.Add(parts[i]);
            }
        }
        
        return list;
    }

    public FriendlyChar GetCharById(int id) {
        int index = Array.FindIndex(characters.ToArray(), c => c.id == id);
        if (index == -1) {
            Debug.LogError("GetCharById was called using an id that was not yet added to the characters list");
        }
        return characters[index];
    }
    public void ConvertNeutral(GameObject neutral) {
        ZombieController controller = neutral.AddComponent<ZombieController>();
        Neutral neutralScript = neutral.GetComponent<Neutral>();

        // Create friendlyChar
        FriendlyChar newFriend = new FriendlyChar();
        newFriend.name = neutralScript.person.name;
        newFriend.id = BerkeleyManager.Instance.LatestFriendId();
        newFriend.experience = 0;
        newFriend.level = 0;
        newFriend.appearance = neutralScript.person.appearance;
        newFriend.personality = neutralScript.person.personality;
        newFriend.oddities = neutralScript.person.oddities;
        newFriend.controller = controller;
        newFriend.stats =  GameOverlord.Instance.defaultCharacterStats.ConvertAll(stat => stat.Clone());
        newFriend.life = newFriend.stats[0].value;

        newFriend.formation = GameLib.Instance.TakeAvailableFormtion();

        // newFriend.weaponOnLoad = neutralScript.weapon.id;
        // newFriend.weaponOnLoadParts = neutralScript.partsUsed;
        // GameOverlord.Instance.nearbyMonsters.Remove( GameOverlord.Instance.nearbyMonsters.Single( s => s.name == neutral.name ) );
        
        // settings on player script
        this.characters.Add(newFriend);

        // Settings on component
        controller.charId = newFriend.id;
        controller.moveSpeed = 1.6f; // TODO: Unhardcode
        controller.turnSpeed = 5f; // TODO: Unhardcode
        controller.feetSpeed = 0.5f; // TODO: Unhardcode

        // Settings on GameObject
        neutral.tag = "Character";
        neutral.name = neutralScript.person.name;
        GameObject nameplate = Instantiate(GameOverlord.Instance.namePlate);
        nameplate.name = "NamePlate";
        nameplate.transform.parent = neutral.transform;
        nameplate.transform.localPosition = new Vector2(0.43f, 0);

        controller.DoStart();
        
        UIManager.Instance.RemoveMonsterIndicator(neutralScript.indicatorIndex);

        Destroy(neutral.transform.Find("Vision").gameObject);
        Destroy(neutralScript);

        UIManager.Instance.CheckAutoEquips();
        UIManager.Instance.ShowIndicator(neutral.name+ " recruited!", UIManager.Instance.recruitedIcon);
    }
    // this ALSO REMOVES THEM
    public List<Part> FindNeededParts(FittablePart[] partTypes) {
        List<Part> shoppingList = new List<Part>();
        List<FittablePart> goingParts = partTypes.Cast<FittablePart>().ToList();

        for(int i = 0; i <parts.Count; i++){
            if (goingParts.Contains(parts[i].fittablePart)) {
                shoppingList.Add( parts[i]);
                goingParts.Remove(parts[i].fittablePart);
                parts.RemoveAt(i);
                i--;
            }
        }
        
        return shoppingList;
    }
    public void ApplyBonus(int charId, Bonus bonus) {
        int index = Array.FindIndex(characters.ToArray(), c => c.id == charId);   
        if (index == -1) {
            return;
        }     
        switch (bonus.bonusType) {
            case (BonusType.PowerUp):
                characters[index].stats.Find(i => i.stat == bonus.powerUp.affectedStat).value += bonus.powerUp.offset;
                characters[index].bonuses.Add(bonus);
                break;
            case (BonusType.Loot):
                Debug.Log("arros");
                for(int i = 0; i <bonus.items.Length; i++){Debug.Log("pickup"+bonus.items[i]);
                    if (UnityEngine.Random.Range(0,2) == 1) // 50-50 chance
                    PickupItemId(bonus.items[i]);
                }
                
                break;
            case (BonusType.PassiveAbility):
                characters[index].bonuses.Add(bonus);
                characters[index].ownedAbilities.Add(bonus.PassiveAbility);
                break;
            case (BonusType.Setting):
                if  (bonus.id ==15) { // 15 = Hardcoded bonus for max party size increase
                    Player.Instance.maxPartySize+=1;
                }
                break;
        }
    }
    public void ClearPartsBeingUsed(int charId ){
        FriendlyChar character = GetCharById(charId);
        for(int i = 0; i <character.equipped.partsBeingUsed.Count; i++){
            AddPart(character.equipped.partsBeingUsed[i].id);
        }
        character.equipped.partsBeingUsed = new List<Part>();
    }
    public void PlayerLevelProgression(int ind, bool limited=false) {
        if (characters[ind].level > playerLevel) playerLevel++;
        else return;

        if (playerLevel == 1) {
            BerkeleyManager.Instance.spawnables[1].limit=3;
            BerkeleyManager.Instance.spawnables[4].limit=3;
            BerkeleyManager.Instance.spawnables[5].limit=2;
        }
        if (playerLevel == 7) {
            BerkeleyManager.Instance.spawnables[1].limit=0; // goblin
            BerkeleyManager.Instance.spawnables[4].limit=6; // trolls
            BerkeleyManager.Instance.spawnables[5].limit=5;
            BerkeleyManager.Instance.spawnables[4].spawnTime=15f;
            BerkeleyManager.Instance.spawnables[5].spawnTime=20f;
        }

        // may still be able to level up again, but only do it twice
        if(!limited)PlayerLevelProgression(ind, true);
    }
    public Equipment GetEquippedBySlot(Slot slot, int charInd, bool left=false ){
        switch (slot) {

            case (Slot.Head):
                return characters[charInd].equipped.head;
                break;
            case (Slot.Chest):
                return characters[charInd].equipped.chest;
                break;
            case (Slot.Pauldron):
                if (left)return characters[charInd].equipped.leftPauldron;
                else return characters[charInd].equipped.rightPauldron;
                break;
            case (Slot.Foot):
                if (left)return characters[charInd].equipped.leftFoot;
                else return characters[charInd].equipped.rightFoot;
                break;
            case (Slot.Hand):
                if (left)return characters[charInd].equipped.leftHand;
                else return characters[charInd].equipped.rightHand;
                break;
            
        }
        return null;
    }
}
