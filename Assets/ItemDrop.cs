using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : Berkeley
{
    public ItemType itemType;
    public int id;
    // Start is called before the first frame update
    public virtual void Start()
    {

    }

    void OnTriggerEnter2D(Collider2D collided)
	{
		// Debug.Log(collided.CompareTag("Hitbox"));
		if (collided.CompareTag("Character"))
		{
			Collided(collided);
		}
	}
	private void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Character"))
		{
			Collided(collision.collider);
		}
	}
	void Collided(Collider2D collided)
	{
		// TODO: give to picker and not just player
        Player.Instance.PickupItem(itemType, id);
		Destroy(transform.gameObject);
	}
}
