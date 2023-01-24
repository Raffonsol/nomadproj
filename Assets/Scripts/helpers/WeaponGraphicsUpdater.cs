using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponGraphicsUpdater
{
    public static void UpdateWeaponGraphic(Weapon weapon, List<Part> partsUsed, GameObject weapObj) {
        if (weapon.id == 100000) return;
        for(int i = 0; i <partsUsed.Count; i++) {
            
            // if (partsUsed[i].partLooks.Length > 0){
            //      Debug.Log(partsUsed[i].partLooks[0].weaponId+ "---" +  weapon.id);
            //       Debug.Log( Array.Find(partsUsed[i].partLooks, look => look.weaponId == weapon.id).weaponId);
            // }
            GameObject newPartObj;
            try {
                // try to use part from part looks. If that doesnt work..
                newPartObj = 
                partsUsed[i].partLooks.Length > 0 &&
                weapon!=null ?
                    Array.Find(partsUsed[i].partLooks, look => look.weaponId == weapon.id).look :
                    partsUsed[i].visual;
            } catch (NullReferenceException) {
                // means part is not there so just use visual property
                newPartObj = partsUsed[i].visual;
            }
            try {
                // try to get existing part and make it look how it should
                GameObject partObj = weapObj.transform.Find(partsUsed[i].fittablePart.ToString()).gameObject;

                partObj.GetComponent<SpriteRenderer>().sprite = newPartObj.GetComponent<SpriteRenderer>().sprite;
                partObj.GetComponent<SpriteRenderer>().color = newPartObj.GetComponent<SpriteRenderer>().color;
                partObj.transform.localScale = newPartObj.transform.localScale;
                partObj.transform.localPosition = newPartObj.transform.localPosition;
            } catch (NullReferenceException) {
                // if couldnt find part then we need to instantiate it
                
                Debug.Log("Instantiating " + newPartObj.name);
                Debug.Log("On wea pon " + weapObj.name);
                GameObject.Instantiate(newPartObj, weapObj.transform);
            }
            
        }
        
    }
    // public static void UpdateEquipmentGraphic(Equipment equipment, bool left = false) {
    //     if (equipment.id == 0) return;
            
        
    //     if (equipment.slot == Slot.Head) {

    //     }
    //         // if (partsUsed[i].partLooks.Length > 0){
    //         //      Debug.Log(partsUsed[i].partLooks[0].weaponId+ "---" +  weapon.id);
    //         //       Debug.Log( Array.Find(partsUsed[i].partLooks, look => look.weaponId == weapon.id).weaponId);
    //         // }
    //         GameObject newPartObj = 
    //             partsUsed[i].partLooks.Length > 0 &&
    //             weapon!=null ?
    //                 Array.Find(partsUsed[i].partLooks, look => look.weaponId == weapon.id).look :
    //                 partsUsed[i].visual;
    //         try {
    //             GameObject partObj = weapObj.transform.Find(partsUsed[i].fittablePart.ToString()).gameObject;
            
    //             partObj.GetComponent<SpriteRenderer>().sprite = newPartObj.GetComponent<SpriteRenderer>().sprite;
    //             partObj.GetComponent<SpriteRenderer>().color = newPartObj.GetComponent<SpriteRenderer>().color;
    //             partObj.transform.localScale = newPartObj.transform.localScale;
    //         } catch (NullReferenceException) {
    //             GameObject.Instantiate(newPartObj, weapObj.transform);
    //         }
            
        
    // }
}