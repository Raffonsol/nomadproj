using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berkeley : MonoBehaviour
{
    public LayerMask unMatchable; 
    public int size = 5;

    private float disappearDistance = 0;
    private float checkTimer = 3f;
    
    void Start() {
        Check();
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        if (disappearDistance == 0)disappearDistance = BerkeleyManager.Instance.disappearDistance;
        // If we are too far, delete us. Get it? Berkeley..
        if (Vector3.Distance(transform.position, Camera.main.transform.position) > disappearDistance) {
            Destroy(gameObject);
        }
        if (checkTimer > 0) 
        checkTimer -= Time.deltaTime;
        else {
            Check();
        }
    }

    void Check() {
        checkTimer = 3f;
        if (Physics2D.OverlapCircle(transform.position, size, unMatchable) && 
            Physics2D.OverlapCircle(transform.position, size, unMatchable).gameObject.name != gameObject.name){
                Destroy(gameObject);
        }
    
    }

    // void OnTriggerEnter2D(Collider2D collided)
	// {
	// 	if (collided.CompareTag("Berkeley") || collided.CompareTag("Rsrc"))
	// 	{
    //         Debug.Log("Death "+collided.gameObject.name);
	// 		Destroy(collided.gameObject);
	// 	}
	// }
	void OnCollisionStay2D(Collision2D collision)
	{
		if ( (gameObject.tag == "Berkeley" || gameObject.tag == "Rsrc") && 
            (collision.collider.CompareTag("Berkeley") || collision.collider.CompareTag("Rsrc")))
		{
            Debug.Log("death "+collision.collider.gameObject.name + " hit me, the " +gameObject.name);
			Destroy(collision.collider.gameObject);
		}
	}
}