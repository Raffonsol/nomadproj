using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int skillKey;
    bool mouse_over = false;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        if (skillKey>1)
        transform.Find("CD").GetComponent<TextMeshProUGUI>().text = Player.Instance.activePerson.skills[skillKey-2].cooldownTimer > 0 
            ?Player.Instance.activePerson.skills[skillKey-2].cooldownTimer.ToString("0.0") : "";
        else
        transform.Find("CD").GetComponent<TextMeshProUGUI>().text = Player.Instance.activePerson.controller.attackCooldownTimer > 0 
            ?Player.Instance.activePerson.controller.attackCooldownTimer.ToString("0.0") : "";
    }
    void ButtonClicked()
    {
        Player.Instance.activePerson.controller.TriggerSkill(skillKey);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
        if (skillKey>1) {
            CharSkill skill = Player.Instance.activePerson.skills[skillKey-2];
            if (skill.id == 0) {
                // weapon skill
                skill=GameLib.Instance.getWeaponsSkill(Player.Instance.activePerson.equipped.primaryWeapon.id);
            } 
            UIManager.Instance.ShowDetailedToolTip(new Vector2(transform.position.x+100f,transform.position.y+120f), skill.name, skill.cooldown+"s cooldown", skill.description);
        } else {
            Weapon wep = Player.Instance.activePerson.equipped.primaryWeapon;
            UIManager.Instance.ShowDetailedToolTip(new Vector2(transform.position.x+100f,transform.position.y+120f), wep.name, wep.cooldown+"s cooldown", wep.handsNeeded+" hands needed");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false;
        UIManager.Instance.HideToolTips();
    }
}
