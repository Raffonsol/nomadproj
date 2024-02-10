using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using UnityEngine.UI;
using TMPro;

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
    Parts,
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
    
    public bool autoEquipping = false;

    public bool tabRefresh = false;
    public bool draggingPart = false;
    // Last part thatw as dropped on UI
    public Part lastDroppedPart;
    public Equipment lastDroppedArmor;
    // FC = from crafting
    public bool draggingPartFC = false;
    // Last part thatw as dropped on UI
    public Part lastDroppedPartFC;
    public Equipment lastDroppedArmorFC;
    public bool lastDroppedArmorFCIsLeft = false;
    public bool partDroppedOnCrafting = false;
    public bool partRemovedFromCrafting = false;

    public bool armorNeedsUpdate = false;
    public bool weaponNeedsUpdate = false;
    
    private int UILayer;
    public int TrasnparentLayer;

    public List<int> lvlUpQueue;

    public GameObject directionArrow;
    public List<Vector2> villageLocations;

    public GameObject itemAddedPanel;
    public float newItemTimer=0f;
    public Sprite recruitedIcon;

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
        villageLocations = new List<Vector2>();
    }
    void Start()
    {
        lvlUpQueue = new List<int>();
        UILayer = LayerMask.NameToLayer("UI");
        TrasnparentLayer = LayerMask.NameToLayer("TransparentFX");
        itemAddedPanel.SetActive(false);
    }
    void FixedUpdate()
    {   
        Vector2 nextPoint = villageLocations[0];
        Vector2 moveDirection = nextPoint - (Vector2)Camera.main.transform.position;
        moveDirection.Normalize();
        // transform.position = Vector3.Lerp (Camera.main.transform.position, target, runSpeed * Time.deltaTime);

        float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg + 80f;
        directionArrow.transform.rotation = Quaternion.Slerp (directionArrow.transform.rotation, 
                                        Quaternion.Euler (0, 0, targetAngle + 180), 
                                        3f * Time.deltaTime);

        if (newItemTimer > 0) {
            newItemTimer-=Time.deltaTime;
            if (newItemTimer <= 0) {
                itemAddedPanel.SetActive(false);
            }
        }
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
    // makes sure menu is open on the selected tab no matter what
    public void ShowTab(Tab tab) {
        openMenu = Menu.Inventory;
        SetOpenTab(tab);
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

    public void CheckAutoEquips() {
        // armor
        AutoEquipArmor();

        // weapon parts
        AutoEquipWeapons();
        
        
    }
    public void AutoEquipArmor() {
        if (!autoEquipping) return;

        for(int i = 0; i <Player.Instance.equipments.Count; i++){
            Debug.Log("Equipping the " + Player.Instance.equipments[i].name);
            bool didIt = AutoEquipSingleArmor(Player.Instance.equipments[i].id);
            if (didIt) i--;
        }
        

    }
    public bool AutoEquipSingleArmor(int armorId) {
        if (!autoEquipping) return false;

        Equipment armor = GameLib.Instance.GetEquipmentById(armorId);

        // go through characters and find who needs equipment of this type
        for(int i = 0; i <Player.Instance.characters.Count; i++){
            bool canEquip = false;
            bool left = false;
            switch (armor.slot) {
                case (Slot.Head):
                if (Player.Instance.characters[i].equipped.head == null) {
                    canEquip = true;
                }
                break;
                case (Slot.Chest):
                if (Player.Instance.characters[i].equipped.chest == null) {
                    canEquip = true;
                }
                break;
                case (Slot.Pauldron):
                if (Player.Instance.characters[i].equipped.rightPauldron == null) {
                    canEquip = true;
                }
                else if (Player.Instance.characters[i].equipped.leftPauldron == null) {
                    canEquip = true;
                    left = true;
                }
                break;
                case (Slot.Hand):
                if (Player.Instance.characters[i].equipped.rightHand == null) {
                    canEquip = true;
                }
                else if (Player.Instance.characters[i].equipped.leftHand == null) {
                    canEquip = true;
                    left = true;
                }
                break;
                case (Slot.Foot):
                if (Player.Instance.characters[i].equipped.rightFoot == null) {
                    canEquip = true;
                }
                else if (Player.Instance.characters[i].equipped.leftFoot == null) {
                    canEquip = true;
                    left = true;
                }
                break;
            }
            if (canEquip) {
                Player.Instance.EquipArmor(armor, left, Player.Instance.characters[i].id);
                Player.Instance.RemoveEquipment(armorId);
                armorNeedsUpdate = true;
                return true;
            }
        }
        return false;
    }
    public void AutoEquipWeapons(int count = -1) {
        if (!autoEquipping) return;
        bool somethingHappened = false;

        int qualifier = -1;
        bool partHaving = false;
        // look for all with no weapons
        for(int i = 0; i <Player.Instance.characters.Count; i++){
            if ( Player.Instance.characters[i].equipped.primaryWeapon == null 
            ||Player.Instance.characters[i].equipped.primaryWeapon.id == 100000) {
                // qualified for an autoequip
                qualifier = i;
                partHaving = false;
                // in case they have some parrts on the builder not resulting in anything
                Player.Instance.ClearPartsBeingUsed(Player.Instance.characters[i].id);
                break;
            }
            else if (Player.Instance.characters[i].equipped.partsBeingUsed.Count == 1) {
                // qualified for an autoequip
                qualifier = (i);
                partHaving = (true);
                break;
            }
        }
        
        // turn list of parts into list of FittablePart enum
        List<FittablePart> partTypesOwned = Player.Instance.parts.Select( x => x.fittablePart).ToList();
        if (qualifier == -1) return;
        
        // check for weapons that can be made. Start at 1 to skip disarmed
        for(int i = 1; i <GameLib.Instance.allWeapons.Length; i++){
            // don't look if it's the equipped weapon
            if (Player.Instance.characters[qualifier].equipped.primaryWeapon != null && GameLib.Instance.allWeapons[i].id == Player.Instance.characters[qualifier].equipped.primaryWeapon.id) continue; 
            bool soFarSoGood = true;
            
            // make sure we are looking at a list of the correct parts to check fi they are available
            List<FittablePart> partPool = new List<FittablePart>();
            partPool.AddRange(partTypesOwned);
            if (partHaving) partPool.Add(Player.Instance.characters[qualifier].equipped.partsBeingUsed[0].fittablePart);

            // check that all needed materials are owned
            for(int j = 0; j <GameLib.Instance.allWeapons[i].partsNeeded.Length; j++){

                if (!partPool.Contains(GameLib.Instance.allWeapons[i].partsNeeded[j])){
                    soFarSoGood = false;
                    break; // part missing, so stop looking at parts
                    // NOTE!!! this assumes that every weapon only uses at most 1 of each part type
                }
            }
            if (!soFarSoGood){
                continue; // part missing, so skip this weapon as a possibility
            }
            // we have all parts, so this weapon is getting equipped
            somethingHappened = true;
            int id = Player.Instance.characters[qualifier].id;
            if (partHaving) {
                // if they alredy had 1 part, just re-add it them to make things simple
                int partId = Player.Instance.characters[qualifier].equipped.partsBeingUsed[0].id;
                Player.Instance.AddPart(partId);
            }
            Player.Instance.EquipWeapon(
                GameLib.Instance.allWeapons[i].id,
                Player.Instance.FindNeededParts(GameLib.Instance.allWeapons[i].partsNeeded),
                id
            );
            
            // remove part from clone list so it doesnt equip on multiple people
            for(int h = 0; h <GameLib.Instance.allWeapons[i].partsNeeded.Length; h++){
                partTypesOwned.Remove(GameLib.Instance.allWeapons[i].partsNeeded[h]);
            }
            // 0--;

            weaponNeedsUpdate = true;
            // if (0 < 0) return;
            break; // so it doesn't equip the same weapon on multiple people
        }
        // If a weapon got equipped, run again because it might be able to get even better
        if (somethingHappened && count <4){ 
            AutoEquipWeapons(++count);
        }
    }
    public void ShowItemPickedUp(string itemName, Sprite icon) {
        ShowIndicator( "+ 1 "+itemName, icon);
    }
    public void ShowIndicator(string text, Sprite icon) {
        itemAddedPanel.SetActive(true);
        itemAddedPanel.transform.Find("text").GetComponent<TextMeshProUGUI>().text = text;
        itemAddedPanel.transform.Find("icon").GetComponent<Image>().sprite = icon;
        newItemTimer = 5f;
    }
}
