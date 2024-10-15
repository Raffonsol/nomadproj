using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mimic
{
    public int spawnableId;
    public Sprite[] transformAnimation;
    public int chance;
    public float frameDurations;
}

public class CollectableRsrc : Berkeley
{
    public DamageRsrcType damageRsrcTypeNeeded;
    public Drop[] drops;
    // these should only drop on successful harvest
    public Drop[] sfcDrops;
    public int life;
    public float shakeStrength = 0.2f;

    public Mimic mimic;

    public AudioClip destroyedSound;
    public AudioClip hitSound;
    public AudioClip shakeSound;
    public AudioClip mimicAwakeSound;

    public bool exhausted = false;

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
    private bool hovering = false;
    private bool canBeMimic = true;
    private bool becomingMimic = false;
    private float mimicAnimationTimer = 0;
    private int mimicAnimationIndex = 0;

    private AudioSource audioSource;

    // Start is called before the first frame update
    protected override void ContinuedStart()
    {
        remainingDrops = new List<Drop>(drops);
        currentLife = life;
        shakeTimer = shakeTime;
        shakeCountdown = shakeCycles;
        nPosition = transform.position;
        invincibleTimer = invincibleTime;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    protected override void ContinuedUpdate()
    {
        BecomeMimic();
        Shake();
        ListenForClick();
        if (invincibleTimer > 0)
        invincibleTimer -= Time.deltaTime;
    }
    void OnTriggerEnter2D(Collider2D collided)
	{
		if (collided.CompareTag("Hitbox"))
		{
			Collided(collided);
		}
	}
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
            RunMimicCheck();
        }
    }
    void SfcHit() {
        invincibleTimer = invincibleTime;
        currentLife -= 1;
        shaking = true;
        audioSource.clip = hitSound; audioSource.Play();
        if (currentLife <= 0) {
            for(int i = 0; i <sfcDrops.Length; i++){
                DropLoot(sfcDrops[i]);
            }
            for(int i = 0; i <remainingDrops.Count; i++){
                DropLoot(remainingDrops[i]);
            }
            GameObject soundBox = GameObject.Instantiate(GameOverlord.Instance.soundBox,transform.position, Quaternion.Euler(0,0,0));
            AudioSource audiSource = soundBox.GetComponent<AudioSource>(); audiSource.clip = destroyedSound; audiSource.Play();
            DestroyAndRecount();
        }
        
        
    }
    void BHit() {
        invincibleTimer = invincibleTime;
        shaking = true;
        audioSource.clip = shakeSound; audioSource.Play();
        if (remainingDrops.Count < 1) {
            exhausted=true;
            return;}
        int i = Random.Range(0, remainingDrops.Count - 1);
        if (Random.Range(0, 101) <= remainingDrops[i].chance) {
            
            DropLoot(remainingDrops[i]);
            remainingDrops.RemoveAt(i);
        }
    }
    void DropLoot(Drop drop) {
        int dropsQ = Random.Range(1, drop.maxDropped);
        
        for(int i = 0; i <dropsQ; i++) {
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
            newPos.x += Random.Range(-shakeStrength, shakeStrength);
            newPos.y += Random.Range(-shakeStrength, shakeStrength);
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
    void RunMimicCheck(){
        if (!canBeMimic) return;
        int rando = Random.Range(0, 102);
        Debug.Log("Mimic roll "+ rando );
        if (rando < mimic.chance) {
            // become mimic
            becomingMimic=true;
            mimicAnimationTimer=mimic.frameDurations;
            audioSource.clip = mimicAwakeSound; audioSource.Play();
        } else {
            // 50/50 chance to determine as not mimic
            if (Random.Range(0, 101) <= 50) canBeMimic=false;
        }
    }
    void BecomeMimic() {
        if (!becomingMimic) {
            return;
        }

        if (mimicAnimationTimer > 0) {
            mimicAnimationTimer -= Time.deltaTime;
        } else {
            mimicAnimationTimer = mimic.frameDurations;

            GetComponent<SpriteRenderer>().sprite = mimic.transformAnimation[mimicAnimationIndex];
            mimicAnimationIndex += 1;

            if (mimicAnimationIndex >= mimic.transformAnimation.Length) {
                becomingMimic = false;
                BerkeleySpawnable spawn = BerkeleyManager.Instance.spawnables[mimic.spawnableId];
                Instantiate(spawn.obj, transform.position, Quaternion.Euler(0,0, transform.rotation.z+90f));
                DestroyAndRecount();
            }
        }

    }
    void OnMouseOver()
    {
		hovering = true;
    }
	void OnMouseExit()
    {
		hovering = false;
    }
    void ListenForClick(){
        if (hovering && Input.GetKey(KeyCode.Mouse0)) {
            ZombieController attacker = Player.Instance.activePerson.controller;
            float distance = attacker.IsRanged() ?  300f: size;
			if (Vector3.Distance(transform.position, attacker.gameObject.transform.position) < size){
                attacker.TriggerSkill(1);
            }
		}
    }
}
