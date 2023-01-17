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

public class Combatant : Berkeley
{
    public int faction = 2;
    public int[] hatesFactions;

    public float maxLife = 60f;
    public float minDamage = 5f;
    public float maxDamage = 15f;
    public float invincibilityTime = 0.3f;
    public float runSpeed = 1.5f;

    public float attackDistance = 0.73f;
    public float attackCooldown = 1f;
    public float patrolSpeed = 1f;
	public float turnSpeed = 3f;
	public float feetSpeed = 0.5f;

    public float alertRange = 50f;
    public float patrolStopInterval = 4f;
    public float giveUpDistance = 30f;

    public int minExpGiven = 1;
    public int maxExpGiven = 3;

    public float stuckCheckTime = 4f;

    [HideInInspector]
    public float invincibleTimer;
    public Drop[] drops;

    protected Routine routine;
    protected float life;

    protected float cooldownTimer;
    protected float patrolTimer;
    protected Vector2 patrolTarget;
    protected Vector2 moveDirection;
    protected Vector2 sizes;

    protected float moveTimer1 = 0;
	protected float moveTimer2 = 0;

    protected float stuckDistanceLimit = 2.4f;
    protected float stuckCheckTimer;
    protected float attackTimer;

    protected Vector3 originPosition;
    protected Vector3 lastCheckPosition;

    protected GameObject chaseTarget;
    protected Vector2 chaseTargetPosition;
    protected PolygonCollider2D hitBox;

    private void Awake() 
    {
        originPosition = transform.position;
    }

    protected void SwitchRoutine(Routine newRoutine)
    {
        // starter setups for each routine
        switch (routine) {
        case (Routine.Patrolling):
            ResetPatrol();
            break;
        case (Routine.Chasing):
            
            break;
        case (Routine.Attacking):
            attackTimer = attackCooldown;
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
    protected void Patrol()
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
    protected void Chase()
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
    protected virtual void Attack()
    {
        // needs to be overriden
        Debug.Log("Uh oh");
    }
    protected void ResetPatrol()
    {
        patrolTimer = patrolStopInterval;
        patrolTarget = new Vector3(
            (UnityEngine.Random.Range(originPosition.x - 20f, originPosition.x + 20f)),
            (UnityEngine.Random.Range(originPosition.y - 20f, originPosition.y + 20f)), 0);
    }
    protected bool StuckCheck()
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
    protected bool DetectEnemy()
    {
        for(int i = 1; i < 4; i++){
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection);
            if (i ==1)
            hit = Physics2D.Raycast(transform.position, moveDirection+(Vector2)transform.up);
            if (i == 3)
            hit = Physics2D.Raycast(transform.position, moveDirection+(Vector2)transform.forward);

            Debug.DrawRay(transform.position, moveDirection+(Vector2)transform.up);
            Debug.DrawRay(transform.position, moveDirection);
            Debug.DrawRay(transform.position, moveDirection+(Vector2)transform.forward);
            if (hit.transform && Array.IndexOf(hatesFactions, Util.TagToFaction(hit.transform.gameObject.tag)) > -1
            && Vector2.Distance(hit.point, transform.position) < alertRange){
                chaseTarget = hit.transform.gameObject;
                Debug.Log("Huh?" + hit.transform.gameObject.name + Util.TagToFaction(hit.transform.gameObject.tag));
                return true;
            }
        }
        return false;
    }
    protected virtual void StepAnim()
	{
        // Needs to be overriden
    } 
    protected virtual void StopAnim()
	{
        // Needs to be overriden
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
            if (hitter.playerParty) Player.Instance.Engage(gameObject);

            chaseTarget = collided.transform.gameObject;
            if (routine!=Routine.Chasing && routine!=Routine.Attacking) SwitchRoutine(Routine.Chasing);
		}

	}
    void Shot(Collider2D collided)
	{
		GameObject target = collided.gameObject;

		ProjectileItem hitter = target.GetComponent<ProjectileItem>();
		if (hitter.faction != faction && invincibleTimer < 0) {
            // drop arrow
            if (hitter.shooter.GetComponent<ZombieController>().self.ownedAbilities.Contains(PassiveAbility.ArrowRecovery)
                && UnityEngine.Random.Range(0, 2) == 1) {
                DropItem(ItemType.Consumable, hitter.consumableId);
            }
			float damage = UnityEngine.Random.Range(hitter.projectileSettings.minDamage, hitter.projectileSettings.maxDamage);
			TakeDamage(damage);
            if (hitter.playerParty) Player.Instance.Engage(gameObject);
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
    public void Die() {
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
            DropItem(drop.itemType, drop.itemId);
           
        }
        
    }
    void DropItem(ItemType dropType, int dropId) {
         Vector2 pos = transform.position;
        GameObject itemObj = Instantiate(GameOverlord.Instance.itemDropPrefab,
            new Vector2(pos.x, pos.y), Quaternion.Euler(0,0,0));
        itemObj.GetComponent<ItemDrop>().id = dropId;
        itemObj.GetComponent<ItemDrop>().itemType = dropType;
        itemObj.GetComponent<SpriteRenderer>().sprite = GameLib.Instance.GetItemByType(dropId, dropType).icon;

    }
}
