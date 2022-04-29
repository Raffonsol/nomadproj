using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public enum ShowingStat
{
    Level,
    Exp,
}

public class UIValueUpdater : MonoBehaviour
{

    public ShowingStat showing;
    private Player player;
    private float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (UIManager.Instance.openMenu != null) {
            if (timer > 0) {
            timer -= Time.deltaTime;
        } else {
            timer = 3f;
            ReloadStat();
        }
        }
    }
    void ReloadStat() {
        string value = "";
        switch(showing) {
            case (ShowingStat.Exp):
                player.activePerson.experienceToNextLevel = player.activePerson.experienceToFirstLevel + player.activePerson.level * player.activePerson.experienceIncrement;
                value = player.activePerson.experience + "/" + player.activePerson.experienceToNextLevel;
                break;
            case (ShowingStat.Level):
                value = ""+ player.activePerson.level;
                break;
        }
        GetComponent<TextMeshProUGUI>().text = value;
    }
}
