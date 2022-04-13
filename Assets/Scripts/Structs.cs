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
    Shoulder,
    Hands,
    Feet,
}
public enum CharacterStat
{
    Life,
    Stamina,
    Mana,
    Level,
    Experience,
    Buffs,
    Debuffs,

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
public class Brain {
    public float patrolStopInterval;
    private float distractionCheckTime;
    private float desistanceCheckTime;
}
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
public class MonsterNPC {
    
    public Brain brain;
    public Movement movement;
    public Attack[] attacks;
    public Drop[] drops;

    public float maxLife;
    

    
}