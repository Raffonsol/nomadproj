using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Neutral : Berkeley
{

    public int faction = 2;
    public int[] hatesFactions;
    public int startingWeaponId;
    public int[] startingWeaponParts;
    public int arrowId;

    public GameObject leftFoot;
	public GameObject rightFoot;

	public GameObject rightHand;
    public GameObject leftHand;
	public GameObject weaponObject;

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

    public Color aboutToAttackColor;

    public float giveUpDistance = 30f;

    public DamageType attackDamageType;

    [HideInInspector]
    public bool damagingOnTouch = false;

    public int minExpGiven = 1;
    public int maxExpGiven = 3;

    private float attackTime;
    private float attackTimer;
    private Weapon weapon;
    private HitBox weaponHitBox;
    private PolygonCollider2D hitBox;

    private Routine routine;

    private float life;

    private float cooldownTimer;
    private float patrolTimer;
    private Vector2 patrolTarget;
    private Vector2 moveDirection;
    private Vector2 sizes;

    private float moveTimer1 = 0;
	private float moveTimer2 = 0;
	private float feetY;
	private float feetX;
	private float feetZ;

    private float stuckDistanceLimit = 2.4f;
    public float stuckCheckTime = 4f;
    private float stuckCheckTimer;

    private bool attacking = false;

    private Vector3 lastCheckPosition;

    private GameObject chaseTarget;
    private Vector2 chaseTargetPosition;
    
    [HideInInspector]
    public float invincibleTimer;
    public Drop[] drops;


    // Start is called before the first frame update
    void Start()
    {
        life = maxLife;

        attackTime = attackCooldown;
		attackTimer = attackTime;

        sizes = transform.gameObject.GetComponent<Collider2D>().bounds.size;
        stuckCheckTimer = stuckCheckTime;
        SwitchRoutine(Routine.Patrolling);
        ResetPatrol();
        List<Part> parts = new List<Part>();
        for(int i = 0; i <startingWeaponParts.Length; i++){
            parts.Add(GameLib.Instance.GetPartById(startingWeaponParts[i]));
        }
        
        Equip(startingWeaponId, parts);
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
            attackTimer = attackTime;
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
        chaseTargetPosition = chaseTarget.transform.position;

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
        SwingAnim();
        attackTimer -= Time.deltaTime;
        Vector2 currentPosition = transform.position;

        if (attackTimer > attackTime * 0.9f && attacking == false) {
            attacking = true;
            if (attackDamageType == DamageType.Melee) {
				hitBox.isTrigger = false;
                weaponHitBox.hitting = true;
                weaponHitBox.faction = faction;
			} else if (attackDamageType == DamageType.Ranged) {
				Consumable item = GameLib.Instance.GetConsumableById(arrowId);
                GameObject arrow = Instantiate(item.visual,transform.position, transform.rotation);
                Projectile newSettings = item.projectileSettings;
                newSettings.minDamage = minDamage;
                newSettings.maxDamage = maxDamage;
                arrow.gameObject.GetComponent<ProjectileItem>().projectileSettings = item.projectileSettings;
                arrow.gameObject.GetComponent<ProjectileItem>().faction = faction;
                arrow.gameObject.GetComponent<ProjectileItem>().Go();
				
			}
        }
        else if (attackTimer <= 0) {
            // complete
            attacking = false;
            weaponHitBox.hitting = false;
            hitBox.isTrigger = true;
            
            cooldownTimer = attackCooldown;
            SwitchRoutine(Routine.Chasing);
        }

    }
    void SwingAnim() {
		if (!attacking) return;

		Step[] attackScripts = (weapon.instance.attackScripts).Reverse().ToArray();
		
		int steps = attackScripts.Length;
		int stepToPlay = (int)Math.Round((attackTimer/attackTime)*steps, 0);
		stepToPlay -= 1;
		// Debug.Log(stepToPlay + " -" + attackTimer);
		if (stepToPlay < 0) {
			rightHand.transform.localPosition = new Vector3(weapon.instance.rightHandPos.x, weapon.instance.rightHandPos.y, -0.2f);
			rightHand.transform.localRotation = Quaternion.Euler(0, 0, weapon.instance.rightHandPos.z);
			leftHand.transform.localPosition = new Vector3(weapon.instance.leftHandPos.x, weapon.instance.leftHandPos.y, -0.2f);
			leftHand.transform.localRotation = Quaternion.Euler(0, 0, weapon.instance.leftHandPos.z);
			weaponObject.transform.localPosition = new Vector3(weapon.instance.weaponPos.x, weapon.instance.weaponPos.y, -9.3f);
			weaponObject.transform.localRotation = Quaternion.Euler(0, 0, weapon.instance.weaponPos.z);
			attacking = false;
			// Player.Instance.characters[charId].hitbox.hitting = false;
			if (attackDamageType == DamageType.Melee) hitBox.isTrigger = true;
		} else {
			rightHand.transform.localPosition = new Vector3(attackScripts[stepToPlay].rightHandPos.x, attackScripts[stepToPlay].rightHandPos.y, -0.2f);
			rightHand.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].rightHandPos.z);
			leftHand.transform.localPosition = new Vector3(attackScripts[stepToPlay].leftHandPos.x, attackScripts[stepToPlay].leftHandPos.y, -0.2f);
			leftHand.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].leftHandPos.z);
			weaponObject.transform.localPosition = new Vector3(attackScripts[stepToPlay].weaponPos.x, attackScripts[stepToPlay].weaponPos.y, -9.3f);
			weaponObject.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].weaponPos.z);
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
    void Equip(int itemId, List<Part> partsUsed) {
        Weapon value = GameLib.Instance.GetWeaponById(itemId);
        weapon = value;
        Destroy(gameObject.transform.Find("Player/Body/Instance/PrimaryWeapon").gameObject);
        GameObject newWeapon = Instantiate(value.visual);
        newWeapon.name = "PrimaryWeapon";
        newWeapon.transform.parent = gameObject.transform.Find("Player/Body/Instance");
        newWeapon.transform.localPosition = new Vector3(value.instance.weaponPos.x, value.instance.weaponPos.y, -9.3f);
        newWeapon.transform.localRotation =  Quaternion.Euler(0, 0, value.instance.weaponPos.z);
        weaponObject = newWeapon;

        attackDamageType = value.damageType;

        WeaponGraphicsUpdater.UpdateWeaponGraphic(value, partsUsed, newWeapon);
         // -- RightHand
        GameObject rHand = gameObject.transform.Find("Player/Body/Instance/RHand").gameObject;
        rHand.transform.localPosition = new Vector3(value.instance.rightHandPos.x, value.instance.rightHandPos.y, -0.2f);
        rHand.transform.localRotation = Quaternion.Euler(0, 0, value.instance.rightHandPos.z);
        // -- LeftHand
        GameObject lHand = gameObject.transform.Find("Player/Body/Instance/LHand").gameObject;
        lHand.transform.localPosition = new Vector3(value.instance.leftHandPos.x, value.instance.leftHandPos.y, -0.2f);
        lHand.transform.localRotation = Quaternion.Euler(0, 0, value.instance.leftHandPos.z);
        // hide hidden

        // settings on graphics
        if (value.damageType == DamageType.Melee) {
            weaponHitBox = weaponObject.transform.Find(weapon.collidablePart.ToString()).gameObject.GetComponent<HitBox>();
            hitBox = weaponHitBox.gameObject.GetComponent<PolygonCollider2D>();
            weaponHitBox.damageRsrcType = value.damageRsrcType;
            weaponHitBox.damageMin = minDamage;
            weaponHitBox.damageMax = maxDamage;
        }
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
			rightFoot.transform.localPosition = new Vector3(0.3f, 0.13f, feetZ);
			moveTimer2 = feetSpeed;

			moveTimer1 -= Time.deltaTime;
			leftFoot.transform.localPosition = new Vector3(-0.3f, 0.28f, feetZ);
		} else {
			leftFoot.transform.localPosition =  new Vector3(-0.3f, 0.13f, feetZ);

			moveTimer2 -= Time.deltaTime;
			rightFoot.transform.localPosition = new Vector3(0.3f, 0.28f, feetZ);

			if (moveTimer2 < 0) {
				moveTimer1 = feetSpeed;
			}
		}
	}
	void StopAnim()
	{
        rightFoot.transform.localPosition = new Vector3(0.3f, 0.13f, feetZ);
		leftFoot.transform.localPosition =  new Vector3(-0.3f, 0.13f, feetZ);
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
    