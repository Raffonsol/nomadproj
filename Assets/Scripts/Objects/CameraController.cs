using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class CameraController : MonoBehaviour {

	private Vector3 newPosition;
	private GameObject following;
	
	void Start () 
	{
		newPosition = transform.position;
		SetFollowing();
	}	

	public void SetFollowing() {
		following = Player.Instance.controller.gameObject;
	}

	void Update () 
	{
		if (following == null) return;
		newPosition = following.transform.position;
		newPosition.z = -10;
		transform.position = newPosition;
	}

	void OnTriggerEnter2D(Collider2D collided)
	{
		if (collided.CompareTag("Monster"))
		{
			// HARDCODED 1 = angry faction
			if (collided.gameObject.GetComponent<Monster>().faction == 1)
			GameOverlord.Instance.nearbyMonsters.Add(collided.transform.gameObject);
		}
		if (collided.CompareTag("Npc"))
		{
			// HARDCODED 1 = angry faction
			if (collided.gameObject.GetComponent<Neutral>().faction == 1)
			GameOverlord.Instance.nearbyMonsters.Add(collided.transform.gameObject);
		}
	}
	void OnTriggerExit2D(Collider2D collided)
	{
		if (collided.CompareTag("Monster"))
		{
			if (collided.gameObject.GetComponent<Monster>().faction == 1)
			try {
				GameOverlord.Instance.nearbyMonsters.Remove( GameOverlord.Instance.nearbyMonsters.Single( s => s.name == collided.gameObject.name ) );
			} catch (InvalidOperationException) {

			}
		}
		if (collided.CompareTag("Npc"))
		{
			if (collided.gameObject.GetComponent<Neutral>().faction == 1)
			GameOverlord.Instance.nearbyMonsters.Remove( GameOverlord.Instance.nearbyMonsters.Single( s => s.name == collided.transform.gameObject.name ) );
		}
	}
}
