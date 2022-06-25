using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public GameObject content;
    
    public Menu menuType;
    public bool dependOnMenu;
    public Tab TabType;
    public bool dependOnTab;
    // Start is called before the first frame update
    void Start()
    {
        content = transform.Find("Content").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (content) {
            content.SetActive(
                (!dependOnMenu || UIManager.Instance.openMenu == menuType) &&
                (!dependOnTab || UIManager.Instance.openTab == TabType));
        }
    }
}
