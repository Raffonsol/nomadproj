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
    Projectiles,
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
    public List<GameObject> monsterArrows;
    public List<GameObject> monsterArrowTargets;
    public GameObject monsterArrowPrefab;
    public GameObject friendArrowPrefab;

    public GameObject itemAddedPanel;
    public float newItemTimer=0f;
    public Sprite recruitedIcon;

    public GameObject[] skillSquares;

    public GameObject smallToolTipPrefab;
    public GameObject detailedToolTipPrefab;

    public GameObject exitGameButton;

    private float toolTipTime=0.03f;
    private float toolTipTimer=0;
    private int toolTipStep=0;

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
        monsterArrows = new List<GameObject>();
        monsterArrowTargets = new List<GameObject>();
        exitGameButton.GetComponent<Button>().onClick.AddListener(() => ExitGame());
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
        ProcessIndicatorArrows();

        if (newItemTimer > 0) {
            newItemTimer-=Time.deltaTime;
            if (newItemTimer <= 0) {
                itemAddedPanel.SetActive(false);
            }
        }


        if (toolTipStep==1) {
            toolTipTimer-=Time.deltaTime;
            if (toolTipTimer<0) {
                toolTipTimer=toolTipTime;
                toolTipStep=2;
                detailedToolTipPrefab.GetComponent<ContentSizeFitter>().verticalFit =ContentSizeFitter.FitMode.MinSize;
            }
        }
        if (toolTipStep==2) {
            toolTipTimer-=Time.deltaTime;
            if (toolTipTimer<0) {
                toolTipStep=0;
                detailedToolTipPrefab.GetComponent<ContentSizeFitter>().verticalFit =ContentSizeFitter.FitMode.PreferredSize;
            }
        }
    }

    void ProcessIndicatorArrows() {
        Vector2 nextPoint = villageLocations[0];
        UpdateDirectionArrow(nextPoint, directionArrow, true,false);
        for (int i = 0; i < monsterArrows.Count; i++)
        {
            try {
                UpdateDirectionArrow(monsterArrowTargets[i].transform.position, monsterArrows[i], false,true);
            } catch (MissingReferenceException) {
                RemoveMonsterIndicator(i);
                return;
            } catch (NullReferenceException) {
                RemoveMonsterIndicator(i);
                return;
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
            bool didIt = AutoEquipSingleArmor(Player.Instance.equipments[i].id);
            if (didIt) i--;
        }
        

    }
    public bool AutoEquipSingleArmor(int armorId) {
        if (!autoEquipping) return false;

        Equipment armor = GameLib.Instance.GetEquipmentById(armorId);
        List<int> tried = new List<int>();
        // go through characters and find who needs equipment of this type
        int i = 0;
        for(int ind = 0; ind <Player.Instance.characters.Count; ind++){
            bool canEquip = false;
            bool left = false;
            do {
                i = UnityEngine.Random.Range(0, Player.Instance.characters.Count);
            } while (tried.Contains(i) && tried.Count < Player.Instance.characters.Count);
            tried.Add(i);
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

        {
        List<int> tried = new List<int>();
        // look for all with no weapons
        int i = 0;
        for(int ind = 0; ind <Player.Instance.characters.Count; ind++){
            int mr = 40;
            do {
                mr--;
                i = UnityEngine.Random.Range(0, Player.Instance.characters.Count);
            } while (tried.Contains(i) && tried.Count < Player.Instance.characters.Count && mr>0);
            tried.Add(i);

            
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
        }
        // turn list of parts into list of FittablePart enum
        List<FittablePart> partTypesOwned = Player.Instance.parts.Select( x => x.fittablePart).ToList();
        if (qualifier == -1) return;
        
        List<Weapon> sortedAllWeapons = new List<Weapon>(GameLib.Instance.allWeapons);
        sortedAllWeapons.Shuffle();
        // check for weapons that can be made. Start at 1 to skip disarmed
        for(int i = 1; i <sortedAllWeapons.Count; i++){
            // don't look if it's the equipped weapon
            if (Player.Instance.characters[qualifier].equipped.primaryWeapon != null && sortedAllWeapons[i].id == Player.Instance.characters[qualifier].equipped.primaryWeapon.id) continue; 
            bool soFarSoGood = true;
            
            // make sure we are looking at a list of the correct parts to check fi they are available
            List<FittablePart> partPool = new List<FittablePart>();
            partPool.AddRange(partTypesOwned);
            if (partHaving) partPool.Add(Player.Instance.characters[qualifier].equipped.partsBeingUsed[0].fittablePart);

            // check that all needed materials are owned
            for(int j = 0; j <sortedAllWeapons[i].partsNeeded.Length; j++){

                if (!partPool.Contains(sortedAllWeapons[i].partsNeeded[j])){
                    soFarSoGood = false;
                    break; // part missing, so stop looking at parts
                    // NOTE!!! this assumes that every weapon only uses at most 1 of each part type
                }
            }
            // check you have ammo it may need
            if (sortedAllWeapons[i]. damageType == DamageType.Ranged) {
                if ((sortedAllWeapons[i].ammo == ConsumableType.Rock && Player.Instance.GetPartsByType(sortedAllWeapons[i].ammo).Count < 1 )
                 || Player.Instance.GetConsumablesByType(sortedAllWeapons[i].ammo).Count <1) { // no ammo of needed type
                    soFarSoGood = false; // ranged weapon works but you dont have ammo for it
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
                sortedAllWeapons[i].id,
                Player.Instance.FindNeededParts(sortedAllWeapons[i].partsNeeded),
                id
            );
            
            // remove part from clone list so it doesnt equip on multiple people
            for(int h = 0; h <sortedAllWeapons[i].partsNeeded.Length; h++){
                partTypesOwned.Remove(sortedAllWeapons[i].partsNeeded[h]);
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
    public void UpdateSkillSquare() {
        FriendlyChar person = Player.Instance.activePerson;
        skillSquares[0].transform.Find("Text").GetComponent<Image>().sprite = person.equipped.primaryWeapon.icon;
        // additional skills
        if (person.skills!=null && person.skills.Count>0){
            if (!skillSquares[1].activeSelf && person.equipped.primaryWeapon.id!=100000){
                skillSquares[1].SetActive(true);
                CharSkill addSkill =person.skills[0];
                if (person.skills[0].id == 0) {
                    // weapon skill
                    addSkill=GameLib.Instance.getWeaponsSkill(person.equipped.primaryWeapon.id);
                    Debug.Log(addSkill.name);
                } 
                skillSquares[1].transform.Find("Text").GetComponent<Image>().sprite = addSkill.icon;
            }
        } else skillSquares[1].SetActive(false);
    }
    public void UpdateDirectionArrow(Vector2 nextPoint, GameObject arrow, bool rotate, bool changeColor) {

        if (rotate){

            Vector2 moveDirection = nextPoint - (Vector2)Camera.main.transform.position;
            moveDirection.Normalize();
            float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg + 80f;
            arrow.transform.rotation = Quaternion.Slerp (arrow.transform.rotation, 
                                            Quaternion.Euler (0, 0, targetAngle + 180), 
                                            3f * Time.deltaTime);
        
        
        }
        float borderSize = 40f;

        Vector3 targetPositionScreenPoint = Camera.main.WorldToScreenPoint (nextPoint);
        bool isOffscreen = targetPositionScreenPoint.x <= borderSize || targetPositionScreenPoint.x >= Screen.width - borderSize || targetPositionScreenPoint.y <= borderSize || targetPositionScreenPoint.y >= Screen.height - borderSize;
        // Debug.Log (isOffscreen + " " + targetPositionScreenPoint);
        
        if (isOffscreen){
            Vector3 cappedTargetScreenPosition = targetPositionScreenPoint;
            cappedTargetScreenPosition.x = Mathf.Clamp (cappedTargetScreenPosition.x, borderSize, Screen.width - borderSize);
            cappedTargetScreenPosition.y = Mathf.Clamp (cappedTargetScreenPosition.y, borderSize, Screen.height - borderSize);

            // Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint (targetPositionScreenPoint);
            arrow.transform.position = cappedTargetScreenPosition;
            arrow.transform.localPosition = new Vector3 (arrow.transform.localPosition.x, arrow.transform.localPosition.y, 0f);
        } else {
            arrow.transform.position = new Vector2(-500f,-500f);
        }
        
        if (changeColor) {
            float transparency = (90f - Vector2.Distance(arrow.transform.position,Camera.main.transform.position)/10f)/10f;
            if (transparency<0)transparency=0;if(transparency>1f)transparency=1f;
            // Debug.Log(transparency + "-"+Vector2.Distance(arrow.transform.position,Camera.main.transform.position) );
            Color color = arrow.GetComponent<Image>().color;
            color.a =  transparency;
            arrow.GetComponent<Image>().color = color;
        }
    } 
    public int AddMonsterIndicator(GameObject target, bool friendly=false) {
        int indicatorIndex = monsterArrows.Count;
        GameObject newArrow = Instantiate(friendly ? friendArrowPrefab : monsterArrowPrefab);
        newArrow.transform.parent = gameObject.transform;
        monsterArrows.Add(newArrow);
        monsterArrowTargets.Add(target);
        return indicatorIndex;
    }
    public void RemoveMonsterIndicator(int index) {
        if (index>=monsterArrows.Count)return;
        GameObject toBeDied =monsterArrows[index];
        monsterArrows.RemoveAt(index);
        monsterArrowTargets.RemoveAt(index);
        Destroy(toBeDied);
    }
    public void ShowSimpleToolTip(Vector2 position, string text) {

        smallToolTipPrefab.transform.position = position;
        smallToolTipPrefab.transform.Find("text").GetComponent<TextMeshProUGUI>().text = text;
    }
    public void ShowDetailedToolTip(
        Vector2 position,
        string title,
        string subTitle,
        string detail
    ) {
        detailedToolTipPrefab.transform.Find("content/description").GetComponent<TextMeshProUGUI>().text = detail;
        detailedToolTipPrefab.transform.Find("content/title").GetComponent<TextMeshProUGUI>().text = title;
        detailedToolTipPrefab.transform.Find("content/subTitle").GetComponent<TextMeshProUGUI>().text = subTitle;
        detailedToolTipPrefab.transform.position = position;
        toolTipTimer=toolTipTime;
        toolTipStep=1;
    }
    public void HideToolTips() {
        detailedToolTipPrefab.transform.position = new Vector2(3200f,3200f);
        smallToolTipPrefab.transform.position = new Vector2(3200f,3200f);
    }
    public void ExitGame(){
        Debug.Log("Exit");
         Application.Quit();
    }
}
