using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftableArmor : MonoBehaviour
{
    private float checkTimer = 0f;
    public int showingItemId;
    public Sprite partNeeded1;
    public Sprite partNeeded2;
    private Equipment showingItem;
    // Start is called before the first frame update
    void Start()
    {
        showingItem = GameLib.Instance.GetEquipmentById(showingItemId);
        transform.Find("craft1").GetComponent<Button>().onClick.AddListener(() => CraftOne());
        transform.Find("craftAll").GetComponent<Button>().onClick.AddListener(() => CraftAll());
        partNeeded1 = GameLib.Instance.GetPartById(showingItem.partsNeeded[0]).icon;
        partNeeded2 = GameLib.Instance.GetPartById(showingItem.partsNeeded[1]).icon;
    }

    // Update is called once per frame
     void Update()
    {
        if (UIManager.Instance.openMenu != Menu.Inventory || UIManager.Instance.openTab != Tab.Crafting) {
            return;
        }

        if (checkTimer > 0) {
            checkTimer -= Time.deltaTime;
        } else {
            checkTimer = 0.4f;
            UpdateUI();
        }
    }

    void UpdateUI() {
        string name = showingItem.name;
        int count = 0;
        for(int i = 0; i <Player.Instance.equipments.Count; i++){
            if (Player.Instance.equipments[i].id == showingItemId) {
                count++;
            }
        }
        
        string owned = "X" + count + " Owned";
        transform.Find("name").GetComponent<TextMeshProUGUI>().text  = name;
        transform.Find("quantity").GetComponent<TextMeshProUGUI>().text  = owned;
        transform.Find("ItemsNeeded/icon1").GetComponent<Image>().sprite = partNeeded1;
        transform.Find("ItemsNeeded/icon2").GetComponent<Image>().sprite = partNeeded2;
        
    }
    bool CraftOne(bool multiple = false) {
        // check if owned
        bool part1Owned = false;
        int part1Id = 0;
        bool part2Owned = false;
        int part2Id = 0;
        for(int i = 0; i <Player.Instance.parts.Count; i++){
            if (!part1Owned && Player.Instance.parts[i].id == showingItem.partsNeeded[0]) {
                part1Owned = true;
                part1Id =Player.Instance.parts[i].id;
                
                if (part2Owned) break;
                continue;

            } 
            if (!part2Owned && Player.Instance.parts[i].id == showingItem.partsNeeded[1]) {
                part2Owned = true;
                part2Id =Player.Instance.parts[i].id;

                if (part1Owned) break;
            }
        }

        if (part1Owned && part2Owned) {
            Player.Instance.RemoveItem(ItemType.Part, part1Id);
            Player.Instance.RemoveItem(ItemType.Part, part2Id);
            Player.Instance.AddEquipment(showingItemId);
            if (!multiple)UpdateUI();
            UIManager.Instance.AutoEquipSingleArmor(showingItemId);
            return true;
        }
        return false;

    }
    void CraftAll() {
        bool oneMade = true;
        while (oneMade) {
            oneMade = CraftOne(true);
        }
        UpdateUI();
    }
}
