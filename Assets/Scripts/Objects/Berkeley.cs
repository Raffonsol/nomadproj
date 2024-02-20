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

    public bool showIndicator=false;

    private float disappearDistance = 0;
    private float checkTimer = 3f;
    public int indicatorIndex;
    
    void Start() {
        
        if (gameObject.name.Contains("rker")) {Debug.Log("Norker spanwed"); Debug.Log(spawnableId+gameObject.name);}
        BerkeleyManager.Instance.spawnables[spawnableId].currentQuantity++;
        BeforeCheck();
        if(gameObject.tag != "Monster")Check();
        ContinuedStart();
        if (showIndicator) {
            // HARDCODED 6 = neutral spawnableId
            indicatorIndex = UIManager.Instance.AddMonsterIndicator(gameObject, spawnableId==6);
        }
    }
    protected virtual void BeforeCheck() {
     
    }
    protected virtual void ContinuedStart() {
     
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        if (disappearDistance == 0)disappearDistance = BerkeleyManager.Instance.disappearDistance;
        // If we are too far, delete us. Get it? Berkeley..
        if (Vector3.Distance(transform.position, Camera.main.transform.position) > disappearDistance) {
            DestroyAndRecount();
        }

        if ((gameObject.tag == "Berkeley" || gameObject.tag == "Rsrc") ) {
            if (checkTimer > 0) 
            checkTimer -= Time.deltaTime;
            else {
                Check();
            }
        }
        
        ContinuedUpdate();
    }
    protected virtual void ContinuedUpdate(){}

    void Check() {
        if (indistructible) return;
        checkTimer = 3f;
        Collider2D potentialHit = Physics2D.OverlapCircle(transform.position, size, unMatchable);

        if (potentialHit && potentialHit.gameObject.name != gameObject.name){
           if (potentialHit.gameObject.tag == "Rsrc" || potentialHit.gameObject.tag == "Berkeley") {
                // we don't want trees destroying all the structures, so if its a rsrc, destroy the rsrc instead
                try {
                    potentialHit.gameObject.GetComponent<Berkeley>().DestroyAndRecount();
                }   
                catch (NullReferenceException e) {
                    Debug.LogWarning("Something was just destroyed but not recounted - "+potentialHit.gameObject.name);
                    Destroy(potentialHit.gameObject);
                }
                Debug.Log("destroy overlapped " + Physics2D.OverlapCircle(transform.position, size, unMatchable).gameObject.name);
           } else {
                Debug.Log("self=destroyed from overlap with" + Physics2D.OverlapCircle(transform.position, size, unMatchable).gameObject.name);
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
        if (showIndicator) {
            UIManager.Instance.RemoveMonsterIndicator(indicatorIndex);
        }
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
			collision.collider.gameObject.GetComponent<Berkeley>().DestroyAndRecount();
		}
        //  else if ((gameObject.tag == "Berkeley" || gameObject.tag == "Rsrc")) {
        //     Debug.Log("Collision with "+collision.collider.gameObject.name+", I am "+gameObject.name);
        // }
	}
    void OnTriggerStay2D(Collider2D collision)
	{
		if ( (gameObject.tag == "Berkeley" || gameObject.tag == "Rsrc") &&
            (collision.gameObject.CompareTag("Road")))
		{
            Debug.Log("road killed me, the " +gameObject.name);
			DestroyAndRecount();
		}
	}
}
