using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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
    public Bonus[] allBonuses;

    [SerializeField]
    public MonsterNPC[] allMonsters;


    [SerializeField]
    public DistToMain[] mainFormations;

    [SerializeField]
    public NamePart[] nameParts;

    [SerializeField]
    public CharSkill[] allCharSkills;
    [SerializeField]
    public CharSkill[] weaponSkills;

    [SerializeField]
    public OddityChances[] oddityChances;

    [SerializeField]
    public Region[] regions;

    [SerializeField]
    public PersonalityLine[] allLines;

    // settings
    public float acquisitionDistance = 5f;
    
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
        // List<Weapon> weapoi = new List<Weapon>(allWeapons);
        // weapoi.Add(weapoi[6]);
        // allWeapons = weapoi.ToArray();
        // List<Part> weapoi = new List<Part>(allParts);
        // weapoi.Add(weapoi[3]);
        // allParts = weapoi.ToArray();
        // List<Equipment> weapoi = new List<Equipment>(allEquipments);
        // weapoi.Add(weapoi[0]);weapoi.Add(weapoi[1]);weapoi.Add(weapoi[2]);weapoi.Add(weapoi[3]);weapoi.Add(weapoi[4]);
        // allEquipments = weapoi.ToArray();
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
            case (ItemType.Consumable):
                {
                    return (Item) GetConsumableById(thisId); 
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

    public void MakeFormtionAvailable(DistToMain lostFormation) {
        for(int i = 0; i <mainFormations.Length; i++){
            if (mainFormations[i].x == lostFormation.x 
            && mainFormations[i].y == lostFormation.y) {
                mainFormations[i].taken = false;
            }
        }
        
    }
    public DistToMain TakeAvailableFormtion() {
        for(int i = 0; i <mainFormations.Length; i++){
            if (!mainFormations[i].taken) {
                mainFormations[i].taken = true;
                return mainFormations[i];
            }
        }
        Debug.LogError("No more available formations");
        return new DistToMain(0, 0);
    }

    public string GenerateName(bool isMale) {
        nameParts.Shuffle();
        
        int j = 0;
        NamePart part = nameParts[j];
        while (part==null || (!(isMale && part.worksForMen) && !(!isMale && part.worksForWomen))) {
            j++;
            part = nameParts[j];
            if (part.type == NamePartType.FullName && UnityEngine.Random.Range(0, 101) < 75) {
                part = null;
            }
        }
        if (part.type == NamePartType.FullName) {
            return part.value;
        }
        bool needFirstPart = part.type == NamePartType.LastPart;
        for(int i = 0; i <nameParts.Length; i++){
            if (
                ((isMale && nameParts[i].worksForMen) || (!isMale && nameParts[i].worksForWomen))
                &&( (needFirstPart && nameParts[i].type == NamePartType.FirstPart) || (!needFirstPart && nameParts[i].type == NamePartType.LastPart))
            ) {
                if (needFirstPart) {
                    return nameParts[i].value+part.value;
                } else {
                    return part.value+nameParts[i].value;
                }
            }
        }
        return "Rafi";
        
    } 
    /**
    Function with hardcoded weapon ids converted to their weapon skill
     */
    public CharSkill getWeaponsSkill(int weaponId) {
        switch(weaponId) {
            case (100002): // axe and pickaxe
            case (100004): {
                return weaponSkills[0];
            }
            case (100009):  // Pike and lance
            case (100006): {
                return weaponSkills[1];
            }
            case (100008): { // Dagger
                return weaponSkills[2];
            }
            case (100005): // bow and sling
            case (100007): {
                return weaponSkills[3];
            }
            case (100001): {
                return weaponSkills[4];
            }
        }

        return weaponSkills[0];
    }

    public Oddity[] MakeListOfOddities() {
        List<Oddity> oddities = new List<Oddity>();
        oddityChances.Shuffle();

        for (int i = 0; i < oddityChances.Length; i++)
        {
            if (UnityEngine.Random.Range(0, 101) < oddityChances[i].percentage
                && !oddities.Intersect(oddityChances[i].conflictsWith).Any()) {
                oddities.Add(oddityChances[i].oddity);
            }
        }

        return oddities.ToArray();
    }

    public string GetLine(LineUsage situation, Personality personality) {
        allLines.Shuffle();
        int i = 0;
        PersonalityLine goWith = allLines[0];

        while ((goWith.personalities.Length>0 && !goWith.personalities.Contains(personality))  ||  goWith.useWhen!= situation  ) {
            i++;
            if (i >= allLines.Length) return "";
            goWith = allLines[i];
        }
        if (UnityEngine.Random.Range(0, 101) < goWith.chance) {
            
            return goWith.value;
        }
        return "";
    }

}
