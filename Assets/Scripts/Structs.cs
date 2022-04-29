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
    Ammo,
    Placeable,
}
public enum DamageRsrcType
{
    None,
    Axe,
    Pickaxe,
    Fishnet,
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
    Hand,
}
public enum PlatingMaterial
{
    Wood,
    Copper,
    Tin,
    Bronze,
    Iron,
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
}
public enum CharacterStat
{
    Life,
    Stamina,
    Mana,

    MeleeDamage,
    RangedDamage,
    MagicDamage,
    
    MaxLife,
    MaxStamina,
    MaxMana,
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
    public float offset;
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
    public DamageRsrcType damageRsrcType;
}
[Serializable]
public class Part : Item
{
    public FittablePart fittablePart;
    public PlatingMaterial material;
    public PartLooks[] partLooks;
}

[Serializable]
public class Consumable : Item
{
    public PowerUp[] modifiers;
    // 0 makes it permanent
    public float duration;
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
}
[Serializable]
public class Appearance
{
    public GameObject head;
    public GameObject chest;
    public GameObject foot;
    public GameObject hand;
}
[Serializable]
public class CurrentCharStat
{
    public CharacterStat stat;
    public int id;
    public int value;
    public GameObject hand;
}

[Serializable]
public class FriendlyChar {
    
    public string name;
    public int id;
    public bool isMale;
    public Personality personality;
    public DistToMain formation;
    public ZombieController controller;
    public float reactionTime = 1;
    public float maxLife;
    public float maxStamina;
    public float maxMana;

    public float maxWeight;

    public float life;
    public float stamina;
    public float mana;

    public float invincibilityTime = 0.3f;
    public float invincibleTimer;

    public int level = 0;
    public int experience = 0;
    public int experienceToFirstLevel = 10;
    public int experienceIncrement = 5;
    [SerializeField]
    public Appearance appearance;

   
    [HideInInspector]
    public Equipped equipped;
    [HideInInspector]
    public HitBox hitbox;
    [HideInInspector]
    public int experienceToNextLevel;
     
}
