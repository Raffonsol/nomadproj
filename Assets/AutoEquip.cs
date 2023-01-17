using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoEquip : MonoBehaviour
{

    public GameObject xMark;
    public bool isOn = false;

    private bool wasOff = true;

    private float countTimer = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => OnButtonClick());
    }

    // Update is called once per frame
    void Update()
    {
        if (countTimer > 0) {
            countTimer -= Time.deltaTime;
        }
        else {
            countTimer = 5f;
            isOn = UIManager.Instance.autoEquipping;
            UpdateUI();
        }
        
    }
    void OnButtonClick() {
        isOn = !isOn;
        wasOff = isOn;
        UIManager.Instance.autoEquipping = isOn;
        UpdateUI();
    }
    void UpdateUI() {
        xMark.SetActive(!isOn);
        if (isOn && wasOff) {
            UIManager.Instance.CheckAutoEquips();
            wasOff = false;
        }
    }
}
