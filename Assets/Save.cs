using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

// inventory
 [System.Serializable]
 public class InvData
 {
    public int[] parts;
    public int[] consumables;
    public int[] equipment;

    public InvData(
        int[] parts,
        int[] consumables,
        int[] equipment 
    )
    {
        this.parts = parts;
        this.consumables = consumables;
        this.equipment = equipment;

    }
 }

// party
 [System.Serializable]
 public class CharData
 {
    public int id;
    public string name;

    public int exp;
    public int lvl;
    public float currentLife;

    public int skinColor;
    // should have 5
    public int[] bodyLooks;

    public bool deployed;
    public DistToMain formation;

    public int[] equipped;
    public int equippedWeapon;
    public int[] partsUsed;

    public CharData(
        int id,
        string name,
        int exp,
        int lvl,
        float currentLife,
        int skinColor,
        int[] bodyLooks,
        bool deployed,
        DistToMain formation,
        int[] equipped,
        int equippedWeapon,
        int[] partsUsed
    )
    {
        this.id = id;
        this.name = name;
        this.exp = exp;
        this.lvl = lvl;
        this.currentLife = currentLife;
        this.skinColor = skinColor;
        this.bodyLooks = bodyLooks;
        this.deployed = deployed;
        this.formation = formation;
        this.equipped = equipped;
        this.equippedWeapon = equippedWeapon;
        this.partsUsed = partsUsed;
    }
 }

[System.Serializable]
public class GameData
{
    public CharData[] party;
    public int activeCharIndex;
    public InvData inventory;
 
    public GameData(
        CharData[] party, 
        int activeCharIndex,
        InvData inventory
        )
    {
        this.party = party;
        this.activeCharIndex = activeCharIndex;
        this.inventory = inventory;
    }
}

public class Save : MonoBehaviour
{
    public GameObject characterObject;

    private float saveTimer = 2;
    void Start()
     {
        //  LoadFile();
         
         Player.Instance.DoStart();
        // start all characters to make them right
        for(int i = 0; i <Player.Instance.characters.Count; i++){
            Player.Instance.characters[i].controller.DoStart();
        }

         SaveFile();
     }
     void Update() {
        if (saveTimer > 0) {
            saveTimer -= Time.deltaTime;
        } else {
            Debug.Log("Saving game");
            SaveFile();
            saveTimer = 20f;
        }
     }
     
 
     public void SaveFile()
     {
         string destination = Application.persistentDataPath + "/save.dat";
         FileStream file;
 
         if(File.Exists(destination)) file = File.OpenWrite(destination);
         else file = File.Create(destination);

         Player plInst = Player.Instance;

        // save characters
        List<CharData> party = new List<CharData>();
        for(int i = 0; i <plInst.characters.Count; i++){
            FriendlyChar chara = plInst.characters[i];

            // get all equipped items
            List<int> equipped = new List<int>();
            try {equipped.Add(chara.equipped.head.id);} catch (NullReferenceException){}
            try {equipped.Add(chara.equipped.rightPauldron.id);} catch (NullReferenceException){}
            try {equipped.Add(chara.equipped.leftPauldron.id);} catch (NullReferenceException){}
            try {equipped.Add(chara.equipped.chest.id);} catch (NullReferenceException){}
            try {equipped.Add(chara.equipped.rightFoot.id);} catch (NullReferenceException){}
            try {equipped.Add(chara.equipped.leftFoot.id);} catch (NullReferenceException){}
            try {equipped.Add(chara.equipped.rightHand.id);} catch (NullReferenceException){}
            try {equipped.Add(chara.equipped.leftHand.id);} catch (NullReferenceException){}

            List<int> partsUsed = new List<int>();
            for(int j = 0; j <chara.equipped.partsBeingUsed.Count; j++){
                partsUsed.Add(chara.equipped.partsBeingUsed[j].id);
            }
            
            CharData charData = new CharData(
                chara.id,
                chara.name,
                chara.experience,
                chara.level,
                chara.life,
                chara.appearance.skinColor,
                chara.appearance.bodyLooks,
                true,
                chara.formation,
                equipped.ToArray(),
                chara.equipped.primaryWeapon.id,
                partsUsed.ToArray()
            );
            party.Add(charData);
        }

        // save inventory
        List<int> parts = new List<int>();
        for(int i = 0; i <plInst.parts.Count; i++){
            parts.Add(plInst.parts[i].id);
        }

        List<int> consumables = new List<int>();
        for(int i = 0; i <plInst.consumables.Count; i++){
            consumables.Add(plInst.consumables[i].id);
        }

        List<int> equipments = new List<int>();
        for(int i = 0; i <plInst.equipments.Count; i++){
            equipments.Add(plInst.equipments[i].id);
        }

        InvData inventory = new InvData(parts.ToArray(), consumables.ToArray(), equipments.ToArray());
        
    
         GameData data = new GameData(
            party.ToArray(),
            plInst.activeCharId,
            inventory
         );
         BinaryFormatter bf = new BinaryFormatter();
         bf.Serialize(file, data);
         file.Close();
     }
 
    public void LoadFile()
    {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;
 
        if(File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            Debug.LogError("File not found");
            return;
        }

        // load confirmed
        // destroy starting characters
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Character");
        foreach(GameObject go in gos)
            Destroy(go);
 
         BinaryFormatter bf = new BinaryFormatter();
         GameData data = (GameData) bf.Deserialize(file);
         file.Close();

        // put loaded data where it needs to be

         // leader setting
        Player.Instance.activeCharId = data.activeCharIndex;

        Player.Instance.characters = new List<FriendlyChar>();
        for(int i = 0; i <data.party.Length; i++){
            FriendlyChar chara = new FriendlyChar();
            CharData cd = data.party[i];

            chara.name = cd.name;
            chara.id = cd.id;
            chara.experience = cd.exp;
            chara.level = cd.lvl;
            chara.life = cd.currentLife;
            chara.appearance = new Appearance();
            chara.appearance.skinColor = cd.skinColor;
            chara.appearance.bodyLooks = cd.bodyLooks;

            chara.formation = new DistToMain(cd.formation.x, cd.formation.y);

            chara.equippedOnLoad = cd.equipped;
            chara.weaponOnLoad = cd.equippedWeapon;
            chara.weaponOnLoadParts = cd.partsUsed;

            GameObject inst = Instantiate(characterObject, new Vector2(cd.formation.x, cd.formation.y), transform.rotation);
            chara.controller = inst.gameObject.GetComponent<ZombieController>();
            chara.controller.charId = cd.id;
            
            
            // leader setting
            if (data.activeCharIndex == cd.id) {
                // this is leader
                chara.controller.leader = true;
                Player.Instance.controller = inst.gameObject.GetComponent<ZombieController>();
                Player.Instance.controller.charId = cd.id;
            }
            // lastly add the char to array
            Player.Instance.characters.Add(chara);
        }
       
        // inventory settings
        List<Part> parts = new List<Part>();
        for(int i = 0; i <data.inventory.parts.Length; i++){
            parts.Add(GameLib.Instance.GetPartById(data.inventory.parts[i]));
        }
        
        Player.Instance.parts = parts;

        List<Equipment> equipments = new List<Equipment>();
        for(int i = 0; i <data.inventory.equipment.Length; i++){
            equipments.Add(GameLib.Instance.GetEquipmentById(data.inventory.equipment[i]));
        }
        Player.Instance.equipments = equipments;

        List<Consumable> consumables = new List<Consumable>();
        for(int i = 0; i <data.inventory.consumables.Length; i++){
            consumables.Add(GameLib.Instance.GetConsumableById(data.inventory.consumables[i]));
        }
        Player.Instance.consumables = consumables;

       
        
        

     }
}
