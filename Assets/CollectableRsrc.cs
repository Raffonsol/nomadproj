using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableRsrc : Berkeley
{
    public DamageRsrcType damageRsrcTypeNeeded;
    public Drop[] drops;
    // these should only drop on successful harvest
    public Drop[] sfcDrops;
    public int life;

    private int currentLife;

    private int shakeCycles = 5;
    private float shakeTime = 0.06f;
    private float shakeTimer;
    private float shakeCountdown;
    private bool shaking;
    private float invincibleTime = 0.8f;
    private float invincibleTimer;
    private List<Drop> remainingDrops;

    private Vector3 nPosition;

    // Start is called before the first frame update
    void Start()
    {
        remainingDrops = new List<Drop>(drops);
        currentLife = life;
        shakeTimer = shakeTime;
        shakeCountdown = shakeCycles;
        nPosition = transform.position;
        invincibleTimer = invincibleTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Shake();
        if (invincibleTimer > 0)
        invincibleTimer -= Time.deltaTime;
    }
    // void OnTriggerEnter2D(Collider2D collided)
	// {
	// 	if (collided.CompareTag("Hitbox"))
	// 	{
	// 		Collided(collided);
	// 	}
	// }
	void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Hitbox"))
		{
			Collided(collision.collider);
		}
	}
	void Collided(Collider2D collided)
	{
        GameObject target = collided.gameObject;
		HitBox hitter = target.GetComponent<HitBox>();
        if (hitter.hitting && invincibleTimer <= 0) {
            if (hitter.damageRsrcType == damageRsrcTypeNeeded) {
                // successfully harvesting
                SfcHit();
            } else {
                // bad hit but can still give loot
                BHit();
            }
            
        }
    }
    void SfcHit() {
        invincibleTimer = invincibleTime;
        currentLife -= 1;
        shaking = true;
        if (currentLife <= 0) {
            for(int i = 0; i <sfcDrops.Length; i++){
                DropLoot(sfcDrops[i]);
            }
            for(int i = 0; i <remainingDrops.Count; i++){
                DropLoot(sfcDrops[i]);
            }
            Destroy(gameObject);
        }
        
        
    }
    void BHit() {
        invincibleTimer = invincibleTime;
        shaking = true;
        if (remainingDrops.Count < 1) return;
        int i = Random.Range(0, remainingDrops.Count - 1);
        if (Random.Range(0, 100) <= remainingDrops[i].chance) {
            
            DropLoot(remainingDrops[i]);
            remainingDrops.RemoveAt(i);
        }
    }
    void DropLoot(Drop drop) {
        int dropsQ = Random.Range(1, drop.maxDropped);
        
        for(int i = 0; i <dropsQ; i++) {Debug.Log("dropping " + drop.itemId);
            Vector2 pos = transform.position;
            GameObject itemObj = Instantiate(GameOverlord.Instance.itemDropPrefab,
                new Vector2(pos.x, pos.y), Quaternion.Euler(0,0,0));
            itemObj.GetComponent<ItemDrop>().id = drop.itemId;
            itemObj.GetComponent<ItemDrop>().itemType = drop.itemType;
            itemObj.GetComponent<SpriteRenderer>().sprite = GameLib.Instance.GetItemByType(drop.itemId, drop.itemType).icon;

        }
        
    }
    void Shake() {
        if (!shaking) {
            return;
        }

        
        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;
        } else {
            shakeCountdown -= 1;
            shakeTimer = shakeTime;

            Vector3 newPos = new Vector3(nPosition.x, nPosition.y, nPosition.z);
            newPos.x += Random.Range(-0.2f, 0.2f);
            newPos.y += Random.Range(-0.2f, 0.2f);
            transform.position = newPos;
            try {
                transform.Find("shadow").transform.position = nPosition;
            } catch {}

            
            if (shakeCountdown <= 0) {
                shaking = false;
                shakeCountdown = shakeCycles;
                shakeTimer = shakeTime;
                transform.position = nPosition;

                try {
                    transform.Find("shadow").transform.localPosition = new Vector2(0,0);
                } catch {}
                
            }
        }

    }
}
