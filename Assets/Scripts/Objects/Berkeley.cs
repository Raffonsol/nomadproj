using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Berkeley : MonoBehaviour
{
    public LayerMask unMatchable; 
    public int size = 5;
    public int spawnableId;
    public bool indistructible = false;

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
            DestroyAndRecount();
        }
        // if (checkTimer > 0) 
        // checkTimer -= Time.deltaTime;
        // else {
        //     Check();
        // }
        ContinuedUpdate();
    }
    public virtual void ContinuedUpdate(){}

    void Check() {
        if (indistructible) return;
        checkTimer = 3f;
        Collider2D potentialHit = Physics2D.OverlapCircle(transform.position, size, unMatchable);

        if (potentialHit && potentialHit.gameObject.name != gameObject.name){
            Debug.Log("destroy overlap " + Physics2D.OverlapCircle(transform.position, size, unMatchable).gameObject.name);
           if (potentialHit.gameObject.tag == "Rsrc") {
                // we don't want trees destroying all the structures, so if its a rsrc, destroy the rsrc instead
                try {
                    potentialHit.gameObject.GetComponent<Berkeley>().DestroyAndRecount();
                }   
                catch (NullReferenceException e) {
                    Destroy(potentialHit.gameObject);
                }
           } else {
                DestroyAndRecount();
           }
        }
    
    }

    public void DestroyAndRecount() {
        // HARDCODED 7 = neutral buildings id
        if (spawnableId == 7) {
            VillageManager.Instance.scenesExisting--;
        } else
        BerkeleyManager.Instance.spawnables[spawnableId].currentQuantity--;
        Destroy(gameObject);
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
