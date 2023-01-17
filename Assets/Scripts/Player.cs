using System.Collections;
using System;
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
    public List<Consumable> consumables;

    public float carryingWeight;

    public GameObject engagedMonster;
    private float engagementTime = 6f;
    public float engagementTimer;

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
        // TODO:  Play some fun animation here

        // trigger UI to show bonus prompt
        UIManager.Instance.lvlUpQueue.Add(characters[ind].id);
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
        } else engagedMonster = null;
    }
    public void CalculateNextLevel()
    {
        activePerson.experienceToNextLevel = activePerson.experienceToFirstLevel + (activePerson.experienceIncrement * activePerson.level);
    }
    public void ResetStats(FriendlyChar person)
    {
        person.invincibleTimer = activePerson.invincibilityTime;
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
        GameObject current = person.controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(equipment.slot, left)).gameObject;
        current.GetComponent<SpriteRenderer>().sprite = equipment.visual.GetComponent<SpriteRenderer>().sprite;
        current.GetComponent<SpriteRenderer>().color = equipment.visual.GetComponent<SpriteRenderer>().color;
        current.transform.localScale = equipment.visual.transform.localScale;

        //stats
        for(int i = 0; i <equipment.modifiers.Length; i++){
            person.stats[(int)equipment.modifiers[i].affectedStat].value += equipment.modifiers[i].offset;
        }
        
        
 
    }
    public void Unequip(Slot slot, bool left = false) {
        GameObject bod = controller.gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(slot, left)).gameObject;
        Equipment equipment = activePerson.equipped.chest;
        switch (slot) {
            case (Slot.Head):
                equipment = activePerson.equipped.head;            
                AddEquipment(activePerson.equipped.head.id);
                activePerson.equipped.head = null;
                
                bod.GetComponent<SpriteRenderer>().sprite = activePerson.appearance.head.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = activePerson.appearance.head.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Chest):
                equipment = activePerson.equipped.chest; 
                AddEquipment(activePerson.equipped.chest.id);
                activePerson.equipped.chest = null;
                bod.GetComponent<SpriteRenderer>().sprite = activePerson.appearance.clothing.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = activePerson.appearance.chest.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Pauldron):
                if (left) {
                    equipment = activePerson.equipped.leftPauldron; 
                    AddEquipment(activePerson.equipped.leftPauldron.id);
                    activePerson.equipped.leftPauldron = null;
                }else {
                    equipment = activePerson.equipped.rightPauldron; 
                    AddEquipment(activePerson.equipped.rightPauldron.id);
                    activePerson.equipped.rightPauldron = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = null;
                
                break;
            case (Slot.Foot):
                if (left) {
                    equipment = activePerson.equipped.leftFoot; 
                    AddEquipment(activePerson.equipped.leftFoot.id);
                    activePerson.equipped.leftFoot = null;
                }else {
                    equipment = activePerson.equipped.rightFoot; 
                     AddEquipment(activePerson.equipped.rightFoot.id);
                    activePerson.equipped.rightFoot = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = activePerson.appearance.foot.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = activePerson.appearance.foot.GetComponent<SpriteRenderer>().color;
                
                break;
            case (Slot.Hand):
                if (left) {
                    equipment = activePerson.equipped.leftHand; 
                    AddEquipment(activePerson.equipped.leftHand.id);
                    activePerson.equipped.leftHand = null;
                }else {
                    equipment = activePerson.equipped.rightHand; 
                     AddEquipment(activePerson.equipped.rightHand.id);
                    activePerson.equipped.rightHand = null;
                }
                bod.GetComponent<SpriteRenderer>().sprite = activePerson.appearance.hand.GetComponent<SpriteRenderer>().sprite;
                bod.GetComponent<SpriteRenderer>().color = activePerson.appearance.hand.GetComponent<SpriteRenderer>().color;
                
                break;
        }

        //stats
        for(int i = 0; i <equipment.modifiers.Length; i++){
            activePerson.stats[(int)equipment.modifiers[i].affectedStat].value -= equipment.modifiers[i].offset;
        }
    }
    public void EquipWeapon(int itemId, List<Part> partsUsed, int charId = -1)
    {
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = GetCharById(charId);
        Weapon value = GameLib.Instance.GetWeaponById(itemId);

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

        // calculate damage
        float dmg = CalculateDamage(person.id);

        // settings on graphics
        if (value.damageType == DamageType.Melee) {
            person.hitbox = newWeapon.transform.Find(value.collidablePart.ToString()).GetComponent<HitBox>();
            person.hitbox.damageRsrcType = value.damageRsrcType;
            person.hitbox.damageMin = dmg;
            person.hitbox.damageMax = dmg * 1.5f;
        }
    }
    // Adds parts back to inventory and equips disarmed
    public void UnequipWeapon(int charId = -1) {
        if (charId == -1) charId = activeCharId;
        FriendlyChar person = GetCharById(charId);
        for(int i = 0; i <person.equipped.partsBeingUsed.Count; i++){
            AddPart(person.equipped.partsBeingUsed[i].id);
        }
        EquipWeapon(100000, new List<Part>(), charId);
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


        for(int i = 0; i <partsUsed.Count; i++){
            for(int j = 0; j <partsUsed[i].statEffects.Length; j++){
                // melee
                if (partsUsed[i].statEffects[j].affectedStat == CharacterStat.MeleeDamage && value.damageType == DamageType.Melee) {
                    dmg += partsUsed[i].statEffects[j].offset; slot = 3;
                }
                // ranged
                if (partsUsed[i].statEffects[j].affectedStat == CharacterStat.RangedDamage && value.damageType == DamageType.Ranged) {
                    dmg += partsUsed[i].statEffects[j].offset; slot = 4;
                }
                // magic
                if (partsUsed[i].statEffects[j].affectedStat == CharacterStat.MagicDamage && value.damageType == DamageType.Magic) {
                    dmg += partsUsed[i].statEffects[j].offset; slot = 5;
                }
            }
        }
        if (dmg == 0) dmg = 1;
        // TODO: add modifiers

        // STATSET
        person.stats[slot].value = (int)System.Math.Floor(dmg);
        person.controller.Reset();

        return dmg;
    }
    public void TakeDamage(float damage, int tarId = -1) {
        if (tarId == -1) tarId = activeCharId;
        int index = Array.FindIndex(characters.ToArray(), c => c.id == tarId);
        FriendlyChar person = characters[index];
        if (person.invincibleTimer < 0) {
            // TODO: localize [12 = armor]
            float minDmg = damage - person.stats[12].value;
            if (minDmg < 0) minDmg = 0;
            person.life -= UnityEngine.Random.Range(minDmg,damage);
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
                UIManager.Instance.AutoEquipSingleArmor(itemId);
                break;
            case ItemType.Weapon:
                AddWeapon(itemId);
                break;
            case ItemType.Part:
                AddPart(itemId);
                UIManager.Instance.AutoEquipWeapons();
                break;

        }
    }
    public void PickupItem(int itemId)
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
            UIManager.Instance.AutoEquipWeapons();
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
        engagedMonster = enemy;
        engagementTimer = engagementTime;
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
        newFriend.name = neutralScript.name;
        newFriend.id = BerkeleyManager.Instance.LatestFriendId();
        newFriend.experience = 0;
        newFriend.level = 0;
        newFriend.appearance = neutralScript.appearance;
        newFriend.controller = controller;
        newFriend.stats =  GameOverlord.Instance.defaultCharacterStats.ConvertAll(stat => stat.Clone());
        newFriend.life = newFriend.stats[0].value;

        newFriend.formation = GameLib.Instance.TakeAvailableFormtion();

        // newFriend.weaponOnLoad = neutralScript.weapon.id;
        // newFriend.weaponOnLoadParts = neutralScript.partsUsed;

        
        // settings on player script
        this.characters.Add(newFriend);

        // Settings on component
        controller.charId = newFriend.id;
        controller.moveSpeed = 1.6f; // TODO: Unhardcode
        controller.turnSpeed = 5f; // TODO: Unhardcode
        controller.feetSpeed = 0.5f; // TODO: Unhardcode
        controller.DoStart();

        // Settings on GameObject
        neutral.tag = "Character";
        neutral.name = neutralScript.name;

        neutral.GetComponent<Berkeley>().enabled = false;
        neutralScript.enabled = false;

        UIManager.Instance.CheckAutoEquips();
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
        characters[index].bonuses.Add(bonus);
        switch (bonus.bonusType) {
            case (BonusType.PowerUp):
                characters[index].stats.Find(i => i.stat == bonus.powerUp.affectedStat).value += bonus.powerUp.offset;
            break;
            case (BonusType.Loot):
                for(int i = 0; i <bonus.items.Length; i++){
                    PickupItem(bonus.items[i]);
                }
                
            break;
            case (BonusType.PassiveAbility):
                characters[index].ownedAbilities.Add(bonus.PassiveAbility);
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
}
