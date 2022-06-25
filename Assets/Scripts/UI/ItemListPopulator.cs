using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemListPopulator : MonoBehaviour
{
    public ItemType itemType;
    public GameObject itemPrefab;
    public Tab tab;
    // Start is called before the first frame update
    void Start()
    {
        ResetList();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Add check that this is the active populator
        if (UIManager.Instance.tabRefresh && UIManager.Instance.openTab==tab) {
            UIManager.Instance.tabRefresh = false;
            ResetList();
        }   
    }
    public void ResetList(){
         foreach (Transform child in transform) {
           GameObject.Destroy(child.gameObject);
        }
        int xm = 0;
        int ym = 0;
        List<int> idsAdded = new List<int>();
        List<InventoryItem> itemsAdded = new List<InventoryItem>();
        List<Item> itemArray;
        if (itemType == ItemType.Part) itemArray = new List<Item>(Player.Instance.parts);
        else if (itemType == ItemType.Equipment) itemArray = new List<Item>(Player.Instance.equipments);
        else itemArray = new List<Item>();

            for(int i = 0; i <itemArray.Count; i++, xm++){
                if (idsAdded.Contains(itemArray[i].id)) {
                    xm -=1; //go back 1
                    InventoryItem toInc = itemsAdded.Find(item => item.itemId==itemArray[i].id);
                    toInc.quantity +=1;
                    toInc.ResetImage();
                } else {

                

                    // at this point we are adding a new item to the list
                    idsAdded.Add(itemArray[i].id);
                    
                    if (xm > 6) {
                        xm = 0;
                        ym++;
                    }
                    GameObject icon = Instantiate(itemPrefab, transform);
                    itemsAdded.Add(icon.GetComponent<InventoryItem>());

                    Vector2 newPos = transform.localPosition;
                    newPos.x += -120f + 40f*xm;
                    newPos.y += 100f - 40f *ym;
                    icon.transform.localPosition = newPos;
                    icon.GetComponent<InventoryItem>().quantity = 1;
                    icon.GetComponent<InventoryItem>().defaultPos = newPos;
                    icon.GetComponent<InventoryItem>().itemType = itemType;
                    icon.GetComponent<InventoryItem>().itemId = itemArray[i].id;
                    icon.GetComponent<InventoryItem>().ResetImage();
                }
            }
            
        // if (itemType == ItemType.Part){
        //     for(int i = 0; i <Player.Instance.parts.Count; i++, xm++){
        //         if (idsAdded.Contains(Player.Instance.parts[i].id)) {
        //             xm -=1; //go back 1
        //             InventoryItem toInc = itemsAdded.Find(item => item.itemId==Player.Instance.parts[i].id);
        //             toInc.quantity +=1;
        //             toInc.ResetImage();
        //         } else {

                

        //             // at this point we are adding a new item to the list
        //             idsAdded.Add(Player.Instance.parts[i].id);
                    
        //             if (xm > 6) {
        //                 xm = 0;
        //                 ym++;
        //             }
        //             GameObject icon = Instantiate(itemPrefab, transform);
        //             itemsAdded.Add(icon.GetComponent<InventoryItem>());

        //             Vector2 newPos = transform.localPosition;
        //             newPos.x += -120f + 40f*xm;
        //             newPos.y += 100f - 40f *ym;
        //             icon.transform.localPosition = newPos;
        //             icon.GetComponent<InventoryItem>().quantity = 1;
        //             icon.GetComponent<InventoryItem>().defaultPos = newPos;
        //             icon.GetComponent<InventoryItem>().itemType = itemType;
        //             icon.GetComponent<InventoryItem>().itemId = Player.Instance.parts[i].id;
        //             icon.GetComponent<InventoryItem>().ResetImage();
        //         }
        //     }
            
        // }
    }
}
