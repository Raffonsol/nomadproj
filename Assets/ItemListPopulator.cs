using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemListPopulator : MonoBehaviour
{
    public ItemType itemType;
    public GameObject itemPrefab;
    // Start is called before the first frame update
    void Start()
    {
        ResetList();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Add check that this is the active populator
        if (UIManager.Instance.tabRefresh) {
            UIManager.Instance.tabRefresh = false;
            ResetList();
        }   
    }
    async void ResetList(){
         foreach (Transform child in transform) {
           GameObject.Destroy(child.gameObject);
        }
        int xm = 0;
        int ym = 0;
        if (itemType == ItemType.Part){
            for(int i = 0; i <Player.Instance.parts.Count; i++, xm++){
                
                if (xm > 6) {
                    xm = 0;
                    ym++;
                }
                GameObject icon = Instantiate(itemPrefab, transform);
                Vector2 newPos = transform.localPosition;
                newPos.x += -120f + 40f*xm;
                newPos.y += 100f - 40f *ym;
                icon.transform.localPosition = newPos;
                icon.GetComponent<InventoryItem>().defaultPos = newPos;
                icon.GetComponent<InventoryItem>().itemType = itemType;
                icon.GetComponent<InventoryItem>().itemId = Player.Instance.parts[i].id;
                icon.GetComponent<InventoryItem>().ResetImage();
            }
            
        }
    }
}
