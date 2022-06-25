using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Step
{
    public Vector3 rightHandPos;
    public Vector3 leftHandPos;

    public Vector3 weaponPos;
}

[System.Serializable]
public class Instance
{
    public Vector3 rightHandPos;
    public Vector3 leftHandPos;

    public Vector3 weaponPos;
    public string[] hide;
    
    [SerializeField]
    public Step[] attackScripts;

    // private bool performing = false;
    // private int performNumber = 0;

    // private int performStep = 0;
    // private float performStepTimer = 0;

    // // Start is called before the first frame update
    // void Start()
    // {
    //     if (rightHand)
    //     {
    //         rightHand.transform.localPosition = rightHandPos.localPosition;
    //     }
    //     if (leftHand) 
    //     {
    //         leftHand.transform.localPosition = leftHandPos.localPosition;
    //     }
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     if (performing) {
    //         performStepTimer += Time.deltaTime;
    //         if (performStepTimer > 1f) {
    //             performStepTimer = 0;
    //             for(int i = 0; i < attackScripts[performNumber].steps[performStep].Length; i+=5){
    //                 // go through every 2 characters
    //                 switch (attackScripts[performNumber].steps[performStep].Substring(i, 2)) {
    //                     case "rh":
    //                         Debug.Log(attackScripts[performNumber].steps[performStep].Substring(i + 2, 2));
    //                     break;
    //                     case "lh":
    //                     break;
    //                     case "wh":
    //                     break;
    //                 }
    //             } 
    //             performStep++;
    //         }


            
    //     }
    // }
    // public void Attack(int attackNumber) 
    // {
    //     performNumber = attackNumber;
    //     performing = true;
    // }
}
