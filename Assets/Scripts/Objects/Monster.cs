using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class Monster : Combatant
{

    public Sprite step1;
    public Sprite step2;
    public Sprite attack;

    public float attackSpeed = 1f;

    public Color aboutToAttackColor;

    [HideInInspector]
    public bool damagingOnTouch = false;

    private HitBox hitScript;
    

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

    
    protected override void Attack()
    {
        attackTimer -= Time.deltaTime;
        Vector2 currentPosition = transform.position;

        if (attackTimer > (attackCooldown / 2)) {
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
    
    protected override void StepAnim()
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
	protected override void StopAnim()
	{
        transform.Find("Body").gameObject.GetComponent<SpriteRenderer>().sprite = step1;
	}
    
}
    