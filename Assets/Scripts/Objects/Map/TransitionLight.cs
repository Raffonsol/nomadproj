using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionLight : MonoBehaviour
{
    public bool isFog;
    public int regionIndex;

    private float uiShowTime = 0;
    

    void Update()
    {
        if (uiShowTime > 0) {
            uiShowTime -= Time.deltaTime;
            if (uiShowTime <=0) {
                UIManager.Instance.HideRegionTransition();
            }
        }
    }


    void OnTriggerStay2D(Collider2D collided)
	{
        if (collided.CompareTag("Character"))
		{
            if (isFog) {
                GameOverlord.Instance.ChangeRegion(regionIndex);
            } else {
                uiShowTime = 5f;
                UIManager.Instance.ShowRegionTransition(regionIndex);
            }
		}
	}
}
