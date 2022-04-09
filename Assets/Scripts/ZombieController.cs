using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class ZombieController : MonoBehaviour {
	
	public float moveSpeed;
	public float turnSpeed;
	public float feetSpeed;

	public GameObject leftFoot;
	public GameObject rightFoot;

	public GameObject rightHand;
    public GameObject leftHand;
	public GameObject weaponObject;

	private Vector2 moveDirection;
	private Vector2 lastClick;
	private List<Transform> congaLine = new List<Transform>();

	private float moveTimer1 = 0;
	private float moveTimer2 = 0;
	private float feetY;
	private float feetX;
	private float feetZ;

	private Weapon weapon;

	private float attackCooldown;
	private float attackCooldownTimer;

	private float attackTime;
	private float attackTimer;
	private bool attacking = false;

	void Start()
	{
		moveDirection = Vector2.right;
		Reset();
	}
	public void Reset() {
		moveTimer1 = feetSpeed;
		feetY = leftFoot.transform.position.y;
		feetX = leftFoot.transform.position.x;
		feetZ = leftFoot.transform.position.z;

		weapon = Player.Instance.equipped.primaryWeapon;

		// finding player's attack cooldown from their weapon
		attackCooldown = Player.Instance.equipped.primaryWeapon.cooldown;
		attackCooldownTimer = attackCooldown;

		attackTime = attackCooldown;
		attackTimer = attackTime;
	}

	private bool isLClickHeld = false;

	void Update () {
		Walk();
		Attack();
		TakeDamage();
	}

	void Attack()
	{
		SwingAnim();
		if (attackCooldownTimer > 0)
		attackCooldownTimer -= Time.deltaTime;
		attackTimer -= Time.deltaTime;
		if (Input.GetKey(KeyCode.Alpha1))
        {
			if (attackCooldownTimer <= 0.05f) {
				// Attack confirmed
				attackCooldownTimer = attackCooldown;
				attackTimer = attackTime;

				attacking = true;
				Player.Instance.hitbox.hitting = true;
			}
        }
	}

	void Walk()
	{
		Vector2 currentPosition = transform.position;
		if (Input.GetButtonDown("Fire1") && !UIManager.Instance.IsPointerOverUIElement()) isLClickHeld = true;
		if (Input.GetButtonUp("Fire1")) isLClickHeld = false;
		if (isLClickHeld) {
			Vector2 moveTowards = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			
			moveDirection = moveTowards - currentPosition;
			moveDirection.Normalize();
			lastClick = moveTowards;
		}

		Vector2 target = moveDirection + currentPosition;
		if (Vector3.Distance(transform.position, lastClick) > 0.2f) {
			transform.position = Vector3.Lerp (currentPosition, target, moveSpeed * Time.deltaTime);

			float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Slerp (transform.rotation, 
		                                       Quaternion.Euler (0, 0, targetAngle + 180), 
		                                       turnSpeed * Time.deltaTime);
			
			StepAnim();
		}
		else {
			StopAnim();
		}
	}
	void TakeDamage(){
		// TODO: knockback
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
			Player.Instance.hitbox.hitting = false;
		} else {
			rightHand.transform.localPosition = new Vector3(attackScripts[stepToPlay].rightHandPos.x, attackScripts[stepToPlay].rightHandPos.y, -0.2f);
			rightHand.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].rightHandPos.z);
			leftHand.transform.localPosition = new Vector3(attackScripts[stepToPlay].leftHandPos.x, attackScripts[stepToPlay].leftHandPos.y, -0.2f);
			leftHand.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].leftHandPos.z);
			weaponObject.transform.localPosition = new Vector3(attackScripts[stepToPlay].weaponPos.x, attackScripts[stepToPlay].weaponPos.y, -9.3f);
			weaponObject.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].weaponPos.z);
		}
	}

	/**
	* 1 - always counting timer 1 or 2, moves tight foot on timer 1 and left on timer 2.	
	*/
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
		// Debug.Log(collided.CompareTag("Hitbox"));
		if (collided.CompareTag("Hitbox"))
		{
			Collided(collided);
		}
	}
	private void OnCollisionStay2D(Collision2D collision)
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
		if (hitter.hitting && hitter.faction != 0  && Player.Instance.invincibleTimer < 0) {
			float damage = UnityEngine.Random.Range(hitter.damageMin, hitter.damageMax);
			Player.Instance.TakeDamage(damage);
		}

	}

	// public static Vector3 getRelativePosition(Vector2 origin, Vector2 position) {
	// 	Vector2 distance = position - origin;
	// 	Vector2 relativePosition = Vector3.zero;
	// 	relativePosition.x = Vector2.Dot(distance, origin.right.normalized);
	// 	relativePosition.y = Vector2.Dot(distance, origin.up.normalized);
		
	// 	return relativePosition;
	// }


}
