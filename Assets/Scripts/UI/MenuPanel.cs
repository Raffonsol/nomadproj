using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuPanel : MonoBehaviour, IPointerExitHandler
{
    public GameObject content;
    
    public Menu menuType;
    public bool dependOnMenu;
    public Tab TabType;
    public Tab secondaryTab;
    public bool dependOnTab;

    private bool pageChangerChecked = false;
    // Start is called before the first frame update
    void Start()
    {
        content = transform.Find("Content").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (content != null ) {
            bool wasActive = content.activeSelf;
            content.SetActive(
                (!dependOnMenu || UIManager.Instance.openMenu == menuType) &&
                (!dependOnTab || UIManager.Instance.openTab == TabType || UIManager.Instance.openTab == secondaryTab));
            // if (content.activeSelf && !wasActive) {
            //     UIManager.Instance.uIPage = 0;
            // }
        }
        if (!pageChangerChecked)
        {
            pageChangerChecked = true;
            Transform pageUp = content.transform.Find("arrowUp");
            if (pageUp == null) return;
            Transform pageDown = content.transform.Find("arrowDown");
            pageUp.GetComponent<Button>().onClick.AddListener(() => UIManager.Instance.PageChange(-1));
            pageDown.GetComponent<Button>().onClick.AddListener(() => UIManager.Instance.PageChange(1));
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideToolTips();
    }
}
