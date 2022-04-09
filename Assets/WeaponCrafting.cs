using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



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
        }
       
        if (UIManager.Instance.partDroppedOnCrafting) {
            
            UIManager.Instance.partDroppedOnCrafting = false;

            Part item = UIManager.Instance.lastDroppedPart;
            // if (itemsPlaced.Count == 0 );
                
            itemsPlaced.Add(item); 
            color.a = 1f;
            image.color = color;

            CheckWeaponsForMatchingParts();

            if (matchingWeapon != null) {

                UpdateTemplateImage();
                Player.Instance.RemovePart(item.id);

                if (isEquippable) {
                    Player.Instance.EquipWeapon(matchingWeapon.id, itemsPlaced);
                }
            }
        }
    }
    void UpdateTemplateImage() {
        for(int i = 0; i < itemsPlaced.Count; i++){
            GameObject temp = Instantiate(singleTemplate, transform);
            temp.GetComponent<Image>().sprite = itemsPlaced[i].visual.GetComponent<SpriteRenderer>().sprite;
            temp.GetComponent<Image>().color = itemsPlaced[i].visual.GetComponent<SpriteRenderer>().color;
            temp.GetComponent<Transform>().localScale = itemsPlaced[i].visual.GetComponent<Transform>().localScale;
            temp.GetComponent<Image>().SetNativeSize();
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
                    }
                }
                soFarSoGood = isPartIn;
            }
            
            if (soFarSoGood) {
                isEquippable = true;
                // Debug.Log("MATCHED " + weapon.name);
                matchingWeapon = weapon;
                return;
            } 
        }
        // TODO Check partials
        isEquippable = false;
        matchingWeapon = null;
    }
}
