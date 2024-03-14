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
    }
    protected override void ResetPatrol()
    {
        patrolTimer = patrolStopInterval;
        patrolTarget = new Vector3(
            (UnityEngine.Random.Range(transform.position.x - 20f, transform.position.x + 20f)),
            (UnityEngine.Random.Range(transform.position.y - 20f, transform.position.y + 20f)), 0);
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
            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().color = aboutToAttackColor;

            Stay();
            moveDirection = chaseTargetPosition - currentPosition;
            moveDirection.Normalize();
            float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, 
		                                       Quaternion.Euler (0, 0, targetAngle + 180), 
		                                       turnSpeed * Time.deltaTime));
        }
        else if (attackTimer > 0)
        {
            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = attack;
            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().color = Color.white;
             if (attackDamageType == DamageType.Melee) {
				 // start damaging;
                damagingOnTouch = true;
                hitScript.hitting = true;
                hitBox.isTrigger = false;

                Vector2 target = moveDirection + currentPosition;
                Move(target, attackSpeed, false );
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
                transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, transform.rotation,0));
				
			}
           
        }
        else {
            // complete
            damagingOnTouch = false;
            hitScript.hitting = false;
            hitBox.isTrigger = true;
            projectileGoing = false;
            
            transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
            
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
                    Stay();
                    transform.GetComponent<Rigidbody2D>().MoveRotation( Quaternion.Slerp (transform.rotation, transform.rotation,0));
                    skillImpactingTimer-= Time.deltaTime;
                } else {
                    //Impact end
                    skillInImpact = false;
                    transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
               
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
            } else {
                // impact start
                skillInImpact = true;
                transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = skill.impact;
                skillImpactingTimer = skill.impactTime;
                
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
            }
        }

    }
    protected override void StepAnim()
	{
		if (moveTimer1 > 0) {
            if (chaseSteps && routine==Routine.Chasing){
                transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = chaseStep1;
			    moveTimer2 = chasingFeetSpeed;
            }
			else {
                transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
			    moveTimer2 = feetSpeed;
            }
			moveTimer1 -= Time.deltaTime;
		} else {
            if (chaseSteps && routine==Routine.Chasing){
                transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = chaseStep2;
            }
			else {
                transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step2;
            }

			moveTimer2 -= Time.deltaTime;

			if (moveTimer2 < 0) {
                if (chaseSteps && routine==Routine.Chasing)moveTimer1 = chasingFeetSpeed;
				else moveTimer1 = feetSpeed;
			}
		}
	}
	protected override void StopAnim()
	{
        transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
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
}
    