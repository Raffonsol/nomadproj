﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;


public class ZombieController : MonoBehaviour {
	
	public float moveSpeed;
	public float turnSpeed;
	public float feetSpeed;
	public bool leader;
	public int charId;

	public GameObject leftFoot;
	public GameObject rightFoot;

	public GameObject rightHand;
    public GameObject leftHand;
	public GameObject weaponObject;

	private Vector2 moveDirection;
	private Vector2 lastClick;

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
	private float boredTime = 5f;
	private float boredTimer;
	private PolygonCollider2D hitBox;
	private bool hovering = false;
	private FriendlyChar self;
	private float reactingTime;
	private Color shadowColor;
	private Color selectionColor;
	private GameObject shadow;

	void Start()
	{
		Player.Instance.EquipWeapon(100000, new List<Part>(), charId);
		shadow = transform.Find("Player/shadow").gameObject;
		shadowColor = shadow.GetComponent<SpriteRenderer>().color;
		selectionColor = Color.white;
		Reset();
	}
	public void Reset() {
		self = Player.Instance.characters[charId];
		reactingTime = self.reactionTime;
		moveTimer1 = feetSpeed;
		feetY = leftFoot.transform.position.y;
		feetX = leftFoot.transform.position.x;
		feetZ = leftFoot.transform.position.z;

		weapon = Player.Instance.characters[charId].equipped.primaryWeapon;

		// finding player's attack cooldown from their weapon
		attackCooldown = Player.Instance.characters[charId].equipped.primaryWeapon.cooldown;
		attackCooldownTimer = attackCooldown;

		attackTime = attackCooldown;
		attackTimer = attackTime;

		hitBox = weaponObject.transform.Find(weapon.collidablePart.ToString()).gameObject.GetComponent<PolygonCollider2D>();
		hitBox.isTrigger = true;
	}

	private bool isLClickHeld = false;
	private Vector2 nextPoint;

	void Update () {
		ListenForClick();
		Walk();
		Attack();
		TakeDamage();
		Die();
	}

	void Attack()
	{
		SwingAnim();
		if (attackCooldownTimer > 0)
		attackCooldownTimer -= Time.deltaTime;
		attackTimer -= Time.deltaTime;
		if (leader && Input.GetKey(KeyCode.Alpha1))
        {
			AttemptAttack();
        }
	}
	void AttemptAttack() {
		if (attackCooldownTimer <= 0) {
			// Attack confirmed
			boredTimer = boredTime;
			attackCooldownTimer = attackCooldown;
			attackTimer = attackTime;

			attacking = true;
			Player.Instance.characters[charId].hitbox.hitting = true;
			hitBox.isTrigger = false;
		}
	}

	public void ListenForClick() {
		if (Input.GetButtonUp("Fire1") && hovering) {
				if (!leader) BecomeLeader();
		}
	}
	public void BecomeLeader() {
		leader = true;
		Player.Instance.controller.gameObject.GetComponent<ZombieController>().leader = false;
		Player.Instance.activeCharId = charId;
		Player.Instance.ResetContol();
		Camera.main.GetComponent<CameraController>().SetFollowing();
		hovering = false;
		UIManager.Instance.armorNeedsUpdate = true;
		UIManager.Instance.weaponNeedsUpdate = true;
	}
	void Walk()
	{
		if (boredTimer > 0)boredTimer -=Time.deltaTime;
		// if (!leader) {
		// 	distToMain.x = transform.position.x - Player.Instance.controller.gameObject.transform.position.x;
		// 	distToMain.y = transform.position.y - Player.Instance.controller.gameObject.transform.position.y;
		// }
		Vector2 currentPosition = transform.position;
		float speed = leader ? moveSpeed : moveSpeed + 0.1f*(float)Math.Pow(Vector2.Distance(currentPosition, (Vector2)Player.Instance.controller.transform.position)- Math.Abs(self.formation.x), 2);
		if (Input.GetButtonDown("Fire1") && !UIManager.Instance.IsPointerOverUIElement()) isLClickHeld = true;
		if (Input.GetButtonUp("Fire1")) isLClickHeld = false;
		if (isLClickHeld) {
			boredTimer = boredTime;
			Vector2 moveTowards = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			// if a follower, gotta offset this move target
			if (!leader) {
				moveTowards.x+=self.formation.x;
				moveTowards.y+=self.formation.y;
			}
			
			if (leader)
			lastClick = moveTowards;
			else
			Task.Delay((int)Math.Round(reactingTime*1000)).ContinueWith(t=> lastClick = moveTowards);
			
		}
		nextPoint = leader ? lastClick : GameOverlord.Instance.Pathfind(currentPosition, lastClick);
		moveDirection = nextPoint - currentPosition;
		moveDirection.Normalize();
		if (!leader)PerformAi(self.personality);
		if (isLClickHeld || boredTimer > 0) {
			Vector2 target = moveDirection + currentPosition;
			if (Vector3.Distance(transform.position, lastClick) > 0.2f) {
				transform.position = Vector3.Lerp (currentPosition, target, speed * Time.deltaTime);

				float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.Slerp (transform.rotation, 
												Quaternion.Euler (0, 0, targetAngle + 180), 
												turnSpeed * Time.deltaTime);
				
				StepAnim();
			}
			else {
				StopAnim();
			}
		} else {
			PerformAi(self.personality);
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
			Player.Instance.characters[charId].hitbox.hitting = false;
			hitBox.isTrigger = true;
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
		if (attacking) return;
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
		if (hitter.hitting && hitter.faction != 0  && Player.Instance.characters[charId].invincibleTimer <= 0) {
			float damage = UnityEngine.Random.Range(hitter.damageMin, hitter.damageMax);
			Player.Instance.TakeDamage(damage, charId);
		}

	}

	void Die() {
		if (self.life > 0) return;
		GameObject deathImage = Instantiate(GameOverlord.Instance.deathPrefab, new Vector2(transform.position.x, transform.position.y),   Quaternion.Euler(0, 0, 0));
		deathImage.transform.parent = null;
		Player.Instance.characters.Remove( Player.Instance.characters.Single( c => c.id == self.id));
		if (leader) {
			Player.Instance.LeaderDied();
		}
		Destroy(transform.gameObject);
	}

	void OnMouseOver()
    {
		hovering = true;
		if(!leader)shadow.GetComponent<SpriteRenderer>().color = selectionColor;
    }
	void OnMouseExit()
    {
        hovering = false;
		shadow.GetComponent<SpriteRenderer>().color = shadowColor;
    }

	void PerformAi(Personality tempPersonality) {
		
		Vector2 target;
		Vector2 currentPosition = transform.position;
		float speed = leader ? moveSpeed : moveSpeed + 0.1f*(float)(Vector2.Distance(currentPosition, (Vector2)Player.Instance.controller.transform.position)- Math.Abs(self.formation.x));
		if ((tempPersonality != Personality.Coward && Player.Instance.engagementTimer > 0.3f) || Player.Instance.engagementTimer > 3f) {
			GameObject engage = Player.Instance.engagedMonster;
			Vector2 threateningTar = engage.transform.position;
			nextPoint = GameOverlord.Instance.Pathfind(currentPosition, threateningTar);
			moveDirection = nextPoint - currentPosition;
			moveDirection.Normalize();
			target = moveDirection + currentPosition;

			float dist = Vector3.Distance(transform.position, threateningTar);
			float monsterSize = engage.GetComponent<Monster>().size;
			if (dist < monsterSize)AttemptAttack();
			if (dist > monsterSize*0.3f) {
				transform.position = Vector3.Lerp (currentPosition, target, speed * Time.deltaTime);
				StepAnim();
			}
			else {
				StopAnim();
			}
			float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.Slerp (transform.rotation, 
												Quaternion.Euler (0, 0, targetAngle + 180), 
												turnSpeed * Time.deltaTime);
			return;
		}
		bool justFollow = false;
		switch (tempPersonality) {
			case (Personality.Hothead):
				if (GameOverlord.Instance.nearbyMonsters.Count > 0) {
					GameObject monster = GameOverlord.Instance.nearbyMonsters[0].gameObject;
					Vector2 angryTar = monster.transform.position;
					nextPoint = GameOverlord.Instance.Pathfind(currentPosition, angryTar);
					moveDirection = nextPoint - currentPosition;
					moveDirection.Normalize();
					target = moveDirection + currentPosition;
					
					float dist = Vector3.Distance(transform.position, angryTar);
					float monsterSize = monster.GetComponent<Monster>().size;
					if (dist < monsterSize)AttemptAttack();
					if (dist > monsterSize*0.3f) {
						transform.position = Vector3.Lerp (currentPosition, target, speed * Time.deltaTime);
						StepAnim();
					}
					else {
						StopAnim();						
					}
					float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
					transform.rotation = Quaternion.Slerp (transform.rotation, 
														Quaternion.Euler (0, 0, targetAngle + 180), 
														turnSpeed * Time.deltaTime);
				} else {justFollow = true;}
				
				break;
			case (Personality.Clingy):
				justFollow = true;
				break;
		}
		if (justFollow) {
			nextPoint = leader ? lastClick : GameOverlord.Instance.Pathfind(currentPosition, lastClick);
			moveDirection = nextPoint - currentPosition;
			moveDirection.Normalize();
			target = moveDirection + currentPosition;
			if (Vector3.Distance(transform.position, lastClick) > 0.2f) {
				transform.position = Vector3.Lerp (currentPosition, target, speed * Time.deltaTime);

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
	}

}
