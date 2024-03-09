using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : Berkeley
{
    public ItemType itemType;
    public int id;
	public bool pickable = false;
    private float spawnTimer = 1f;
	private float targetAngle;
	private Vector2 targetFall;
	private Vector2 currentPosition;
	private float existenceDuration;
	void Start(){
		existenceDuration = 20f; // 20 seconds is plenty to get an item people
		Vector2 currentPosition = transform.position;
		targetAngle = Random.Range(1, 360);
		targetFall = new Vector2(
			Random.Range(currentPosition.x -2, currentPosition.x + 2),
			Random.Range(currentPosition.y -2, currentPosition.y + 2)
		);
		
		GameOverlord.Instance.nearbyDrops.Add(gameObject);
	}
    void FixedUpdate()
    {
		Vector2 currentPosition = transform.position;
													   
		if (spawnTimer > 0) {
			spawnTimer -= Time.deltaTime;
			transform.position = Vector3.Lerp (currentPosition, targetFall, 1.5f*Time.deltaTime);
			transform.rotation = Quaternion.Slerp (transform.rotation, 
		                                       Quaternion.Euler (0, 0, targetAngle + 180), 
		                                       1.5f*Time.deltaTime);
		} else if (existenceDuration> 0) {
			existenceDuration -= Time.deltaTime;
			return;
		} else {
			GameOverlord.Instance.nearbyDrops.Remove(gameObject);
			Destroy(gameObject);
		}
		if (spawnTimer > 0.9f) {
			transform.localScale = new Vector2(1.3f, 1.3f);
		}
		else if (spawnTimer > 0.7f) {
			transform.localScale = new Vector2(1f, 1f);
		}
		else if (spawnTimer > 0.5f) {
			transform.localScale = new Vector2(0.7f, 0.7f);
		}
		else if (spawnTimer > 0.3f) {
			transform.localScale = new Vector2(0.5f, 0.5f);
		}else {
			transform.localScale = new Vector2(0.7f, 0.7f);
			pickable = true;
		}
    }

    void OnTriggerEnter2D(Collider2D collided)
	{
		// Debug.Log(collided.CompareTag("Hitbox"));
		if (collided.CompareTag("Character"))
		{
			Collided(collided);
		}
	}
	void OnTriggerStay2D(Collider2D collided)
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
		if (!pickable) return;
		
		GameOverlord.Instance.nearbyDrops.Remove(gameObject);
        Player.Instance.PickupItem(itemType, id);
        if (itemType==ItemType.Part||itemType==ItemType.Equipment)UIManager.Instance.CheckAutoEquips();
		Destroy(gameObject);
	}
}
