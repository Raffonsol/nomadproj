using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Walker : MonoBehaviour
{
	protected bool isBeingTossed = false;
	protected bool isStunned = false;
    protected bool skillMoveLocked = false;
	protected Vector2 moveDirection;
	protected float turnRate;

    protected Vector2 knockBackLandPosition;


    protected void Walk(float speed, Vector2 nextPoint)
	{
		if (isBeingTossed || skillMoveLocked ) return;
		// if (!leader) {
		// 	distToMain.x = transform.position.x - Player.Instance.controller.gameObject.transform.position.x;
		// 	distToMain.y = transform.position.y - Player.Instance.controller.gameObject.transform.position.y;
		// }
		Vector2 currentPosition = transform.position;
		moveDirection = nextPoint - currentPosition;
		moveDirection.Normalize();
			
        Vector2 target = moveDirection + currentPosition;
        if (Vector3.Distance(transform.position, nextPoint) > 0.2f) {
            if (isStunned) transform.GetComponent<Rigidbody2D>().MovePosition( Vector3.Lerp (currentPosition, target, 0));
            else transform.GetComponent<Rigidbody2D>().MovePosition( Vector3.Lerp (currentPosition, target, speed * Time.deltaTime));

            float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, 
                                            Quaternion.Euler (0, 0, targetAngle + 180f), 
                                            turnRate * Time.deltaTime));
            
            StepAnim();
        }
        else {
            transform.GetComponent<Rigidbody2D>().MovePosition( Vector3.Lerp (currentPosition, target, 0));
            transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, 
                                            Quaternion.Euler (0, 0, 0),0));
            StopAnim();
        }
	}
    protected void Stay()
    {
        transform.GetComponent<Rigidbody2D>().MovePosition(transform.position);
        transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, 
                                            Quaternion.Euler (0, 0, 0),0));
    }
    protected virtual void StepAnim() {

    }
    protected virtual void StopAnim() {

    }
}
