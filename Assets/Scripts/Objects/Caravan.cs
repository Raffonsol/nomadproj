using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caravan : Combatant
{
    public GameObject seat;
    public GameObject rider;
    public GameObject horse;
    
    public Sprite step1;
    public Sprite step2;

    private int targetRegionIndex;
    private bool hovering = false;
    private float uiShowTime = 0;

    protected override void ContinuedStart()
    {
        rider = VillageManager.Instance.SpawnNpc(seat.transform.position.x,seat.transform.position.y);
        rider.transform.parent = seat.transform;
        rider.transform.localPosition = new Vector2(0,0);
        Destroy(rider.GetComponent<Neutral>());
        Destroy(rider.GetComponent<Rigidbody2D>());
        Destroy(rider.GetComponent<Collider2D>());
        rider.transform.localRotation = Quaternion.Euler(0,0,180);

        // decide where this goes
        targetRegionIndex= UnityEngine.Random.Range(1,5);
    }

    protected override void ListenForClick() {

        if (uiShowTime > 0) {
            uiShowTime -= Time.deltaTime;
            if (uiShowTime <=0) {
                UIManager.Instance.HideRegionTransition();
            }
        }

	}
    void OnMouseOver()
    {
		hovering = true;
        uiShowTime = 5f;
        UIManager.Instance.ShowRegionTransition(targetRegionIndex);
    }
    protected override void StepAnim()
	{
		if (moveTimer1 > 0) {
            horse.GetComponent<SpriteRenderer>().sprite = step1;
            moveTimer2 = feetSpeed;
            
			moveTimer1 -= Time.deltaTime;
		} else {
            horse.GetComponent<SpriteRenderer>().sprite = step2;
			moveTimer2 -= Time.deltaTime;

			if (moveTimer2 < 0) {
				moveTimer1 = feetSpeed;
			}
		}
	}

}
