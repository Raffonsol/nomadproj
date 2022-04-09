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

    public Sprite step1;
    public Sprite step2;
    public Sprite attack;

    public float maxLife = 60f;
    public float minDamage = 5f;
    public float maxDamage = 15f;

    public float invincibilityTime = 0.3f;

    public float runSpeed = 1.5f;
    public float patrolSpeed = 1f;
	public float turnSpeed = 3f;
	public float feetSpeed = 0.5f;

    public float alertRange = 50f;

    public float patrolStopInterval = 4f;

    public float attackDuration = 1.5f;
    public Color aboutToAttackColor;

    [HideInInspector]
    public bool damagingOnTouch = false;

    private Routine routine;

    private float life;

    private float patrolTimer;
    private Vector2 patrolTarget;
    private Vector2 moveDirection;
    private Vector2 size;

    private float moveTimer1 = 0;
	private float moveTimer2 = 0;

    private float stuckDistanceLimit = 3.4f;
    private float stuckCheckTime = 4f;
    private float stuckCheckTimer;

    private float attackTimer;

    private Vector3 lastCheckPosition;

    private GameObject chaseTarget;
    private Vector2 chaseTargetPosition;

    private PolygonCollider2D hitBox;
    private HitBox hitScript;
    
    [HideInInspector]
    public float invincibleTimer;


    // Start is called before the first frame update
    void Start()
    {
        life = maxLife;

        hitBox = transform.Find("HitBox").gameObject.GetComponent<PolygonCollider2D>();
        hitScript = transform.Find("HitBox").gameObject.GetComponent<HitBox>();
        hitScript.damageMin = minDamage;
        hitScript.damageMax = maxDamage;

        hitBox.isTrigger = true;
        size = transform.Find("Body").gameObject.GetComponent<Renderer>().bounds.size;
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
        Die();
        
    }
    void Patrol()
    {
        patrolTimer -= Time.deltaTime;

        Vector2 currentPosition = transform.position;


        moveDirection = patrolTarget - currentPosition;
        moveDirection.Normalize();
        Vector2 target = moveDirection + currentPosition;
		if (Vector3.Distance(currentPosition, patrolTarget) > 0.3f) {
			
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, moveDirection);

            float blockDistance = Vector2.Distance(currentPosition, hit.point) + size.x + size.y;
            
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
        chaseTargetPosition = chaseTarget.transform.position;

        moveDirection = chaseTargetPosition - currentPosition;
        moveDirection.Normalize();
        Vector2 target = moveDirection + currentPosition;
		if (Vector3.Distance(currentPosition, chaseTargetPosition) > 0.73f) {
			
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, moveDirection);

            float blockDistance = Vector2.Distance(currentPosition, hit.point) + size.x * size.y;
            
            // pathblock checking
            if (Vector2.Distance(currentPosition, chaseTargetPosition) < blockDistance || blockDistance > 10f) {
                // gonna keep going
                transform.position = Vector3.Lerp (currentPosition, target, runSpeed * Time.deltaTime);

			    float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp (transform.rotation, 
		                                       Quaternion.Euler (0, 0, targetAngle + 180), 
		                                       turnSpeed * Time.deltaTime);
			    if (StuckCheck()) {
                    // For now just resetting into patrol when stuck
                    SwitchRoutine(Routine.Patrolling);
                }
            }
            else {
                // TODO: find a path
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
            transform.position = Vector3.Lerp (currentPosition, target, runSpeed * Time.deltaTime);
        }
        else {
            // complete
            damagingOnTouch = false;
            hitScript.hitting = false;
            hitBox.isTrigger = true;
            
            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
            
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
        Debug.DrawRay(transform.position, moveDirection, Color.red);
        if (hit.transform && hit.transform.gameObject.tag == "Character"
        && Vector2.Distance(hit.point, transform.position) < alertRange){
            chaseTarget = hit.transform.gameObject;
            Debug.Log("Huh?");
            return true;
        }
        hit = Physics2D.Raycast(transform.position, moveDirection*alertRange);
        Debug.DrawRay(transform.position, moveDirection*alertRange, Color.green);
        if (hit.transform && hit.transform.gameObject.tag == "Character"
        && Vector2.Distance(hit.point, transform.position) < alertRange){
            chaseTarget = hit.transform.gameObject;
            Debug.Log("Hugh?");
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
		if (hitter.hitting && hitter.faction != 1 && invincibleTimer < 0) {
			float damage = UnityEngine.Random.Range(hitter.damageMin, hitter.damageMax);
			TakeDamage(damage);

            chaseTarget = collided.transform.gameObject;
            SwitchRoutine(Routine.Chasing);
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
        Destroy(transform.gameObject);
        
    }
}
    