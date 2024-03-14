using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CraftableArmor : MonoBehaviour, IPointerEnterHandler
{
    private float checkTimer = 0f;
    public int[] showingItemId;
    public Sprite partNeeded1;
    public Sprite partNeeded2;

    private Equipment showingItem;
    private int page = 0;
    // Start is called before the first frame update
    void Start()
    {
        transform.Find("craft1").GetComponent<Button>().onClick.AddListener(() => CraftOne());
        transform.Find("craftAll").GetComponent<Button>().onClick.AddListener(() => CraftAll());
        Reset();
    }
    void Reset()
    {
        page = UIManager.Instance.uIPage;
        if (page > showingItemId.Length-1) return;
        showingItem = GameLib.Instance.GetEquipmentById(showingItemId[page]);
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
            Reset();
            UpdateUI();
        }
    }

    void UpdateUI() { 
        if (page > showingItemId.Length-1) {
            page = showingItemId.Length-1;
            UIManager.Instance.uIPage = page;
            return;
        }
        string name = showingItem.name;
        int count = 0;
        for(int i = 0; i <Player.Instance.equipments.Count; i++){
            if (Player.Instance.equipments[i].id == showingItemId[page]) {
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
            Player.Instance.AddEquipment(showingItemId[page]);
            if (!multiple)UpdateUI();
            UIManager.Instance.AutoEquipSingleArmor(showingItemId[page]);
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
    public void OnPointerEnter(PointerEventData eventData)
    {
        string name1 = GameLib.Instance.GetPartById(showingItem.partsNeeded[0]).name;
        string name2 = GameLib.Instance.GetPartById(showingItem.partsNeeded[1]).name;
        if (name1.Equals(name2)) {
            UIManager.Instance.ShowDetailedToolTip(new Vector2(transform.position.x+1.2f,transform.position.y-152f),
                showingItem.name,"", "Needs two pieces of "+name1);
        } else
            UIManager.Instance.ShowDetailedToolTip(new Vector2(transform.position.x+1.2f,transform.position.y-152f),
                showingItem.name,"", "Needs one piece of "+name1+", and one of "+name2);
        
    }

}
