using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Routine
{
    Patrolling,
    Sleeping,
    Alerted,
    Chasing,
    Attacking,
    Fleeing,
}


public class Monster : Berkeley
{

    public int faction = 1;
    public int[] hatesFactions;

    public Sprite step1;
    public Sprite step2;
    public Sprite attack;

    public float maxLife = 60f;
    public float minDamage = 5f;
    public float maxDamage = 15f;

    public float invincibilityTime = 0.3f;

    public float runSpeed = 1.5f;
    public float attackSpeed = 1f;
    public float attackDistance = 0.73f;
    public float attackCooldown = 1f;
    public float patrolSpeed = 1f;
	public float turnSpeed = 3f;
	public float feetSpeed = 0.5f;

    public float alertRange = 50f;

    public float patrolStopInterval = 4f;

    public float attackDuration = 1.5f;
    public Color aboutToAttackColor;

    public float giveUpDistance = 30f;

    [HideInInspector]
    public bool damagingOnTouch = false;

    public int minExpGiven = 1;
    public int maxExpGiven = 3;

    private Routine routine;

    private float life;

    private float cooldownTimer;
    private float patrolTimer;
    private Vector2 patrolTarget;
    private Vector2 moveDirection;
    private Vector2 sizes;

    private float moveTimer1 = 0;
	private float moveTimer2 = 0;

    private float stuckDistanceLimit = 2.4f;
    public float stuckCheckTime = 4f;
    private float stuckCheckTimer;

    private float attackTimer;

    private Vector3 lastCheckPosition;

    private GameObject chaseTarget;
    private Vector2 chaseTargetPosition;

    private PolygonCollider2D hitBox;
    private HitBox hitScript;
    
    [HideInInspector]
    public float invincibleTimer;
    public Drop[] drops;


    // Start is called before the first frame update
    void Start()
    {
        life = maxLife;

        hitBox = transform.Find("HitBox").gameObject.GetComponent<PolygonCollider2D>();
        hitScript = transform.Find("HitBox").gameObject.GetComponent<HitBox>();
        hitScript.damageMin = minDamage;
        hitScript.damageMax = maxDamage;
        cooldownTimer = attackCooldown;

        hitBox.isTrigger = true;
        sizes = transform.Find("Body").gameObject.GetComponent<Renderer>().bounds.size;
        stuckCheckTimer = stuckCheckTime;
        SwitchRoutine(Routine.Patrolling);
        ResetPatrol();
    }
    void SwitchRoutine(Routine newRoutine)
    {
        // starter setups for each routine
        switch (routine) {
        case (Routine.Patrolling):
            ResetPatrol();
            break;
        case (Routine.Chasing):
            
            break;
        case (Routine.Attacking):
            attackTimer = attackDuration;
            break;
        }
        routine = newRoutine;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
         if (invincibleTimer > -1f) {
            invincibleTimer -= Time.deltaTime;
        }
        if (cooldownTimer >= 0) {
            cooldownTimer -= Time.deltaTime;
        }
        Die();
        switch (routine) {
        case (Routine.Patrolling):
            Patrol();
            break;
        case (Routine.Chasing):
            Chase();
            break;
        case (Routine.Attacking):
            Attack();
            break;
        }
        
    }
    void Patrol()
    {
        patrolTimer -= Time.deltaTime;

        Vector2 currentPosition = transform.position;

        Vector2 nextPoint = GameOverlord.Instance.Pathfind(currentPosition, patrolTarget);
        moveDirection = nextPoint - currentPosition;
        moveDirection.Normalize();
        Vector2 target = moveDirection + currentPosition;
		if (Vector3.Distance(currentPosition, patrolTarget) > 0.3f) {
			
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, moveDirection);

            float blockDistance = Vector2.Distance(currentPosition, hit.point) + sizes.x + sizes.y;
            
            // pathblock checking
            if (Vector2.Distance(currentPosition, patrolTarget) < blockDistance || blockDistance > 10f) {
                // gonna keep going
                transform.position = Vector3.Lerp (currentPosition, target, patrolSpeed * Time.deltaTime);

			    float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp (transform.rotation, 
		                                       Quaternion.Euler (0, 0, targetAngle + 180), 
		                                       turnSpeed * Time.deltaTime);
			    if (StuckCheck()) {
                    ResetPatrol();
                }
            }
            else {
                // find a path
                ResetPatrol();
            }
            if(DetectEnemy())
            {
                SwitchRoutine(Routine.Chasing);
            }
            StepAnim();
			
		}
		else {
            ResetPatrol();
			StopAnim();
		}

    }
    void Chase()
    {
        if (chaseTarget == null) SwitchRoutine(Routine.Patrolling);
        
        Vector2 currentPosition = transform.position;
        try {
            chaseTargetPosition = chaseTarget.transform.position;
        } catch (MissingReferenceException) {
            // meaning they don't exist anymore
            chaseTarget = null;
            SwitchRoutine(Routine.Patrolling);
        }

        Vector2 nextPoint = GameOverlord.Instance.Pathfind(currentPosition, chaseTargetPosition);
        moveDirection = nextPoint - currentPosition;
        moveDirection.Normalize();
        Vector2 target = moveDirection + currentPosition;
        if (Vector3.Distance(currentPosition, chaseTargetPosition) > giveUpDistance) {SwitchRoutine(Routine.Patrolling); return;}
		if (Vector3.Distance(currentPosition, chaseTargetPosition) > attackDistance || cooldownTimer > 0) {
			
            transform.position = Vector3.Lerp (currentPosition, target, runSpeed * Time.deltaTime);

            float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp (transform.rotation, 
                                            Quaternion.Euler (0, 0, targetAngle + 180), 
                                            turnSpeed * Time.deltaTime);
            if (StuckCheck()) {
                // For now just resetting into patrol when stuck
                SwitchRoutine(Routine.Patrolling);
            }
            

            StepAnim();
			
		}
		else {
			StopAnim();
            SwitchRoutine(Routine.Attacking);
		}
    }
    void Attack()
    {
        attackTimer -= Time.deltaTime;
        Vector2 currentPosition = transform.position;

        if (attackTimer > (attackDuration / 2)) {
            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().color = aboutToAttackColor;

            
            moveDirection = chaseTargetPosition - currentPosition;
            moveDirection.Normalize();
            float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp (transform.rotation, 
		                                       Quaternion.Euler (0, 0, targetAngle + 180), 
		                                       turnSpeed * Time.deltaTime);
        }
        else if (attackTimer > 0)
        {
            // start damaging;
            damagingOnTouch = true;
            hitScript.hitting = true;
            hitBox.isTrigger = false;

            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = attack;
            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().color = Color.white;

            Vector2 target = moveDirection + currentPosition;
            transform.position = Vector3.Lerp (currentPosition, target, attackSpeed * Time.deltaTime);
        }
        else {
            // complete
            damagingOnTouch = false;
            hitScript.hitting = false;
            hitBox.isTrigger = true;
            
            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
            
            cooldownTimer = attackCooldown;
            SwitchRoutine(Routine.Chasing);
        }

    }
    void ResetPatrol()
    {
        patrolTimer = patrolStopInterval;
        patrolTarget = new Vector3(
            Math.Abs(UnityEngine.Random.Range(transform.position.x - 20f, transform.position.x + 20f)),
            Math.Abs(UnityEngine.Random.Range(transform.position.y - 20f, transform.position.y + 20f)), 0);
    }
    bool StuckCheck()
    {
        stuckCheckTimer -= Time.deltaTime;
        if (stuckCheckTimer < 0) {
            stuckCheckTimer = stuckCheckTime;
            if (Vector2.Distance(transform.position, lastCheckPosition) < stuckDistanceLimit) {
                return true;
            }
            lastCheckPosition = transform.position;
        }
        return false;
    }
    bool DetectEnemy()
    {
        // TODO: use eyes to cast more rays here
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection);
        // Debug.DrawRay(transform.position, moveDirection, Color.red);
        if (hit.transform && Array.IndexOf(hatesFactions, Util.TagToFaction(hit.transform.gameObject.tag)) > -1
        && Vector2.Distance(hit.point, transform.position) < alertRange){
            chaseTarget = hit.transform.gameObject;
            Debug.Log("Huh?");
            return true;
        }
        
        return false;
    }
    void StepAnim()
	{
		if (moveTimer1 > 0) {
			transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
			moveTimer2 = feetSpeed;
			moveTimer1 -= Time.deltaTime;
		} else {
			transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step2;

			moveTimer2 -= Time.deltaTime;

			if (moveTimer2 < 0) {
				moveTimer1 = feetSpeed;
			}
		}
	}
	void StopAnim()
	{
        transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
	}
    void OnTriggerEnter2D(Collider2D collided)
	{
		if (collided.CompareTag("Projectile"))
		{
			Shot(collided);
		}
	}
	void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Hitbox"))
		{
			Collided(collision.collider);
		}
        if (collision.collider.CompareTag("Projectile"))
		{
			Shot(collision.collider);
		}
	}
	void Collided(Collider2D collided)
	{
		GameObject target = collided.gameObject;

		HitBox hitter = target.GetComponent<HitBox>();
		if (hitter.hitting && hitter.faction != faction && invincibleTimer < 0) {
			float damage = UnityEngine.Random.Range(hitter.damageMin, hitter.damageMax);
			TakeDamage(damage);
            Player.Instance.Engage(gameObject);

            chaseTarget = collided.transform.gameObject;
            if (routine!=Routine.Chasing && routine!=Routine.Attacking) SwitchRoutine(Routine.Chasing);
		}

	}
    void Shot(Collider2D collided)
	{
		GameObject target = collided.gameObject;

		ProjectileItem hitter = target.GetComponent<ProjectileItem>();
		if (hitter.faction != faction && invincibleTimer < 0) {
			float damage = UnityEngine.Random.Range(hitter.projectileSettings.minDamage, hitter.projectileSettings.maxDamage);
			TakeDamage(damage);
            Player.Instance.Engage(gameObject);
            Destroy(target);

            try {
                chaseTarget = hitter.shooter;
                if (routine!=Routine.Chasing && routine!=Routine.Attacking) SwitchRoutine(Routine.Chasing);
            } catch (NullReferenceException) {
                // TODO: search in the direction projectile came from
            }
            
		}

	}
    public void TakeDamage(float damage) {
         if (invincibleTimer > 0) return;
        invincibleTimer = invincibilityTime; 
        life -= damage;
        GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, transform);
        DamageText.GetComponent<DamageText>().textToDisplay = damage.ToString("0.00");
    }
    void Die() {
        if (life > 0) return;
        GameObject deathImage = Instantiate(GameOverlord.Instance.deathPrefab, new Vector2(transform.position.x, transform.position.y),   Quaternion.Euler(0, 0, 0));
        deathImage.transform.parent = null;
        if (Player.Instance.engagedMonster!=null &&
            Player.Instance.engagedMonster.name == name) {
            Player.Instance.engagementTimer = 0;
        }
        for(int i = 0; i <drops.Length; i++){
            DropLoot(drops[i]);
        }
        int exp = UnityEngine.Random.Range(minExpGiven, maxExpGiven);
        Player.Instance.GainExperience(exp);
        // GameOverlord.Instance.nearbyMonsters.Remove( GameOverlord.Instance.nearbyMonsters.Single( s => s.name == transform.gameObject.name ) );
        Destroy(transform.gameObject);
    }
    void DropLoot(Drop drop) {
        int dropsQ = UnityEngine.Random.Range(1, drop.maxDropped);
        
        for(int i = 0; i <dropsQ; i++) {
            Vector2 pos = transform.position;
            GameObject itemObj = Instantiate(GameOverlord.Instance.itemDropPrefab,
                new Vector2(pos.x, pos.y), Quaternion.Euler(0,0,0));
            itemObj.GetComponent<ItemDrop>().id = drop.itemId;
            itemObj.GetComponent<ItemDrop>().itemType = drop.itemType;
            itemObj.GetComponent<SpriteRenderer>().sprite = GameLib.Instance.GetItemByType(drop.itemId, drop.itemType).icon;

        }
        
    }
}
    