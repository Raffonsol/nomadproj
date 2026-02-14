using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public class Monster : Combatant
{

    public Sprite step1;
    public Sprite step2;
    public Sprite attack;

    public bool chaseSteps=false;
    public Sprite chaseStep1;
    public Sprite chaseStep2;

    public float attackSpeed = 1f;
    public int projectileId;

    public MonsterSkill[] skills;

    public Color aboutToAttackColor;

    [HideInInspector]
    public bool damagingOnTouch = false;
    

    private HitBox hitScript;
    private bool projectileGoing = false;

    private bool hovering = false;

    private Vector2 skillTargetPosition;
    
    // Cached animation sprites
    private Sprite currentDisplayedSprite = null;
    private float stepAnimTimer = 0f;

    private float stunTimer = 0f;
    private bool isStunned = false;

    // Start is called before the first frame update
    protected override void ContinuedStart()
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
    protected override void RunSkillTimers()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i].cooldownTimer>0) {
                skills[i].cooldownTimer-=Time.deltaTime;
            }
        }
        if (stunTimer > 0) {
            stunTimer -= Time.deltaTime;
        } else if (isStunned) {
            isStunned = false;
        }
    }
    protected override void Move(Vector2 target, float speed, bool step=true)
    {
        if (isStunned) {
            Stay();
            return;
        }
        base.Move(target, speed, step);
    }
    protected override void ResetPatrol()
    {
        patrolTimer = patrolStopInterval;
        
        // Reduced patrol range from 20 to 8 units for better NPC density and behavior
        float patrolRange = 8f;
        patrolTarget = new Vector2(
            transform.position.x + UnityEngine.Random.Range(-patrolRange, patrolRange),
            transform.position.y + UnityEngine.Random.Range(-patrolRange, patrolRange)
        );
    }
    protected override bool CheckSkills(bool attacking = true)
    {
        if (routine == Routine.UsingSkill) return false;
        bool oneSkillFound = false;
        for (int i = 0; i < skills.Length; i++)
        {
            if ((attacking && !skills[i].castWhenAttacking) || (!attacking && !skills[i].castWhenAttacked)) continue;
            if (skills[i].cooldownTimer<=0.01f) {
                oneSkillFound=true;
                skillCastId = i;
                skillChannelingTimer = skills[i].channelingTime;
                transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = skills[i].channeling;
                SwitchRoutine(Routine.UsingSkill);
                break;
            }
        }
        return oneSkillFound;
    }
    
    protected override void Attack()
    {
        attackTimer -= Time.deltaTime;
        Vector2 currentPosition = transform.position;

        if (attackTimer > (attackCooldown / 2)) {
            bodyRenderer.color = aboutToAttackColor;

            Stay();
            moveDirection = chaseTargetPosition - currentPosition;
            moveDirection.Normalize();
            float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, 
		                                       Quaternion.Euler(0, 0, targetAngle + 180), 
		                                       turnSpeed * Time.deltaTime));
        }
        else if (attackTimer > 0)
        {
            bodyRenderer.sprite = attack;
            bodyRenderer.color = Color.white;
             if (attackDamageType == DamageType.Melee) {
				 // start damaging;
                damagingOnTouch = true;
                hitScript.hitting = true;
                hitBox.isTrigger = false;

                Vector2 target = moveDirection + currentPosition;
                Move(target, attackSpeed, false );

                
                audioSource.clip = attackSound; audioSource.Play(); 
			} else if (!projectileGoing && attackDamageType == DamageType.Ranged) {
                
                projectileGoing = true;
				Consumable item = GameLib.Instance.GetConsumableById(projectileId);
                GameObject arrow = Instantiate(item.visual,transform.position, transform.rotation);
                Projectile newSettings = item.projectileSettings;
                newSettings.minDamage = minDamage;
                newSettings.maxDamage = maxDamage;
                arrow.gameObject.GetComponent<ProjectileItem>().projectileSettings = item.projectileSettings;
                arrow.gameObject.GetComponent<ProjectileItem>().faction = faction;
                arrow.gameObject.GetComponent<ProjectileItem>().shooter = gameObject;
				arrow.gameObject.GetComponent<ProjectileItem>().playerParty = false;
				arrow.gameObject.GetComponent<ProjectileItem>().consumableId = projectileId;
                arrow.gameObject.GetComponent<ProjectileItem>().Go();
                Stay();
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, transform.rotation, 0));
				
			}
           
        }
        else {
            // complete
            damagingOnTouch = false;
            hitScript.hitting = false;
            hitBox.isTrigger = true;
            projectileGoing = false;
            
            bodyRenderer.sprite = step1;
            currentDisplayedSprite = step1;
            
            cooldownTimer = attackCooldown;
            SwitchRoutine(Routine.Chasing);
        }

    }
    protected override void SkillCast()
    {
        if  (skillChannelingTimer > 0) {
            skillChannelingTimer -= Time.deltaTime;
        } else {
            MonsterSkill skill = skills[skillCastId];
            if (skillInImpact) {
                if (skillImpactingTimer > 0) {
                    // in impact
                    if (skill.skillTypes.Contains(SkillType.Move)){
                        Vector2 currentPosition = transform.position;
                        Vector2 nextPoint = Pathfind(currentPosition, skillTargetPosition);
                        moveDirection = nextPoint - currentPosition;
                        if (skill.targetSystem == 1)moveDirection = currentPosition-nextPoint; 
                        moveDirection.Normalize();
                        Vector2 target = moveDirection + currentPosition;
                        if (
                            // COPY OF MOVE SYSTEM IN COMBATANT
                            (Vector3.Distance(currentPosition, skillTargetPosition) > attackDistance || cooldownTimer > 0)
                            ) {
                            if (Vector3.Distance(currentPosition, skillTargetPosition) > attackDistance || movesOnAttackCD) // second check needed so he doesnt attack on cooldown
                            {
                                Move(target, runSpeed );
                            } else {
                                Stay();
                            }
                            float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                            rb.MoveRotation(Quaternion.Slerp(transform.rotation, 
                                                            Quaternion.Euler(0, 0, targetAngle + 180), 
                                                            turnSpeed * Time.deltaTime));
                            if (StuckCheck()) {
                                // For now just resetting into patrol when stuck
                                SwitchRoutine(Routine.Patrolling);
                            }
                        }
                        else {
                            Stay();
                        }
                    }
                    else {
                        Stay();
                        rb.MoveRotation(Quaternion.Slerp(transform.rotation, transform.rotation, 0));
                    }
                    skillImpactingTimer-= Time.deltaTime;
                } else {
                    //Impact end
                    skillInImpact = false;
                    bodyRenderer.sprite = step1;
                    currentDisplayedSprite = step1;
               
                    // stop damaging;
                    // damagingOnTouch = false;
                    // hitScript.hitting = false;
                    // hitBox.isTrigger = true;
                    defensive = false;
                    defensiveKnockback = 0;
                    defensiveDamage = 0;

                    skill.cooldownTimer = skill.cooldown;
                    SwitchRoutine(Routine.Chasing);
                }
            }
            else {
                // impact start
                skillInImpact = true;
                bodyRenderer.sprite = skill.impact;
                currentDisplayedSprite = skill.impact;
                skillImpactingTimer = skill.impactTime;
                audioSource.clip = skill.audioClip; audioSource.Play();
                
                if (skill.skillTypes.Contains(SkillType.EnterDefensiveMode)){
                    defensive = true;
                    defensiveKnockback = skill.knockBack;
                    defensiveDamage = skill.damageBase;
                }
				 // start damaging;
                // damagingOnTouch = true;
                // hitScript.hitting = true;
                // hitBox.isTrigger = false;

                //create impact collision
                if (skill.skillTypes.Contains(SkillType.CreateDamageObject)){
                    
                    GameObject aoe = Instantiate(skill.impactCollision,transform.position, transform.rotation);
                    if(aoe.GetComponent<ProjectileItem>() != null)
                    {
                        Projectile newSettings = new Projectile();
                        newSettings.maxLife = 0.4f;
                        newSettings.minDamage = skill.damageBase;
                        newSettings.maxDamage = skill.damageBase*1.5f;
                        newSettings.knockBack = skill.knockBack;
                        newSettings.speed=0;
                        aoe.gameObject.GetComponent<ProjectileItem>().projectileSettings = newSettings;
                        aoe.gameObject.GetComponent<ProjectileItem>().faction = faction;
                        aoe.gameObject.GetComponent<ProjectileItem>().shooter = gameObject;
                        aoe.gameObject.GetComponent<ProjectileItem>().playerParty = false;
                        aoe.gameObject.GetComponent<ProjectileItem>().Go();
                    }
                }
                if (skill.skillTypes.Contains(SkillType.TargetedStun)){
                    if (chaseTarget == null) return;
                    GameObject aoe = Instantiate(skill.impactCollision,chaseTarget.transform.position, transform.rotation);
                    PlayDeath anim = aoe.GetComponent<PlayDeath>();
                    if (anim != null) {
                        anim.stickTarget = chaseTarget;
                        anim.sticky = true;
                    }

                    ZombieController tar = chaseTarget.GetComponent<ZombieController>();
                    if (tar != null) {
                        tar.Stun(skill.offset);
                    }
                    HitBox hitter = chaseTarget.GetComponent<HitBox>();
                    if (hitter != null && hitter.friendlyOwner!=null) {
                        tar = hitter.friendlyOwner;
                        tar.Stun(skill.offset);
                    }
                }
                if (skill.skillTypes.Contains(SkillType.Move)){
                    if (skill.targetSystem == 1) { // pick random engaged target unit location
                        skillTargetPosition = chaseTargetPosition;
                    }
                    if (skill.targetSystem == 5) { // pick random target location are within offset
                        skillTargetPosition = new Vector2(UnityEngine.Random.Range(transform.position.x - skill.offset, transform.position.x + skill.offset), UnityEngine.Random.Range(transform.position.y - skill.offset, transform.position.y + skill.offset));
                    }

                    if (skill.moveSystem == 3) { // teleport to target
                        transform.position = skillTargetPosition;

                    }
                }
            }
        }

    }
    protected override void StepAnim()
	{
        stepAnimTimer -= Time.deltaTime;
        
        if (stepAnimTimer <= 0) {
            // Determine which sprite to show
            Sprite nextSprite = (routine == Routine.Chasing && chaseSteps) 
                ? (moveTimer1 > 0 ? chaseStep1 : chaseStep2)
                : (moveTimer1 > 0 ? step1 : step2);
            
            // Only update if changed (avoid redundant sprite assignments)
            if (nextSprite != currentDisplayedSprite) {
                bodyRenderer.sprite = nextSprite;
                currentDisplayedSprite = nextSprite;
            }
            
            float speed = (routine == Routine.Chasing && chaseSteps) 
                ? chasingFeetSpeed : feetSpeed;
            stepAnimTimer = speed;
        }
        
        moveTimer1 -= Time.deltaTime;
	}
	protected override void StopAnim()
	{
        bodyRenderer.sprite = step1;
        currentDisplayedSprite = step1;
	}
    
    void OnMouseOver()
    {
		hovering = true;
    }
	void OnMouseExit()
    {
		hovering = false;
    }
    protected override void ListenForClick(){
        if (hovering && Input.GetKey(KeyCode.Mouse0)) {
            if (Player.Instance == null || Player.Instance.activePerson.controller == null) return;
            ZombieController attacker = Player.Instance.activePerson.controller;
            float distance = attacker.IsRanged() ?  300f: size;
			if (Vector3.Distance(transform.position, attacker.gameObject.transform.position) < size){
                attacker.TriggerSkill(1);
            }
		}
    }
    public void Stun(float duration) {
        stunTimer = duration;
        isStunned = true;
    }
}
    