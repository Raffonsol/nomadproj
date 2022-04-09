using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	private Vector3 newPosition;
	
	void Start () 
	{
		newPosition = transform.position;
	}	

	void Update () 
	{
		newPosition = GameObject.FindGameObjectsWithTag("Character")[0].transform.position;
		newPosition.z = -10;
		transform.position = newPosition;
	}
}
