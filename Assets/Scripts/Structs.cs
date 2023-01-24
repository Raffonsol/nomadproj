using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ------------------- ITEMS -------------------------------------

public enum ItemType
{
    Equipment,
    Consumable,
    Material,
    Part,
    Weapon,
}
public enum DamageRsrcType
{
    None,
    Axe,
    Pickaxe,
    Fishnet,
}
public enum DamageType
{
    Melee,
    Ranged,
    Magic
}
public enum FittablePart
{
    LongStick,
    ShortStick,
    ShortBlade,
    LongBlade,
    Hilt,
    Pommel,
    HammerHead,
    ArrowHead,
    Bowstring,
    Hand, // Leave this here, its for unarmed
    Fiber,
}
public enum PlatingMaterial
{
    Wood,
    Copper,
    Tin,
    Bronze,
    Iron,
    Rock,
    Mythril,
    Titanium,
    Adamarium,
    Ervium,
    Viranium,
    Jotyne,
    Valerean,
    Critonite,
}
public enum Rarity
{
    Common,
    Uncommon,
    WellCrafted,
    StandardQualtity,
    OutstandingQuality,
    Divine,
}
public enum Slot
{
    Head,
    Chest,
    Pauldron,
    Hand,
    Foot,
    Clothing,
}
public enum ConsumableType
{
    Potion,
    Arrow,
    Bolt,
    Scroll,
    Food,
}
public enum CharacterStat
{
    MaxLife,
    MaxStamina,
    MaxMana,
    MeleeDamage,
    RangedDamage,
    MagicDamage,
    Life,
    Stamina,
    Mana,
    LifeRegen,
    StaminaRegen,
    ManaRegen,
    Armor,
    MovementSpeed,
    
}
[Serializable]
public class PowerUp
{
    public CharacterStat affectedStat;
    public int offset;
}
[Serializable]
public class PartLooks
{
    public int weaponId;
    public GameObject look;
    public bool useAlt;
    public float yOffset;
    public float xOffset;
    // rotation
    public float zOffset;
}

[Serializable]
public class Projectile
{
    public float speed;
    public float minDamage;
    public float maxDamage;
    public float maxDistance;
}

[Serializable]
public class Item
{
    public int id;
    public string name;
    public ItemType[] secondaryItemTypes;

    // Graphics
    public Sprite icon;
    public GameObject visual;

    // Controls
    public int quantityLimit;
    public int levelRequirement;

    // Value
    public Rarity rarity;
    public float value;
    public float weight;
}

[Serializable]
public class Equipment : Item
{
    public Slot slot;

    public PowerUp[] modifiers;
    
    public FittablePart[] partsNeeded;
}

[Serializable]
public class Weapon : Item
{
    public int handsNeeded;
    [SerializeField]
    public Instance instance;
    public float cooldown;
    public FittablePart[] partsNeeded;
    public FittablePart collidablePart;
    public DamageType damageType;
    public DamageRsrcType damageRsrcType;

    public ConsumableType ammo;
}
[Serializable]
public class Part : Item
{
    public FittablePart fittablePart;
    public PlatingMaterial material;
    public PartLooks[] partLooks;
    public PowerUp[] statEffects;
    public FittablePart[] partsNeeded;
}

[Serializable]
public class Consumable : Item
{
    public PowerUp[] modifiers;
    // 0 makes it permanent
    public float duration;
    public ConsumableType consumableType;
    public Projectile projectileSettings;
    public FittablePart[] partsNeeded;
}
[Serializable]
public class Material : Item
{
    public int minQuality;
    public int maxQuality;
}



// ------------------------MONSTERS ------------------------------
public class Attack {
    public float attackDuration;
    public float attackSpeed;
    
    public float minDamage;
    public float maxDamage;
    public Sprite attack;
}
[Serializable]
public class Brain {
    public float patrolStopInterval;
    private float distractionCheckTime;
    private float desistanceCheckTime;
}
[Serializable]
public class Movement {
    public Sprite step1;
    public Sprite step2;
    public float runSpeed;
    public float patrolSpeed;
	public float turnSpeed;
	public float feetSpeed;
}
[Serializable]
public class Drop {
    public int itemId;
    public ItemType itemType;
    public int chance;
    public int maxDropped;
}
[Serializable]
public class MonsterNPC {
    
    public int id;
    public string name;
    public Brain brain;
    public Movement movement;
    public Attack[] attacks;
    public Drop[] drops;

    public float maxLife;
    

    
}

// ------------------------NPC ------------------------------
public enum Personality
{
    Hothead,
    Clingy,
    Coward,
    Lazy,
    Giddy,
}
[Serializable]
public class BodyLook {
    public int id;
    public Sprite look;
    public Slot slot;
    // 1 = for male, 2 = for female, 0 = for either, 3+ = conditional
    public int xChromossomes;
}
public class Equipped
{
    public Equipment head;
    public Equipment rightPauldron;
    public Equipment leftPauldron;
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
        rightPauldron = null;
        leftPauldron = null;
        chest = null;
        rightFoot = null;
        leftFoot = null;
        rightHand = null;
        leftHand = null;

        partsBeingUsed = new List<Part>();
    }
}
[Serializable]
public class DistToMain {
	public float x;
	public float y;

    public bool taken = false;

    public DistToMain(float x, float y) {
        this.x = x;
        this.y = y;
    }
}
[Serializable]
public class Appearance
{   
    public bool isMale;
    public int skinColor;
    // should have 5
    public int[] bodyLooks;
    
    // for in game usage
    public GameObject head;
    public GameObject chest;
    public GameObject foot;
    public GameObject hand;
    public GameObject clothing;

    public void SetPartObject(GameObject obj, Slot slot) {
        GameObject addObj = new GameObject();
        switch (slot) {

            case (Slot.Head):
                this.head = addObj;
                this.head.AddComponent<SpriteRenderer>();
                this.head.GetComponent<SpriteRenderer>().sprite = obj.GetComponent<SpriteRenderer>().sprite;
                this.head.GetComponent<SpriteRenderer>().color = obj.GetComponent<SpriteRenderer>().color;

                break;
            case (Slot.Clothing):
                this.clothing = addObj;
                this.clothing.AddComponent<SpriteRenderer>();
                this.clothing.GetComponent<SpriteRenderer>().sprite = obj.GetComponent<SpriteRenderer>().sprite;
                this.clothing.GetComponent<SpriteRenderer>().color = obj.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Chest):
                this.chest = addObj;
                this.chest.AddComponent<SpriteRenderer>();
                this.chest.GetComponent<SpriteRenderer>().sprite = obj.GetComponent<SpriteRenderer>().sprite;
                this.chest.GetComponent<SpriteRenderer>().color = obj.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Foot):
                this.foot = addObj;
                this.foot.AddComponent<SpriteRenderer>();
                this.foot.GetComponent<SpriteRenderer>().sprite = obj.GetComponent<SpriteRenderer>().sprite;
                this.foot.GetComponent<SpriteRenderer>().color = obj.GetComponent<SpriteRenderer>().color;
                break;
            case (Slot.Hand):
                this.hand = addObj;
                this.hand.AddComponent<SpriteRenderer>();
                this.hand.GetComponent<SpriteRenderer>().sprite = obj.GetComponent<SpriteRenderer>().sprite;
                this.hand.GetComponent<SpriteRenderer>().color = obj.GetComponent<SpriteRenderer>().color;
                break;
            
        }
        addObj.SetActive(false);
    }
}
[Serializable]
public class CurrentCharStat
{
    public CharacterStat stat;
    public int id;
    public int value;

    public CurrentCharStat Clone()
    {
        return (CurrentCharStat)this.MemberwiseClone();
    }
}

[Serializable]
public class FriendlyChar {
    
    public string name;
    public int id;
    public bool isMale;
    public Personality personality;
    public DistToMain formation;
    public ZombieController controller;
    public List<CurrentCharStat> stats;
    public float reactionTime = 1;

    public float maxWeight;

    public float life;
    public float stamina;
    public float mana;

    public int level = 0;
    public int experience = 0;
    public int experienceToFirstLevel = 10;
    public int experienceIncrement = 5;
    [SerializeField]
    public Appearance appearance;
    [SerializeField]
    public List<Bonus> bonuses;

    [HideInInspector]
    public int[] equippedOnLoad = new int[0];
    [HideInInspector]
    public int weaponOnLoad = 0;
    [HideInInspector]
    public int[] weaponOnLoadParts = new int[0];
    [HideInInspector]
    public int[] bonusOnLoad = new int[0];

    [HideInInspector]
    public Equipped equipped;
    [HideInInspector]
    public HitBox hitbox;
    [HideInInspector]
    public int experienceToNextLevel;

    public List<PassiveAbility> ownedAbilities;
     
}
public class NeutralChar {
    public string name;
    public bool isMale;
    public Personality personality;
    public Appearance appearance;
}

public enum PassiveAbility
{
    ArrowRecovery,
    DropWeaponOnDeath,
    DropArmorOnDeath,
}
public enum BonusType
{
    PowerUp,
    PassiveAbility,
    Loot,
}
[Serializable]
public class Bonus
{
    public int id;
    public int minLvl;
    public string name;
    public Sprite icon;
    public string description;

    public BonusType bonusType;
    // if type = PowerUp
    public PowerUp powerUp;
    // if type = Loot
    public int[] items;
    // if passive ability
    public PassiveAbility PassiveAbility;
}
public enum NamePartType
{
    FullName,
    FirstPart,
    LastPart,
}
[Serializable]
public class NamePart
{
    public NamePartType type;
    public string value;
    public bool worksForMen;
    public bool worksForWomen;
}
//  ----------------------- Caravan ----

public class Horse {
    public string name;
    public int id;
    public GameObject instance;
}
public class CraftRecipe {
    public Material[] materials;
    public ItemType productType;
    public int productId;
    public int lvlRequired;
}
public class CraftingTable {
    public int id;
    public CraftRecipe[] recipes;
    public Sprite visual;
}
public class Cart {
    public Horse leadingHorse;
    public int id;
    public GameObject instance;
    // 4 item array | -1 means empty | numbers = CraftingTable ids
    public int[] tableSlots;
}

// ----------------- City ----------------------

public enum PreSceneRoomType
{
    Empty,
    Farm,
    House,
    Yard
}
[Serializable]
public class PreSceneRoom {
    // all the types that this room can be oocupied with
    public PreSceneRoomType[] types;
    public bool occupied = false;
    public int row;
    public int col;

    public PreSceneRoom Clone()
    {
        return (PreSceneRoom)this.MemberwiseClone();
    }
}
[Serializable]
public class PreSceneOccupant {
    public PreSceneRoomType type;
    public GameObject visual;
}
[Serializable] // to be used as property of prescene only
public class PreSceneAvailableTypes {
    public PreSceneRoomType type;
    public int quantity;

    public PreSceneAvailableTypes Clone()
    {
        return (PreSceneAvailableTypes)this.MemberwiseClone();
    }
}
[Serializable]
public class PreScene {
    // each prescene must have all its avalable types fit somewhere
    public PreSceneAvailableTypes[] availableTypes;
    public PreSceneRoom[] rooms;
    public float spacePerRoom = 30f;
    // 4, 9, or 25
    public int gridCells;

    public int adults = 2;
}