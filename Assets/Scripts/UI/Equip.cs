using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EquippableUI {
    public Slot slot;
    public int equippedId;
    public bool left;
    public GameObject square;
}
public class Equip : MonoBehaviour
{
    public EquippableUI headSlot;
    public EquippableUI chestSlot;
    public EquippableUI rBootSlot;
    public EquippableUI lBootSlot;
    public EquippableUI rHandSlot;
    public EquippableUI lHandSlot;
    public EquippableUI rPauldronSlot;
    public EquippableUI lPauldronSlot;
    public EquippableUI rRingSlot;
    public EquippableUI lRingSlot;
    public EquippableUI capeSlot;
    public EquippableUI necklaceSlot;

    public GameObject armorTemplate;

    private Color transp;
    private Color opak;
    // Start is called before the first frame update
    void Start()
    {
        transp = Color.white;
        opak = Color.white;
        transp.a = 0.5f;
        opak.a = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        CheckDroppedItems();
        CheckForReset();
        if (headSlot.equippedId == 0) {
            headSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = transp;
        } else {
            headSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = opak;
        }
        if (chestSlot.equippedId == 0) {
            chestSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = transp;
        } else {
            chestSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = opak;
        }
        if (rBootSlot.equippedId == 0) {
            rBootSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = transp;
        } else {
            rBootSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = opak;
        }
        if (lBootSlot.equippedId == 0) {
            lBootSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = transp;
        } else {
            lBootSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = opak;
        }
        if (lPauldronSlot.equippedId == 0) {
            lPauldronSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = transp;
        } else {
            lPauldronSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = opak;
        }
        if (rPauldronSlot.equippedId == 0) {
            rPauldronSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = transp;
        } else {
            rPauldronSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = opak;
        }
        if (lHandSlot.equippedId == 0) {
            lHandSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = transp;
        } else {
            lHandSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = opak;
        }
        if (rHandSlot.equippedId == 0) {
            rHandSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = transp;
        } else {
            rHandSlot.square.transform.Find("Image").gameObject.GetComponent<Image>().color = opak;
        }
    }
    void CheckForReset() {
        if (UIManager.Instance.armorNeedsUpdate) {
            UIManager.Instance.armorNeedsUpdate = false;
            GameObject temp = new GameObject();
            Equipment item = new Equipment();
            bool needInstantiation = false;
            GameObject fixer;
            // head
            {
            if (headSlot.equippedId != 0) {
                needInstantiation = false;
                temp = headSlot.square.transform.Find("DraggableEquipped").gameObject;
            } else {
                needInstantiation = true;
            }
            headSlot.equippedId = Player.Instance.activePerson.equipped.head != null
                ? Player.Instance.activePerson.equipped.head.id
                : 0 ;
            if (headSlot.equippedId != 0) {
                if (needInstantiation) {
                    temp = Instantiate(armorTemplate, headSlot.square.transform);
                    temp.name = "DraggableEquipped";
                }
                item = GameLib.Instance.GetEquipmentById(headSlot.equippedId);
                temp.GetComponent<Image>().sprite = item.icon;
                temp.GetComponent<Image>().SetNativeSize();
                temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                temp.GetComponent<PartInCrafting>().itemId = item.id;
                temp.GetComponent<PartInCrafting>().isLeft = false;
                temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
            } else if (!needInstantiation)
                {Destroy(temp);}
            }

            // chest
            {
            if (chestSlot.equippedId != 0) {
                needInstantiation = false;
                temp = chestSlot.square.transform.Find("DraggableEquipped").gameObject;
            } else {
                needInstantiation = true;
            }
            chestSlot.equippedId = Player.Instance.activePerson.equipped.chest != null
                ? Player.Instance.activePerson.equipped.chest.id
                : 0 ;
            if (chestSlot.equippedId != 0) {
                if (needInstantiation) {
                    temp = Instantiate(armorTemplate, chestSlot.square.transform);
                    temp.name = "DraggableEquipped";
                }
                item = GameLib.Instance.GetEquipmentById(chestSlot.equippedId);
                temp.GetComponent<Image>().sprite = item.icon;
                temp.GetComponent<Image>().SetNativeSize();
                temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                temp.GetComponent<PartInCrafting>().itemId = item.id;
                temp.GetComponent<PartInCrafting>().isLeft = false;
                temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
            } else if (!needInstantiation)
                {Destroy(temp);}
            }

            // left shoulder
            {
            if (lPauldronSlot.equippedId != 0) {
                needInstantiation = false;
                temp = lPauldronSlot.square.transform.Find("DraggableEquipped").gameObject;
            } else {
                needInstantiation = true;
            }
            lPauldronSlot.equippedId = Player.Instance.activePerson.equipped.leftPauldron != null
                ? Player.Instance.activePerson.equipped.leftPauldron.id
                : 0 ;
            if (lPauldronSlot.equippedId != 0) {
                if (needInstantiation) {
                    temp = Instantiate(armorTemplate, lPauldronSlot.square.transform);
                    temp.name = "DraggableEquipped";
                    fixer = Instantiate(new GameObject(), temp.transform);
                    fixer.AddComponent<Image>();
                    
                } else {
                    fixer = temp.transform.GetChild(0).gameObject;
                }
                item = GameLib.Instance.GetEquipmentById(lPauldronSlot.equippedId);
                fixer.GetComponent<Image>().sprite = item.icon;
                fixer.GetComponent<Image>().SetNativeSize();
                fixer.transform.rotation = Quaternion.Euler(0,180,0);
                fixer.layer = UIManager.Instance.TrasnparentLayer;
                temp.GetComponent<Image>().color = opak;
                temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);

                temp.GetComponent<PartInCrafting>().itemId = item.id;
                temp.GetComponent<PartInCrafting>().isLeft = true;
                temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
                } else if (!needInstantiation)
                    {Destroy(temp);}
            }
            
            // right shoulder
            {
            if (rPauldronSlot.equippedId != 0) {
                needInstantiation = false;
                temp = rPauldronSlot.square.transform.Find("DraggableEquipped").gameObject;
            } else {
                needInstantiation = true;
            }
            rPauldronSlot.equippedId = Player.Instance.activePerson.equipped.rightPauldron != null
                ? Player.Instance.activePerson.equipped.rightPauldron.id
                : 0 ;
            if (rPauldronSlot.equippedId != 0) {
                if (needInstantiation) {
                    temp = Instantiate(armorTemplate, rPauldronSlot.square.transform);
                    temp.name = "DraggableEquipped";
                }
                item = GameLib.Instance.GetEquipmentById(rPauldronSlot.equippedId);
                temp.GetComponent<Image>().sprite = item.icon;
                temp.GetComponent<Image>().SetNativeSize();
                temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                temp.GetComponent<PartInCrafting>().itemId = item.id;
                temp.GetComponent<PartInCrafting>().isLeft = false;
                temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
            } else if (!needInstantiation)
                {Destroy(temp);}
            }
            
             // left hand
            {
            if (lHandSlot.equippedId != 0) {
                needInstantiation = false;
                temp = lHandSlot.square.transform.Find("DraggableEquipped").gameObject;
            } else {
                needInstantiation = true;
            }
            lHandSlot.equippedId = Player.Instance.activePerson.equipped.leftHand != null
                ? Player.Instance.activePerson.equipped.leftHand.id
                : 0 ;
            if (lHandSlot.equippedId != 0) {
                if (needInstantiation) {
                    temp = Instantiate(armorTemplate, lHandSlot.square.transform);
                    temp.name = "DraggableEquipped";
                    fixer = Instantiate(new GameObject(), temp.transform);
                    fixer.AddComponent<Image>();
                    
                } else {
                    fixer = temp.transform.GetChild(0).gameObject;
                }
                item = GameLib.Instance.GetEquipmentById(lHandSlot.equippedId);
                fixer.GetComponent<Image>().sprite = item.icon;
                fixer.GetComponent<Image>().SetNativeSize();
                fixer.transform.rotation = Quaternion.Euler(0,180,0);
                fixer.layer = UIManager.Instance.TrasnparentLayer;
                temp.GetComponent<Image>().color = opak;
                temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);

                temp.GetComponent<PartInCrafting>().itemId = item.id;
                temp.GetComponent<PartInCrafting>().isLeft = true;
                temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
                } else if (!needInstantiation)
                    {Destroy(temp);}
            }
            
            // right hand
            {
            if (rHandSlot.equippedId != 0) {
                needInstantiation = false;
                temp = rHandSlot.square.transform.Find("DraggableEquipped").gameObject;
            } else {
                needInstantiation = true;
            }
            rHandSlot.equippedId = Player.Instance.activePerson.equipped.rightHand != null
                ? Player.Instance.activePerson.equipped.rightHand.id
                : 0 ;
            if (rHandSlot.equippedId != 0) {
                if (needInstantiation) {
                    temp = Instantiate(armorTemplate, rHandSlot.square.transform);
                    temp.name = "DraggableEquipped";
                }
                item = GameLib.Instance.GetEquipmentById(rHandSlot.equippedId);
                temp.GetComponent<Image>().sprite = item.icon;
                temp.GetComponent<Image>().SetNativeSize();
                temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                temp.GetComponent<PartInCrafting>().itemId = item.id;
                temp.GetComponent<PartInCrafting>().isLeft = false;
                temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
            } else if (!needInstantiation)
                {Destroy(temp);}
            }
            
             // left foot
            {
            if (lBootSlot.equippedId != 0) {
                needInstantiation = false;
                temp = lBootSlot.square.transform.Find("DraggableEquipped").gameObject;
            } else {
                needInstantiation = true;
            }
            lBootSlot.equippedId = Player.Instance.activePerson.equipped.leftFoot != null
                ? Player.Instance.activePerson.equipped.leftFoot.id
                : 0 ;
            if (lBootSlot.equippedId != 0) {
                if (needInstantiation) {
                    temp = Instantiate(armorTemplate, lBootSlot.square.transform);
                    temp.name = "DraggableEquipped";
                    fixer = Instantiate(new GameObject(), temp.transform);
                    fixer.AddComponent<Image>();
                    
                } else {
                    fixer = temp.transform.GetChild(0).gameObject;
                }
                item = GameLib.Instance.GetEquipmentById(lBootSlot.equippedId);
                fixer.GetComponent<Image>().sprite = item.icon;
                fixer.GetComponent<Image>().SetNativeSize();
                fixer.transform.rotation = Quaternion.Euler(0,180,0);
                fixer.layer = UIManager.Instance.TrasnparentLayer;
                temp.GetComponent<Image>().color = opak;
                temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);

                temp.GetComponent<PartInCrafting>().itemId = item.id;
                temp.GetComponent<PartInCrafting>().isLeft = true;
                temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
                } else if (!needInstantiation)
                    {Destroy(temp);}
            }
            
            // right foot
            {
            if (rBootSlot.equippedId != 0) {
                needInstantiation = false;
                temp = rBootSlot.square.transform.Find("DraggableEquipped").gameObject;
            } else {
                needInstantiation = true;
            }
            rBootSlot.equippedId = Player.Instance.activePerson.equipped.rightFoot != null
                ? Player.Instance.activePerson.equipped.rightFoot.id
                : 0 ;
            if (rBootSlot.equippedId != 0) {
                if (needInstantiation) {
                    temp = Instantiate(armorTemplate, rBootSlot.square.transform);
                    temp.name = "DraggableEquipped";
                }
                item = GameLib.Instance.GetEquipmentById(rBootSlot.equippedId);
                temp.GetComponent<Image>().sprite = item.icon;
                temp.GetComponent<Image>().SetNativeSize();
                temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);
                temp.GetComponent<PartInCrafting>().itemId = item.id;
                temp.GetComponent<PartInCrafting>().isLeft = false;
                temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
            } else if (!needInstantiation)
                {Destroy(temp);}
            }
        }
    }
    void CheckDroppedItems() {
        if (UIManager.Instance.partDroppedOnCrafting) {
            
            UIManager.Instance.partDroppedOnCrafting = false;

            bool goingWithLeft = false;
            bool sideMatters = false;
            bool isEmpty = true;
            float distToL;
            float distToR;
            Equipment item = UIManager.Instance.lastDroppedArmor;
            GameObject temp = new GameObject();
            switch (item.slot) {
                case Slot.Head:
                    if (headSlot.equippedId != 0) {
                        isEmpty = false;
                        Player.Instance.AddEquipment(Player.Instance.activePerson.equipped.head.id);

                        temp = headSlot.square.transform.Find("DraggableEquipped").gameObject;
                    } else {
                        temp = Instantiate(armorTemplate, headSlot.square.transform);
                        temp.name = "DraggableEquipped";
                    }
                    headSlot.equippedId = item.id;
                    break;
                case Slot.Chest:
                    if (chestSlot.equippedId != 0){
                         isEmpty = false;
                         Player.Instance.AddEquipment(Player.Instance.activePerson.equipped.head.id);
                         temp = chestSlot.square.transform.Find("DraggableEquipped").gameObject;
                    } else {
                        temp = Instantiate(armorTemplate, chestSlot.square.transform);
                        temp.name = "DraggableEquipped";
                    }
                    chestSlot.equippedId = item.id;
                    break;
                case Slot.Pauldron:
                    sideMatters = true;
                   distToL = Vector2.Distance((Vector2)Input.mousePosition, (Vector2)lPauldronSlot.square.transform.position);
                   distToR = Vector2.Distance((Vector2)Input.mousePosition, (Vector2)rPauldronSlot.square.transform.position);
                    if (distToL < distToR) {
                        goingWithLeft = true;
                        if (lPauldronSlot.equippedId != 0){
                            isEmpty = false;
                            Player.Instance.AddEquipment(Player.Instance.activePerson.equipped.leftPauldron.id);
                            temp = lPauldronSlot.square.transform.Find("DraggableEquipped").gameObject;
                        } else {
                            temp = Instantiate(armorTemplate, lPauldronSlot.square.transform);
                            temp.name = "DraggableEquipped";
                        }
                        lPauldronSlot.equippedId = item.id;
                    } else {
                        if (rPauldronSlot.equippedId != 0){
                            isEmpty = false;
                            Player.Instance.AddEquipment(Player.Instance.activePerson.equipped.rightPauldron.id);
                            temp = rPauldronSlot.square.transform.Find("DraggableEquipped").gameObject;
                        } else {
                            temp = Instantiate(armorTemplate, rPauldronSlot.square.transform);
                            temp.name = "DraggableEquipped";
                        }
                        rPauldronSlot.equippedId = item.id;
                    }
                    break;
                case Slot.Hand:
                    sideMatters = true;
                    distToL = Vector2.Distance((Vector2)Input.mousePosition, (Vector2)lHandSlot.square.transform.position);
                    distToR = Vector2.Distance((Vector2)Input.mousePosition, (Vector2)rHandSlot.square.transform.position);
                    if (distToL < distToR) {
                        goingWithLeft = true;
                        if (lHandSlot.equippedId != 0){
                            isEmpty = false;
                            Player.Instance.AddEquipment(Player.Instance.activePerson.equipped.leftHand.id);
                            temp = lHandSlot.square.transform.Find("DraggableEquipped").gameObject;
                        } else {
                            temp = Instantiate(armorTemplate, lHandSlot.square.transform);
                            temp.name = "DraggableEquipped";
                        }
                        lHandSlot.equippedId = item.id;
                    } else {
                        if (rHandSlot.equippedId != 0){
                            isEmpty = false;
                            Player.Instance.AddEquipment(Player.Instance.activePerson.equipped.rightHand.id);
                            temp = rHandSlot.square.transform.Find("DraggableEquipped").gameObject;
                        } else {
                            temp = Instantiate(armorTemplate, rHandSlot.square.transform);
                            temp.name = "DraggableEquipped";
                        }
                        rHandSlot.equippedId = item.id;
                    }
                    break;
                case Slot.Foot:
                    sideMatters = true;
                    distToL = Vector2.Distance((Vector2)Input.mousePosition, (Vector2)lBootSlot.square.transform.position);
                    distToR = Vector2.Distance((Vector2)Input.mousePosition, (Vector2)rBootSlot.square.transform.position);
                    if (distToL < distToR) {
                        goingWithLeft = true;
                        if (lBootSlot.equippedId != 0){
                            isEmpty = false;
                            Player.Instance.AddEquipment(Player.Instance.activePerson.equipped.leftFoot.id);
                            temp = lBootSlot.square.transform.Find("DraggableEquipped").gameObject;
                        } else {
                            temp = Instantiate(armorTemplate, lBootSlot.square.transform);
                            temp.name = "DraggableEquipped";
                        }
                        lBootSlot.equippedId = item.id;
                    } else {
                        if (rBootSlot.equippedId != 0){
                            isEmpty = false;
                            Player.Instance.AddEquipment(Player.Instance.activePerson.equipped.rightFoot.id);
                            temp = rBootSlot.square.transform.Find("DraggableEquipped").gameObject;
                        } else {
                            temp = Instantiate(armorTemplate, rBootSlot.square.transform);
                            temp.name = "DraggableEquipped";
                        }
                        rBootSlot.equippedId = item.id;
                    }
                    break;

            }
            temp.GetComponent<Image>().sprite = item.icon;
            temp.GetComponent<Image>().SetNativeSize();
            if (sideMatters && goingWithLeft) { 
                GameObject fixer = Instantiate(new GameObject(), temp.transform);
                fixer.AddComponent<Image>();
                fixer.GetComponent<Image>().sprite = item.icon;
                fixer.GetComponent<Image>().SetNativeSize();
                fixer.transform.rotation = Quaternion.Euler(0,180,0);
                temp.GetComponent<Image>().color = opak;
            }
            temp.transform.localScale = new Vector3(0.6f, 0.6f, 1);
            temp.GetComponent<PartInCrafting>().itemId = item.id;
            temp.GetComponent<PartInCrafting>().isLeft = goingWithLeft;
            temp.GetComponent<PartInCrafting>().itemType = ItemType.Equipment;
            
            Player.Instance.EquipArmor(item, goingWithLeft);                

            // UpdateTemplateImage();
            Player.Instance.RemoveEquipment(item.id);

        }
        if (UIManager.Instance.partRemovedFromCrafting) {
            
            UIManager.Instance.partRemovedFromCrafting = false;
            GameObject itemObj;
            Equipment item = UIManager.Instance.lastDroppedArmorFC;
              switch (item.slot) {
                case Slot.Head:
                    headSlot.equippedId = 0;
                    Player.Instance.Unequip(Slot.Head);
                    Destroy(headSlot.square.transform.Find("DraggableEquipped").gameObject);
                    break;
                case Slot.Chest:
                    chestSlot.equippedId = 0;
                    Player.Instance.Unequip(Slot.Chest);
                    Destroy(chestSlot.square.transform.Find("DraggableEquipped").gameObject);
                    break;
                case Slot.Pauldron:
                    
                    if (UIManager.Instance.lastDroppedArmorFCIsLeft) {
                        itemObj = lPauldronSlot.square.transform.Find("DraggableEquipped").gameObject;
                        lPauldronSlot.equippedId = 0;
                        Player.Instance.Unequip(Slot.Pauldron, true);
                    } else {
                        itemObj = rPauldronSlot.square.transform.Find("DraggableEquipped").gameObject;
                        rPauldronSlot.equippedId = 0;
                        Player.Instance.Unequip(Slot.Pauldron, false);
                    }
                    Destroy(itemObj);
                    break;
                case Slot.Foot:
                    if (UIManager.Instance.lastDroppedArmorFCIsLeft) {
                        itemObj = lBootSlot.square.transform.Find("DraggableEquipped").gameObject;
                        lBootSlot.equippedId = 0;
                        Player.Instance.Unequip(Slot.Foot, true);
                    } else {
                        itemObj = rBootSlot.square.transform.Find("DraggableEquipped").gameObject;
                        rBootSlot.equippedId = 0;
                        Player.Instance.Unequip(Slot.Foot, false);
                    }
                    Destroy(itemObj);
                    break;
                case Slot.Hand:
                    if (UIManager.Instance.lastDroppedArmorFCIsLeft) {
                        itemObj = lHandSlot.square.transform.Find("DraggableEquipped").gameObject;
                        lHandSlot.equippedId = 0;
                        Player.Instance.Unequip(Slot.Hand, true);
                    } else {
                        itemObj = rHandSlot.square.transform.Find("DraggableEquipped").gameObject;
                        rHandSlot.equippedId = 0;
                        Player.Instance.Unequip(Slot.Hand, false);
                    }
                    Destroy(itemObj);
                    break;
              }
        }
    }
}
