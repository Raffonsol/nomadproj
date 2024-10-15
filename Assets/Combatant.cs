using System.Collections;
using System;
using System.Linq;
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
    Searching,
    Tossed, // for knockback
    UsingSkill,
}

public class Combatant : Berkeley
{
    public string named;
    // 0=inGroup 1=hostile 2=neutral
    public int faction = 2;
    public int[] hatesFactions;

    public float maxLife = 60f;
    public float minDamage = 5f;
    public float maxDamage = 15f;
    public float invincibilityTime = 0.3f;
    public float runSpeed = 1.5f;
    public bool movesOnAttackCD = true;
    public bool movesAwayOnAttackCD = false;
    public bool heavy = false;

    public float attackDistance = 0.73f;
    public float attackCooldown = 1f;
    // switches targets when attacked no matter if already has target
    public bool distractable = true;

    public DamageType attackDamageType;
    public float patrolSpeed = 1f;
	public float turnSpeed = 3f;
	public float feetSpeed = 0.5f;
    public bool runsAway = false;
	public float chasingFeetSpeed = 0.5f;

    public float alertRange = 10f;
    public float searchTimer = 20f;
    public float patrolStopInterval = 4f;
    public float giveUpDistance = 30f;

    public int minExpGiven = 1;
    public int maxExpGiven = 3;

    public float stuckCheckTime = 4f;

    [HideInInspector]
    public float invincibleTimer;
    public Drop[] drops;

    public bool searches =true;

    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;

    protected int skillCastId;
    protected float skillChannelingTimer;
    protected float skillImpactingTimer;
    protected bool skillInImpact = false;

    protected bool engaged = false;

    protected Routine routine;
    public float life;

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

    protected bool defensive = false;
    protected float defensiveKnockback = 0;
    protected float defensiveDamage = 0;

    protected Vector3 originPosition;
    protected Vector3 lastCheckPosition;

    protected GameObject chaseTarget;
    protected Vector2 chaseTargetPosition;
    protected PolygonCollider2D hitBox;
    protected Vision vision;
    protected float visionTimer;

    protected AudioSource audioSource;

    protected Vector2 knockBackLandPosition;

    protected float pathFindTime = 1f;
    protected float pathFindTimer = 0f;
    protected Vector2 lastFoundPath;


    private void Awake() 
    {
        originPosition = transform.position;
        vision = transform.Find("Vision").GetComponent<Vision>();
        audioSource = GetComponent<AudioSource>();
    }

    protected void SwitchRoutine(Routine newRoutine)
    {
        
        // close-ups for each routine
        switch (routine) {
        case (Routine.Patrolling):
            ResetPatrol();
            break;
        case (Routine.Chasing):
            break;
        case (Routine.Attacking):
            attackTimer = attackCooldown;
            hitBox.isTrigger = true;
            break;
        case (Routine.Searching):
            originPosition = Camera.main.transform.position;
            GetComponent<CircleCollider2D>().isTrigger = false;
            break;
        }
        routine = newRoutine;
    }
    // Update is called once per frame
    protected override void ContinuedUpdate()
    {
         if (invincibleTimer > -1f) {
            invincibleTimer -= Time.deltaTime;
        }
        if (cooldownTimer >= 0) {
            cooldownTimer -= Time.deltaTime;
        }
        Die();
        ListenForClick();
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
        case (Routine.UsingSkill):
            SkillCast();
            break;
        case (Routine.Tossed):
            BeTossed();
            break;
        case (Routine.Searching):
            Search();
            break;
        }
        
    }
    protected virtual void ListenForClick(){
    }
    protected virtual void RunSkillTimers()
    {
        // implemented on monster
    }
    protected Vector2 Pathfind(Vector2 currentPosition, Vector2 patrolTarget) {

        
        if (pathFindTimer > 0 && lastFoundPath!=null && Vector2.Distance(transform.position,lastFoundPath)>0.1f ) {
            pathFindTimer -= Time.deltaTime;
            return lastFoundPath;
        }
        
        Vector2 path;
        path =patrolTarget;
        lastFoundPath = path;
        pathFindTimer = pathFindTime;

        // MapMaker.Instance.debugMarker.transform.position = path;

        return path;
    }
    protected void Patrol()
    {
        patrolTimer -= Time.deltaTime;

        Vector2 currentPosition = transform.position;

        Vector2 nextPoint = Pathfind(currentPosition, patrolTarget);
        moveDirection = nextPoint - currentPosition;
        moveDirection.Normalize();
        Vector2 target = moveDirection + currentPosition;
		if (Vector3.Distance(currentPosition, patrolTarget) > 0.3f) {
			
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, moveDirection);

            float blockDistance = Vector2.Distance(currentPosition, hit.point) + sizes.x + sizes.y;
            
            // pathblock checking
            if (Vector2.Distance(currentPosition, patrolTarget) < blockDistance || blockDistance > 10f) {
                // gonna keep going
                Move( target, patrolSpeed);

			    float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, 
		                                       Quaternion.Euler (0, 0, targetAngle + 180), 
		                                       turnSpeed * Time.deltaTime));
			    if (StuckCheck()) {
                    ResetPatrol();
                }
            }
            else {
                Stay();
                StopAnim();
                transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, transform.rotation,0));
                // find a path
                ResetPatrol();
            }
            if(DetectEnemy())
            {
                SwitchRoutine(Routine.Chasing);
                engaged = true;
            }
			
		}
		else {
            ResetPatrol();
			StopAnim();
		}

    }
    protected void Search()
    {
        if (searchTimer > 0) {
            searchTimer -= Time.deltaTime;
        } else {
            // searching for too long creates problems
            gameObject.GetComponent<Berkeley>().DestroyAndRecount();
        }
        Vector2 currentPosition = transform.position;

        Vector2 nextPoint = Camera.main.transform.position;
        moveDirection = nextPoint - currentPosition;
        moveDirection.Normalize();
        Vector2 target = moveDirection + currentPosition;
		if (Vector3.Distance(currentPosition, nextPoint) > alertRange*2f) {

            transform.position = Vector3.Lerp (currentPosition, target, patrolSpeed * 2f * Time.deltaTime);


        }
		else {
            MonsterCheck();
            SwitchRoutine(Routine.Patrolling);
		}

        
    }
    protected void Stay() {
        if (heavy) {
            transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezePositionX ;
        } else {
            transform.GetComponent<Rigidbody2D>().MovePosition( Vector3.Lerp (transform.position, transform.position,0));
        }
    }
    protected void Move(Vector2 target, float speed, bool step=true) {
        if(step)StepAnim();
        if (heavy) {
            transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
        transform.GetComponent<Rigidbody2D>().MovePosition( Vector3.Lerp (transform.position, target, speed * Time.deltaTime));
    }
    protected void Chase()
    {
        RunSkillTimers();
        if (chaseTarget == null) SwitchRoutine(Routine.Patrolling);
        
        Vector2 currentPosition = transform.position;
        try {
            chaseTargetPosition = chaseTarget.transform.position;
        } catch (NullReferenceException) {
            // meaning they don't exist anymore
            chaseTarget = null;
            SwitchRoutine(Routine.Patrolling);
        }
         catch (MissingReferenceException) {
            // meaning they don't exist anymore
            chaseTarget = null;
            SwitchRoutine(Routine.Patrolling);
        }

        Vector2 nextPoint = Pathfind(currentPosition, chaseTargetPosition);
        moveDirection = nextPoint - currentPosition;
        if (runsAway || (movesAwayOnAttackCD && cooldownTimer > 0))moveDirection = currentPosition-nextPoint;
        moveDirection.Normalize();
        Vector2 target = moveDirection + currentPosition;
        if (Vector3.Distance(currentPosition, chaseTargetPosition) > giveUpDistance) {SwitchRoutine(Routine.Patrolling); return;}
		if (
            // (runsAway && Vector3.Distance(currentPosition, chaseTargetPosition) < attackDistance) ||
            (Vector3.Distance(currentPosition, chaseTargetPosition) > attackDistance || cooldownTimer > 0)
            ) {
			if (Vector3.Distance(currentPosition, chaseTargetPosition) > attackDistance || movesOnAttackCD) // second check needed so he doesnt attack on cooldown
            {
                Move(target, runSpeed );
            } else {
                Stay();
            }
            float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, 
                                            Quaternion.Euler (0, 0, targetAngle + 180), 
                                            turnSpeed * Time.deltaTime));
            if (StuckCheck()) {
                // For now just resetting into patrol when stuck
                SwitchRoutine(Routine.Patrolling);
            }
		}
		else {
            Stay();
            transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, transform.rotation,0));
            bool usedSkill = false;
            usedSkill = CheckSkills();
            if (!usedSkill) SwitchRoutine(Routine.Attacking);
		}
    }
    protected virtual void Attack()
    {
        // needs to be overriden
        Debug.Log("Uh oh");
    }
    protected virtual void SkillCast()
    {
        // needs to be overriden
        Debug.Log("Uh oh");
    }
    protected virtual void ResetPatrol()
    {
        patrolTimer = patrolStopInterval;
        patrolTarget = new Vector3(
            (UnityEngine.Random.Range(originPosition.x - 6f, originPosition.x + 6f)),
            (UnityEngine.Random.Range(originPosition.y - 6f, originPosition.y + 6f)), 0);
            
    }
    protected virtual bool CheckSkills(bool attacking = true)
    {
        return false;
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
        if (!GameOverlord.Instance.factionsThatSeekOutFight.Contains( faction)) return false;

        // if very far, switch to a lower code searching script
        if ( Vector2.Distance(transform.position, Camera.main.transform.position) > alertRange*2f) {
            if (searches){
                SwitchRoutine(Routine.Searching);
                // disables collision tso we dont have to pathfind
                GetComponent<CircleCollider2D>().isTrigger = true;
                return false;
            } else if (Vector2.Distance(transform.position, Camera.main.transform.position) > alertRange*4f) {
                DestroyAndRecount();
            }
        } 
        // TODO: redo
        if (visionTimer > 0) {
            visionTimer -= Time.deltaTime;
        } else {
            visionTimer = 1f;
            if (vision.peopleInDetection.Count > 0 && Array.IndexOf(hatesFactions, Util.TagToFaction(vision.peopleInDetection[0].tag)) > -1){
                chaseTarget = vision.peopleInDetection[0];
                Debug.Log("Huh?" + vision.peopleInDetection[0].name);
                if (chaseTarget.tag == "Character") Player.Instance.Engage(gameObject);
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
        if (collided.CompareTag("Hitbox"))
		{
			Collided(collided);
		}
		if (collided.CompareTag("Projectile"))
		{
			Shot(collided);
		}
	}
    void OnTriggerStay2D(Collider2D collided)
	{
        if (collided.CompareTag("Hitbox"))
		{
			Collided(collided);
		}
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
            if (defensive) { // defensive = skill where attackers take damage
                if (hitter.friendlyOwner!=null) {
                    hitter.friendlyOwner.TakeDamage(defensiveDamage);
                    hitter.friendlyOwner.KnockedBack(gameObject, defensiveKnockback);
                }
            } else {

			    float damage = UnityEngine.Random.Range(hitter.damageMin, hitter.damageMax);
                TakeDamage(damage);
                // Debug.Log("damage, "+collided.gameObject.name + " - "+" -\n"+collided.gameObject.transform.position);
                KnockedBack(target, hitter.knockBack);
                if (hitter.playerParty ){ 
                    Player.Instance.Engage(gameObject);
                    engaged = true;
                }
                // attention
                if (distractable || chaseTarget == null){
                    chaseTarget = collided.transform.gameObject;
                    HitBox box = chaseTarget.GetComponent<HitBox>();
                    if (box != null && box.friendlyOwner != null) chaseTarget = box.friendlyOwner.gameObject;
                    if (routine!=Routine.Chasing && routine!=Routine.Attacking && routine!=Routine.UsingSkill) SwitchRoutine(Routine.Chasing);
                }
                // recording
                if (hitter.recordHits) {
                    hitter.friendlyOwner.AddSkillHit(this);
                }
            }
            audioSource.clip = hitSound; audioSource.Play();
		}
	}
    void Shot(Collider2D collided)
	{
		GameObject target = collided.gameObject;

		ProjectileItem hitter = target.GetComponent<ProjectileItem>();
		if (hitter.faction != faction && invincibleTimer < 0) {
            // drop arrow
            if (hitter.faction == 0 && hitter.shooter.GetComponent<ZombieController>().self.ownedAbilities.Contains(PassiveAbility.ArrowRecovery)
                && UnityEngine.Random.Range(0, 2) == 1) {
                DropItem(ItemType.Consumable, hitter.consumableId);
            }
            if (defensive) {
                // do nothing for defensive arrows, but no damage either
            } else {
                float damage = UnityEngine.Random.Range(hitter.projectileSettings.minDamage, hitter.projectileSettings.maxDamage);
                TakeDamage(damage);
                KnockedBack(target, hitter.projectileSettings.knockBack);
                if (hitter.playerParty) {
                    Player.Instance.Engage(gameObject);
                    engaged = true;
                }
                Destroy(target); // deestroying arrow proj

                if (distractable || chaseTarget == null)
                try {
                    chaseTarget = hitter.shooter;
                    if (routine!=Routine.Chasing && routine!=Routine.Attacking && routine!=Routine.UsingSkill) SwitchRoutine(Routine.Chasing);
                } catch (NullReferenceException) {
                    // TODO: search in the direction projectile came from
                }
            }   
            audioSource.clip = hitSound; audioSource.Play(); 
		}

	}
    public void TakeDamage(float damage) {
         if (invincibleTimer > 0) return;
        invincibleTimer = invincibilityTime; 
        life -= damage;
        GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, transform);
        DamageText.GetComponent<DamageText>().textToDisplay = damage.ToString("0.00");

        // stagger 
        cooldownTimer+=(cooldownTimer/10f);

        // defensive skills
        bool usedSkill = CheckSkills(false);

        // audio
        // audio.Play();
    }
    public void Die() {
        if (life > 0) return;
        
        GameObject deathImage = Instantiate(GameOverlord.Instance.deathPrefab, new Vector2(transform.position.x, transform.position.y),   Quaternion.Euler(0, 0, 0));
        deathImage.transform.parent = null;
        if (Player.Instance.engagedMonster.Count > 0) {
            Player.Instance.Unengage(gameObject);
        }
        for(int i = 0; i <drops.Length; i++){
            DropLoot(drops[i]);
        }
        int exp = UnityEngine.Random.Range(minExpGiven, maxExpGiven);
        Player.Instance.GainExperience(exp);

        GameObject soundBox = GameObject.Instantiate(GameOverlord.Instance.soundBox,transform.position, Quaternion.Euler(0,0,0));
        AudioSource audiSource = soundBox.GetComponent<AudioSource>(); audiSource.clip = deathSound; audiSource.Play();

        // GameOverlord.Instance.nearbyMonsters.Remove( GameOverlord.Instance.nearbyMonsters.Single( s => s.name == transform.gameObject.name ) );
       gameObject.GetComponent<Berkeley>().DestroyAndRecount();
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

    public void KnockedBack(GameObject source, float amount) {
        if (amount <0.1f) return;
        knockBackLandPosition = Vector3.MoveTowards(transform.position,source.transform.position, amount*-2f);
        SwitchRoutine(Routine.Tossed);
        // TODO: Implemenent knockback
    }
	void BeTossed() {
		if (Vector3.Distance(transform.position, knockBackLandPosition) > 0.2f) {
			Move(knockBackLandPosition, 3f, false);
		} else {
			SwitchRoutine(Routine.Chasing);
		}
	}
}
