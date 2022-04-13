using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartInCrafting : EventTrigger
{
    public int itemId;
    private bool dragging;
    [HideInInspector]
    public Vector2 defaultPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         if (dragging) {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //  transform.localPosition = new Vector2(0,0);
        } else {
            transform.position = new Vector2(0,0);
            transform.localPosition = defaultPos;
        }
    }
    public override void OnPointerDown(PointerEventData eventData) {
        dragging = true;
        UIManager.Instance.draggingPartFC = true;
    }

    public override void OnPointerUp(PointerEventData eventData) {
        dragging = false;
        UIManager.Instance.draggingPartFC = false;

        // Part crafting
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        if (!(UIHelper.UIOverlapCheck(raycastResults.ToArray(), "CraftDraggable"))) {
            UIManager.Instance.partRemovedFromCrafting = true;

            UIManager.Instance.lastDroppedPartFC = GameLib.Instance.GetPartById(itemId);
        }
        // Debug.Log(raysastResults[1].gameObject.name);

        
    }
}
