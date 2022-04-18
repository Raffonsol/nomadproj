using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;



public class WeaponCrafting : MonoBehaviour
{
    public Image image;
    public Color color;
    public List<Part> itemsPlaced;
    public GameObject singleTemplate;
    private bool isEquippable;
    private Weapon matchingWeapon;
    // Start is called before the first frame update
    void Start()
    {
        // TODO load saved itemsPlaces
        itemsPlaced = new List<Part>();
        image = gameObject.GetComponent<Image>();
        color = image.color;
        color.a = 0;
        image.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (itemsPlaced.Count == 0) {
            if (UIManager.Instance.draggingPart) {
                color.a = 0.5f;
                image.color = color;
            } else {
                color.a = 0;
                image.color = color;
            }
        } else if (color.a != 1f) {
            color.a = 1f;
            image.color = color;
        }
        CheckForReset();
        CheckDroppedItems();
    }
    void CheckForReset() {
        if (UIManager.Instance.weaponNeedsUpdate) {
            
            UIManager.Instance.weaponNeedsUpdate = false;
            itemsPlaced = Player.Instance.activePerson.equipped.partsBeingUsed;
            CheckWeaponsForMatchingParts();
            UpdateTemplateImage();
        }
    }
    void CheckDroppedItems() {
        if (UIManager.Instance.partDroppedOnCrafting) {
            
            UIManager.Instance.partDroppedOnCrafting = false;

            Part item = UIManager.Instance.lastDroppedPart;
            // if (itemsPlaced.Count == 0 );
                
            itemsPlaced.Add(item); 

            CheckWeaponsForMatchingParts();
            UpdateTemplateImage();
            Player.Instance.RemovePart(item.id);

            if (matchingWeapon != null && isEquippable) {
                Player.Instance.EquipWeapon(matchingWeapon.id, itemsPlaced);
            } else { 
                Player.Instance.EquipWeapon(100000, itemsPlaced);
            }
        }
        if (UIManager.Instance.partRemovedFromCrafting) {
            
            UIManager.Instance.partRemovedFromCrafting = false;

            Part item = UIManager.Instance.lastDroppedPartFC;
                
            for(int i = 0; i <itemsPlaced.Count; i++){
                if (itemsPlaced[i].id == item.id) {
                    itemsPlaced.RemoveAt(i);
                    break;
                }
            }
            Player.Instance.PickupItem(ItemType.Part, item.id);

            CheckWeaponsForMatchingParts();
            UpdateTemplateImage();
            if (matchingWeapon != null && isEquippable) {
                Player.Instance.EquipWeapon(matchingWeapon.id, itemsPlaced);
            } else { 
                Player.Instance.EquipWeapon(100000, itemsPlaced);
            }
        }
    }
    void UpdateTemplateImage() {
        foreach (Transform child in transform) {
           GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < itemsPlaced.Count; i++){
            GameObject temp = Instantiate(singleTemplate, transform);
            PartLooks partLook = itemsPlaced[i].partLooks.Length > 0 && matchingWeapon != null ? 
                Array.Find(itemsPlaced[i].partLooks, look => 
                    look.weaponId == matchingWeapon.id
                ) :
                null;

            temp.GetComponent<Image>().sprite = partLook == null ?
                itemsPlaced[i].visual.GetComponent<SpriteRenderer>().sprite :
                partLook.look.GetComponent<SpriteRenderer>().sprite;
                    
            temp.GetComponent<Image>().color = itemsPlaced[i].visual.GetComponent<SpriteRenderer>().color;
            temp.GetComponent<Transform>().localScale = itemsPlaced[i].visual.GetComponent<Transform>().localScale;
            temp.GetComponent<Image>().SetNativeSize();
            if (partLook != null) {
                temp.GetComponent<PartInCrafting>().defaultPos = new Vector2(partLook.xOffset, partLook.yOffset);
                temp.GetComponent<Transform>().localRotation = Quaternion.Euler(0, 0, partLook.zOffset);
            }
            
            temp.GetComponent<PartInCrafting>().itemId = itemsPlaced[i].id;
            temp.GetComponent<PartInCrafting>().itemType = ItemType.Part;
        }
    }
    void CheckWeaponsForMatchingParts() {
        // start at index 1 to skip unarmed
        for(int i = 1; i <GameLib.Instance.allWeapons.Length; i++){
            bool soFarSoGood = true;

            List<int> usedIds = new List<int>();
            Weapon weapon = GameLib.Instance.allWeapons[i];
            for(int j = 0; j <weapon.partsNeeded.Length; j++){

                bool isPartIn = false;
                for(int k = 0; k <itemsPlaced.Count; k++){
                    if (weapon.partsNeeded[j] == itemsPlaced[k].fittablePart
                    && !usedIds.Contains(k))
                    {
                        isPartIn = true;
                        usedIds.Add(k);
                        break;
                    }else {
                        isPartIn = false;
                    }
                }
                soFarSoGood = isPartIn;
            }

            // we know that all the parts we used are matched, but now are there more?
            if (usedIds.Count != itemsPlaced.Count) {
                soFarSoGood = false;
            }

            // now we have to make sure there aren't any parts missing
            if (weapon.partsNeeded.Length != itemsPlaced.Count) {
                soFarSoGood = false;
            }
            
            if (soFarSoGood) {
                isEquippable = true;
                matchingWeapon = weapon;
                return;
            } 
        }
        // TODO Check partials
        isEquippable = false;
        matchingWeapon = null;
    }
}
