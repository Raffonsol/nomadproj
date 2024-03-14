using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;

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
		if (GameOverlord.Instance.fightingBerkeleyTags.Contains( collided.gameObject.tag))
		{
			GameOverlord.Instance.nearbyMonsters.Add(collided.transform.gameObject);
		}
		if (collided.gameObject.tag == "Rsrc")
		{
			GameOverlord.Instance.nearbyRsrc.Add(collided.transform.gameObject);
		}

	}
	void OnTriggerExit2D(Collider2D collided)
	{
		if (GameOverlord.Instance.fightingBerkeleyTags.Contains( collided.gameObject.tag))
		{
			try {
				GameOverlord.Instance.nearbyMonsters.Remove( GameOverlord.Instance.nearbyMonsters.Single( s => s.name == collided.gameObject.name ) );
			} catch (InvalidOperationException) {

			} catch (MissingReferenceException e) {
				GameOverlord.Instance.nearbyMonsters = new List<GameObject>();
				Debug.LogError("Deleted monster somehow left camra bounds?"+e);
			}
		}
		if (collided.gameObject.tag == "Rsrc")
		{
			try {GameOverlord.Instance.nearbyRsrc.Remove(collided.transform.gameObject);} 
			catch (MissingReferenceException e) {
				GameOverlord.Instance.nearbyRsrc = new List<GameObject>();
				Debug.LogError("Deleted rsrc somehow left camra bounds?"+e);
			}
		}

	}
}
