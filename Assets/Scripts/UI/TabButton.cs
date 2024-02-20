using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Tab tabToOpen;

    private Color unselected;
    private Color selected;
    // Start is called before the first frame update
    void Start()
    {
        unselected = gameObject.GetComponent<Image>().color;
        selected = unselected;
        selected.a = 0.5f;
        gameObject.GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        if (UIManager.Instance.openTab == tabToOpen) {
            gameObject.GetComponent<Image>().color = selected;
        } else {
            gameObject.GetComponent<Image>().color = unselected;
        }
    }
    void ButtonClicked()
    {
        UIManager.Instance.SetOpenTab(tabToOpen);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.ShowSimpleToolTip(transform.position*0.965f, tabToOpen.ToString());
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideToolTips();
    }
}
