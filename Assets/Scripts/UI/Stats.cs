using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Stats : MonoBehaviour
{
    public bool isStat; // otherwise its skills

    private float checkTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (UIManager.Instance.openMenu != Menu.Inventory || UIManager.Instance.openTab != Tab.Skills) {
            return;
        }

        if (checkTimer > 0) {
            checkTimer -= Time.deltaTime;
        } else {
            checkTimer = 0.3f;
            UpdateUI();
        }
    }
    void UpdateUI() {
        string statText ="";
        if (isStat) {
            statText = "Life: " +Player.Instance.activePerson.life.ToString("0.0") +"/"+ Player.Instance.activePerson.stats[0].value +
            "\nLife Regen: " + Player.Instance.activePerson.stats[9].value + 
            "\nArmor: " + Player.Instance.activePerson.stats[12].value + 
            "\nMelee Damage: " + Player.Instance.activePerson.stats[3].value + 
            "\nRanged Damage: " + Player.Instance.activePerson.stats[4].value
            ;

        } else {
            for(int i = 0; i <Player.Instance.activePerson.bonuses.Count; i++){
                Bonus bonus =Player.Instance.activePerson.bonuses[i];
                statText += " "+ bonus.name + ": \t" + bonus.description + "\n";
            }
            
        }
         GetComponent<TextMeshProUGUI>().text = statText;
    }
}
