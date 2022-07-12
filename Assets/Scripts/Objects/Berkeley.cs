using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berkeley : MonoBehaviour
{
    public LayerMask unMatchable; 
    public int size = 5;

    private float disappearDistance = 0;
    private float checkTimer = 3f;


    // Update is called once per frame
    void FixedUpdate()
    {   
        if (disappearDistance == 0)disappearDistance = BerkeleyManager.Instance.disappearDistance;
        // If we are too far, delete us. Get it? Berkeley..
        if (disappearDistance != null && Vector3.Distance(transform.position, Camera.main.transform.position) > disappearDistance) {
            Destroy(gameObject);
        }
        if (checkTimer > 0) 
        checkTimer -= Time.deltaTime;
        else {
            checkTimer = 3f;
            if (Physics2D.OverlapCircle(transform.position, size, unMatchable)) Destroy(gameObject);
        }
    }
    void OnTriggerEnter2D(Collider2D collided)
	{
		if (collided.CompareTag("Berkeley") || collided.CompareTag("Rsrc"))
		{
            Debug.Log("Death "+collided.gameObject.name);
			Destroy(collided.gameObject);
		}
	}
	void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Berkeley") || collision.collider.CompareTag("Rsrc"))
		{
            Debug.Log("death "+collision.collider.gameObject.name);
			Destroy(collision.collider.gameObject);
		}
	}
}
