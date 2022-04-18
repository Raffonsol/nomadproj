using UnityEngine;
using System.Collections;
using System.Linq;

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
		newPosition = following.transform.position;
		newPosition.z = -10;
		transform.position = newPosition;
	}

	void OnTriggerEnter2D(Collider2D collided)
	{
		if (collided.CompareTag("Monster"))
		{
			GameOverlord.Instance.nearbyMonsters.Add(collided.transform.gameObject);
		}
	}
	void OnTriggerExit2D(Collider2D collided)
	{
		if (collided.CompareTag("Monster"))
		{
			GameOverlord.Instance.nearbyMonsters.Remove( GameOverlord.Instance.nearbyMonsters.Single( s => s.name == collided.transform.gameObject.name ) );
		}
	}
}
