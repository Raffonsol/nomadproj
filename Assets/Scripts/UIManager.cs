using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Menu
{
    System,
    Inventory,
}
public enum Tab
{
    Armor,
    Weapons,
    Potions,
    Food,
    Crafting,
    Skills,
    Development,
    Map,
    Army
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public Menu? openMenu;
    public Tab openTab = Tab.Armor;
    public bool tabRefresh = false;
    public bool draggingPart = false;
    // Last part thatw as dropped on UI
    public Part lastDroppedPart;
    // FC = from crafting
    public bool draggingPartFC = false;
    // Last part thatw as dropped on UI
    public Part lastDroppedPartFC;
    public bool partDroppedOnCrafting = false;
    public bool partRemovedFromCrafting = false;
    
    private int UILayer;
    // Singleton stuff
    private void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }
    void Start()
    {
        UILayer = LayerMask.NameToLayer("UI");
    }

    public void OpenMenu(Menu menu)
    {
        if (openMenu == menu) {
            openMenu = null;
        } else {
            openMenu = menu;
        }
        
    }
    public void SetOpenTab(Tab tab)
    {
        openTab = tab;
        tabRefresh = true;
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
 
 
    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }
 
 
    //Gets all event system raycast results of current mouse or touch position.
    public static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
