using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaponGraphicsUpdater
{
    public static void UpdateWeaponGraphic(Weapon weapon, List<Part> partsUsed, GameObject weapObj) {
        for(int i = 0; i <partsUsed.Count; i++) {
            
            // Debug.Log(partsUsed[i].fittablePart.ToString() + " -|- " + weapObj.name);
            GameObject partObj = weapObj.transform.Find(partsUsed[i].fittablePart.ToString()).gameObject;
            GameObject newPartObj = partsUsed[i].visual;
            partObj.GetComponent<SpriteRenderer>().sprite = newPartObj.GetComponent<SpriteRenderer>().sprite;
            partObj.GetComponent<SpriteRenderer>().color = newPartObj.GetComponent<SpriteRenderer>().color;
            partObj.transform.localScale = newPartObj.transform.localScale;
        }
        
    }
}