using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : EventTrigger 
{

    public ItemType itemType;
    public int itemId;
    public Image icon;
    public int quantity;
    private bool dragging;
    [HideInInspector]
    public Vector2 defaultPos;
    // Start is called before the first frame update
    public void LateStart()
    {
        
        ResetImage();
        
    }
    public void ResetImage(){
        icon = transform.Find("Image").gameObject.GetComponent<Image>();
        if (!icon) return;
        switch (itemType) {
            case ItemType.Equipment:
                icon.sprite = GameLib.Instance.GetEquipmentById(itemId).icon;
                break;
            case ItemType.Weapon:
                icon.sprite = GameLib.Instance.GetWeaponById(itemId).icon;
                break;
            case ItemType.Part:
                icon.sprite = GameLib.Instance.GetPartById(itemId).icon;
                break;

        }
         transform.Find("Quantity").gameObject.GetComponent<TextMeshProUGUI>().text = quantity == 1? "" : quantity.ToString();
    }

    // Update is called once per frame
    void Update()
    {
         if (dragging) {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //  transform.localPosition = new Vector2(0,0);
        } else {
            transform.position = new Vector2(0,0);
            transform.localPosition = defaultPos;
        }
    }
    public override void OnPointerDown(PointerEventData eventData) {
        dragging = true;
        // if (itemType == ItemType.Part) {
            UIManager.Instance.draggingPart = true;
        // }
        
    }

    public override void OnPointerUp(PointerEventData eventData) {
        dragging = false;
        UIManager.Instance.draggingPart = false;

        // Part crafting
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        if (UIHelper.UIOverlapCheck(raycastResults.ToArray(), "CraftDraggable")) {
            UIManager.Instance.partDroppedOnCrafting = true;

            UIManager.Instance.lastDroppedPart = GameLib.Instance.GetPartById(itemId);
        }
        if (UIHelper.UIOverlapCheck(raycastResults.ToArray(), "Equip")) {
            UIManager.Instance.partDroppedOnCrafting = true;

            UIManager.Instance.lastDroppedArmor = GameLib.Instance.GetEquipmentById(itemId);
        }
        // Debug.Log(raysastResults[1].gameObject.name);

        
    }
}
