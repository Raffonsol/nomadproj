using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
	
    public float invincibilityTime = 0.3f;
    public float invincibleTimer;

	public float armorDefense=0;

	private Vector2 moveDirection;
	private Vector2 lastClick;
	private Vector2 lastNpcClick;

	private float moveTimer1 = 0;
	private float moveTimer2 = 0;
	private float feetY;
	private float feetX;
	private float feetZ;

	private Weapon weapon;

	private float attackCooldown;
	public float attackCooldownTimer;

	private float attackTime;
	private float attackTimer;
	private float skillTimer;
	private bool attacking = false;
	private float boredTime = 5f;
	private float boredTimer;
	private PolygonCollider2D hitBox;
	private bool hovering = false;
	public FriendlyChar self;
	private float reactingTime;
	private Color shadowColor;
	private Color selectionColor;
	private GameObject shadow;
	private DamageType attackDamageType;

	private float regenInterval = 1f;
	private float regenTimer = 1f;

    private Vector2 knockBackLandPosition;
	private bool isBeingTossed = false;
	private bool isStunned = false;

	private bool usingSkill = false;
	private CharSkill castingSkill = null;
	private Vector2 skillMoveTarget;
	private GameObject skillTargetTarget;
	private bool skillMoveLocked = false;
	private float skillKnockBack = 0f;
	private float knockTimer = 0f;
	private float stunTimer = 0f;
	private float skillMoveProximity =0.2f;
	private float skillRunTime =0f;
	private List<Combatant> skillHits;

	public void DoStart()
	{
		Player.Instance.EquipWeapon(100000, new List<Part>(), charId);
		shadow = transform.Find("Player/shadow").gameObject;
		shadowColor = Color.black;
		shadowColor.a = 0.2f;
		selectionColor = Color.white;
		
		Reset();
		ResetAppearance();
        EquipStartingGear();
		LoadBonuses();
	}
	public void Reset() {
		
		lastClick = transform.position;
		lastNpcClick = lastClick;
		self = Player.Instance.GetCharById(charId);

		leftFoot = transform.Find("Player/Body/LFoot").gameObject;
		rightFoot = transform.Find("Player/Body/RFoot").gameObject;
		rightHand = transform.Find("Player/Body/Instance/RHand").gameObject;
		leftHand = transform.Find("Player/Body/Instance/LHand").gameObject;

		reactingTime = self.reactionTime;
		moveTimer1 = feetSpeed;
		feetY = leftFoot.transform.position.y;
		feetX = leftFoot.transform.position.x;
		feetZ = leftFoot.transform.position.z;

		weapon = self.equipped.primaryWeapon;

		// finding player's attack cooldown from their weapon
		attackCooldown = self.equipped.primaryWeapon.cooldown;
		attackCooldownTimer = attackCooldown;

		
		attackTime = attackCooldown;
		attackTimer = attackTime;
		
        invincibleTimer = invincibilityTime;

		attackDamageType = weapon.damageType;
		if (weaponObject && attackDamageType == DamageType.Melee) {
			hitBox = weaponObject.transform.Find(weapon.collidablePart.ToString()).gameObject.GetComponent<PolygonCollider2D>();
			hitBox.isTrigger = true;
		}
		
	}
	 void EquipStartingGear() {
        if (self.equippedOnLoad.Length > 0) {
			bool left = false;
			for(int i = 0; i <self.equippedOnLoad.Length; i++){
				Equipment gear = GameLib.Instance.GetEquipmentById(self.equippedOnLoad[i]);
				Player.Instance.EquipArmor(
					gear,
					left,
					charId
				);
				left = (!left && Array.IndexOf( new[] { Slot.Hand, Slot.Foot, Slot.Pauldron }, gear.slot) > -1 );
			}
			UIManager.Instance.armorNeedsUpdate = true;
		}
		List<Part> partsUsed = new List<Part>();
		if (self.weaponOnLoadParts.Length > 0) {
			for(int i = 0; i <self.weaponOnLoadParts.Length; i++){
				partsUsed.Add(GameLib.Instance.GetPartById(self.weaponOnLoadParts[i]));
			}
		}
		if (self.weaponOnLoad != 0) {
			Player.Instance.EquipWeapon(self.weaponOnLoad, partsUsed, charId );
			UIManager.Instance.weaponNeedsUpdate = true;
		}
    }

	private void ResetAppearance() {
		bool left = false;
		for(int i = 0; i <self.appearance.bodyLooks.Length; i++){
			// setting appearance looks
			BodyLook look = GameLib.Instance.GetBodyPartById(self.appearance.bodyLooks[i]);
			// don't overwrite armor
			if (look.slot == Slot.Clothing && self.equipped.chest != null) continue;

			GameObject current = gameObject.transform.Find("Player/Body/" +Util.SlotToBodyPosition(look.slot, left, true)).gameObject;
			current.GetComponent<SpriteRenderer>().sprite = look.look;
			if (look.slot != Slot.Clothing) // don't change clothing color
			current.GetComponent<SpriteRenderer>().color = GameLib.Instance.skinColorPresets[self.appearance.skinColor];

			// save object of part
			self.appearance.SetPartObject(current, look.slot);

			// if it is a hand or a foot, next will be the same and lets make it left
			left = (!left && Array.IndexOf( new[] { Slot.Hand, Slot.Foot }, look.slot) > -1 );

			if (left) i--;
		}
		
	}

	private void LoadBonuses() {
		self.bonuses = new List<Bonus>();
		self.ownedAbilities = new List<PassiveAbility>();
		for(int i = 0; i <self.bonusOnLoad.Length; i++){
			Player.Instance.ApplyBonus(self.id, GameLib.Instance.allBonuses[self.bonusOnLoad[i]]);
		}
		
	}

	private bool isLClickHeld = false;
	private Vector2 nextPoint;

	void Update () {
		if (self == null) return;
		ListenForClick();
		Walk();
		Attack();
		Die();
		Regen();
		CountInvincibleTimer();
		BeTossed();
	}
	void CountInvincibleTimer() {
		if (invincibleTimer >= 0) {
			invincibleTimer -= Time.deltaTime;
		}
	}

	void Attack()
	{
		SwingAnim();
		RunSkillCast();
		if (attackCooldownTimer > 0)
		attackCooldownTimer -= Time.deltaTime;
		attackTimer -= Time.deltaTime;
		if (skillTimer > 0)
		skillTimer -= Time.deltaTime;
		else if(skillMoveLocked) skillMoveLocked = false;
		if (skillRunTime > 0)
		skillRunTime -= Time.deltaTime;
		else if(usingSkill) EndSkill();
		if (knockTimer > 0)
		knockTimer -= Time.deltaTime;
		else if(isBeingTossed) isBeingTossed = false;
		if (stunTimer > 0)
		stunTimer -= Time.deltaTime;
		else if(isStunned) isStunned = false;
		if(self.skills!=null)for (int i = 0; i < self.skills.Count; i++)
		{
			self.skills[i].cooldownTimer -= Time.deltaTime;
		}
		if (leader && Input.GetKey(KeyCode.Alpha1))
        {
			AttemptAttack();
        }
		if (leader && Input.GetKey(KeyCode.Alpha2))
        {
			AttemptSkill(0);
        }
	}
	public void TriggerSkill(int skill){
		if (leader) {
			if (skill == 1)
			AttemptAttack();
			if (skill>1)
			AttemptSkill(skill-2);
		}
	}
	void AttemptSkill(int skillIndex) {
		if (self.skills == null || self.skills.Count <skillIndex+1) return;
		 CharSkill castSkill =self.skills[skillIndex];
		if (self.skills[skillIndex].id == 0) {
			// weapon skill
			castSkill=GameLib.Instance.getWeaponsSkill(self.equipped.primaryWeapon.id);
		} 
		// cooldown timers
		if (self.skills[skillIndex].cooldownTimer>0){
			return;
		}
		skillHits = new List<Combatant>();
		Player.Instance.GetCharById(charId).hitbox.recordHits=true;
		if ((castSkill.skillTypes.Contains(SkillType.Move)&&castSkill.moveSystem>=2) 
			||castSkill.skillTypes.Contains(SkillType.TargetedDamage)) {
			switch (castSkill.targetSystem) {
				case 0: {
					skillTargetTarget =GameOverlord.Instance.nearbyMonsters[0];// TODO proximity calculator
					break;
				}
				case 1:{
					if (GameOverlord.Instance.nearbyMonsters.Count < 1) return;
					skillTargetTarget = GameOverlord.Instance.nearbyMonsters[UnityEngine.Random.Range(0, GameOverlord.Instance.nearbyMonsters.Count-1)];
					break;
				}
			}
		}
		if (castSkill.skillTypes.Contains(SkillType.Move)) {
			switch (castSkill.moveSystem) {
				case 0: {
					skillMoveTarget = transform.position+transform.right*-1f*castSkill.offset; // Vector3.MoveTowards(transform.position,hitBox.transform.position, castSkill.offset);
					skillMoveProximity =0.2f;
					break;
				}
				case 1: {
					skillMoveTarget = transform.position+transform.right*castSkill.offset;
					skillMoveProximity =0.2f;
					break;
				}
				case 2:
				case 3:{
					if (GameOverlord.Instance.nearbyMonsters.Count < 1||skillTargetTarget==null) return;
					skillMoveTarget = skillTargetTarget.transform.position;
					skillMoveProximity =-0.4f;
					GetComponent<CircleCollider2D>().isTrigger = true;
					break;
				}
			} 
			skillMoveLocked = true;
		}
		if (castSkill.skillTypes.Contains(SkillType.TargetedDamage)) {
			Monster monster = skillTargetTarget.GetComponent<Monster>();
			monster.TakeDamage(castSkill.damageBase);
            monster.KnockedBack(gameObject, castSkill.knockBack);
		}
		if (castSkill.skillTypes.Contains(SkillType.CreateDamageObject)) {
			GameObject arrow = Instantiate(castSkill.impactCollision,transform.position, transform.rotation);
			Projectile newSettings = new Projectile();
			float dmg = castSkill.damageBase+Player.Instance.CalculateDamage(self.id);
			newSettings.minDamage = dmg;
			newSettings.maxDamage = dmg * 1.5f;
			newSettings.speed=3f;newSettings.maxDistance=1000f;newSettings.maxLife=70f;
			newSettings.knockBack=castSkill.knockBack;
			arrow.gameObject.AddComponent<ProjectileItem>();
			arrow.gameObject.GetComponent<ProjectileItem>().projectileSettings = newSettings;
			arrow.gameObject.GetComponent<ProjectileItem>().faction = 0;
			arrow.gameObject.GetComponent<ProjectileItem>().shooter = gameObject;
			arrow.gameObject.GetComponent<ProjectileItem>().playerParty = true;
			arrow.gameObject.GetComponent<ProjectileItem>().consumableId = 800000;
			arrow.gameObject.GetComponent<ProjectileItem>().Go();
		}
		skillTimer = castSkill.speed;
		skillRunTime = castSkill.maxRunTime;
		skillKnockBack+= castSkill.knockBack;
		Player.Instance.GetCharById(charId).hitbox.knockBack+= castSkill.knockBack;
		self.skills[skillIndex].cooldownTimer=self.skills[skillIndex].cooldown;
		
		Player.Instance.GetCharById(charId).hitbox.hitting = true;
		Player.Instance.GetCharById(charId).hitbox.playerParty = true;
		if (attackDamageType == DamageType.Melee) {
			hitBox.isTrigger = false;
			self.hitbox.hitting = true;
			self.hitbox.playerParty = true;
		}
		usingSkill = true;
		castingSkill = castSkill;

	}
	public void AddSkillHit(Combatant combatant) {
		skillHits.Add(combatant);
		if (castingSkill.skillTypes.Contains(SkillType.TargetedLifesteal) && castingSkill.targetSystem==4){
			Heal(castingSkill.offset);
		}
	}
	void AttemptAttack() {
		if (attackCooldownTimer <= 0) {
			// Attack confirmed (but it might be ranged with no arrows)
			boredTimer = boredTime;
			attackCooldownTimer = attackCooldown;
			attackTimer = attackTime;

			attacking = true;
			
			if (attackDamageType == DamageType.Melee) {
				self.hitbox.hitting = true;
				self.hitbox.playerParty = true;
				hitBox.isTrigger = false;
			} else if (attackDamageType == DamageType.Ranged) {
				// turn towards mouse
				Vector2 moveTowards = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				moveDirection = moveTowards - (Vector2)transform.position;
				moveDirection.Normalize();
				float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.Euler (0, 0, targetAngle + 180);
				if (weapon.ammo == ConsumableType.Rock && Player.Instance.GetPartsByType(weapon.ammo).Count > 0) {
					
					Part item = Player.Instance.parts.First( s => s.consumableType == weapon.ammo);
					int itemId = item.id;
					Player.Instance.RemovePart(item.id);
					// Create arrow projectile and give it damage
					// STATSET
					GameObject arrow = Instantiate(item.visual,transform.position, transform.rotation);
					Projectile newSettings = item.projectileSettings;
					float dmg = Player.Instance.CalculateDamage(self.id);
					newSettings.minDamage = dmg;
					newSettings.maxDamage = dmg * 1.5f;
					arrow.gameObject.AddComponent<ProjectileItem>();
					arrow.gameObject.GetComponent<ProjectileItem>().projectileSettings = item.projectileSettings;
					arrow.gameObject.GetComponent<ProjectileItem>().faction = 0;
					arrow.gameObject.GetComponent<ProjectileItem>().shooter = gameObject;
					arrow.gameObject.GetComponent<ProjectileItem>().playerParty = true;
					arrow.gameObject.GetComponent<ProjectileItem>().consumableId = itemId;
					arrow.gameObject.GetComponent<ProjectileItem>().Go();
					Debug.Log("rock shot");
				}
				else if (Player.Instance.GetConsumablesByType(weapon.ammo).Count > 0) {
					
					Consumable item = Player.Instance.consumables.First( s => s.consumableType == weapon.ammo);
					int itemId = item.id;
					Player.Instance.consumables.Remove(item);
					// Create arrow projectile and give it damage
					// STATSET
					GameObject arrow = Instantiate(item.visual,transform.position, transform.rotation);
					Projectile newSettings = item.projectileSettings;
					float dmg = Player.Instance.CalculateDamage(self.id);
					newSettings.minDamage = dmg;
					newSettings.maxDamage = dmg * 1.5f;
					arrow.gameObject.GetComponent<ProjectileItem>().projectileSettings = item.projectileSettings;
					arrow.gameObject.GetComponent<ProjectileItem>().faction = 0;
					arrow.gameObject.GetComponent<ProjectileItem>().shooter = gameObject;
					arrow.gameObject.GetComponent<ProjectileItem>().playerParty = true;
					arrow.gameObject.GetComponent<ProjectileItem>().consumableId = itemId;
					arrow.gameObject.GetComponent<ProjectileItem>().Go();
				} else {
					if (!leader) {
						Player.Instance.UnequipWeapon(self.id);
						UIManager.Instance.AutoEquipWeapons();
					}
					GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, transform);
					DamageText.GetComponent<DamageText>().textToDisplay = "No arrows";
					return;
				}
				
					
			}
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
		UIManager.Instance.UpdateSkillSquare();
		hovering = false;
		UIManager.Instance.armorNeedsUpdate = true;
		UIManager.Instance.weaponNeedsUpdate = true;
	}
	void Walk()
	{
		if (isBeingTossed || skillMoveLocked ) return;
		if (boredTimer > 0)boredTimer -=Time.deltaTime;
		// if (!leader) {
		// 	distToMain.x = transform.position.x - Player.Instance.controller.gameObject.transform.position.x;
		// 	distToMain.y = transform.position.y - Player.Instance.controller.gameObject.transform.position.y;
		// }
		Vector2 currentPosition = transform.position;
		float speed = leader ? moveSpeed : moveSpeed + 0.1f*(float)Math.Pow(Vector2.Distance(currentPosition, (Vector2)Player.Instance.controller.transform.position)- Math.Abs(self.formation.x), 2);
		if (Input.GetButtonDown("Fire1") && !UIManager.Instance.IsPointerOverUIElement() && !Player.Instance.isTeamHovered) 
			isLClickHeld = true;
		if (Input.GetButtonUp("Fire1")) isLClickHeld = false;
		if (isLClickHeld) {
			boredTimer = boredTime;
			Vector2 moveTowards = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			// if a follower, gotta offset this move target
			if (!leader) {
				moveTowards.x+=self.formation.x;
				moveTowards.y+=self.formation.y;
			}
			// Leader goes straight to click, non-leader has a chance to ignore, resulting in delay to match move position. This is here to simulate reaction time of ofllowers
			if (leader) // || UnityEngine.Random.Range(0, 52) > 50) // moved to AI
			lastClick = moveTowards;
			lastNpcClick = moveTowards;
		}
		nextPoint = leader ? lastClick : GameOverlord.Instance.Pathfind(currentPosition, lastClick);
		moveDirection = nextPoint - currentPosition;
		moveDirection.Normalize();
		if (!leader)PerformAi(self.personality);
		else if (isLClickHeld || boredTimer > 0) {
			Vector2 target = moveDirection + currentPosition;
			if (Vector3.Distance(transform.position, lastClick) > 0.2f) {
				if (!isStunned)
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
	public void TakeDamage(float damage) {
        if (invincibleTimer < 0) {
            float minDmg = damage - armorDefense;
            if (minDmg < 0) minDmg = 0;
            self.life -= UnityEngine.Random.Range(minDmg,damage);
            invincibleTimer = invincibilityTime; 
            GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, transform);
            DamageText.GetComponent<DamageText>().textToDisplay = damage.ToString("0.00");
        }
        
	}
	void RunSkillCast() {
		if (!usingSkill) return;

		if (castingSkill.skillTypes.Contains(SkillType.StanceScript)) {
			SkillStanceAnim();
		}
		if (castingSkill.skillTypes.Contains(SkillType.Move)) {
			SkillMoveAnim();
		}
		
	}
	void SkillStanceAnim() {

		Step[] attackScripts = (castingSkill.stance.attackScripts).Reverse().ToArray();
		
		int steps = attackScripts.Length;
		int stepToPlay = (int)Math.Round((skillTimer/castingSkill.speed)*steps, 0);
		stepToPlay -= 1;
		// Debug.Log(stepToPlay + " -" + attackTimer);
		if (stepToPlay < 0) {
			rightHand.transform.localPosition = new Vector3(weapon.instance.rightHandPos.x, weapon.instance.rightHandPos.y, -0.2f);
			rightHand.transform.localRotation = Quaternion.Euler(0, 0, weapon.instance.rightHandPos.z);
			leftHand.transform.localPosition = new Vector3(weapon.instance.leftHandPos.x, weapon.instance.leftHandPos.y, -0.2f);
			leftHand.transform.localRotation = Quaternion.Euler(0, 0, weapon.instance.leftHandPos.z);
			weaponObject.transform.localPosition = new Vector3(weapon.instance.weaponPos.x, weapon.instance.weaponPos.y, -9.3f);
			weaponObject.transform.localRotation = Quaternion.Euler(0, 0, weapon.instance.weaponPos.z);
			EndSkill();
		} else {
			rightHand.transform.localPosition = new Vector3(attackScripts[stepToPlay].rightHandPos.x, attackScripts[stepToPlay].rightHandPos.y, -0.2f);
			rightHand.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].rightHandPos.z);
			leftHand.transform.localPosition = new Vector3(attackScripts[stepToPlay].leftHandPos.x, attackScripts[stepToPlay].leftHandPos.y, -0.2f);
			leftHand.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].leftHandPos.z);
			weaponObject.transform.localPosition = new Vector3(attackScripts[stepToPlay].weaponPos.x, attackScripts[stepToPlay].weaponPos.y, -9.3f);
			weaponObject.transform.localRotation = Quaternion.Euler(0, 0, attackScripts[stepToPlay].weaponPos.z);
		}
	}
	void SkillMoveAnim() {
		if (!skillMoveLocked || isBeingTossed) return;
		if (Vector3.Distance(transform.position, skillMoveTarget) > skillMoveProximity) {
			transform.position = Vector3.Lerp (transform.position, skillMoveTarget, (castingSkill.moveSystem==3?99f : castingSkill.speed) * Time.deltaTime);
		} else {
			EndSkill();
			lastClick=transform.position;
		}
	}
	void EndSkill() {
		skillMoveLocked = false;
		skillKnockBack-= castingSkill.knockBack;
		Player.Instance.GetCharById(charId).hitbox.knockBack-= castingSkill.knockBack;
		usingSkill = false;
		Player.Instance.GetCharById(charId).hitbox.hitting = false;
		GetComponent<CircleCollider2D>().isTrigger = false;
		if (attackDamageType == DamageType.Melee) hitBox.isTrigger = true;
		Player.Instance.GetCharById(charId).hitbox.recordHits=false;
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
			Player.Instance.GetCharById(charId).hitbox.hitting = false;
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
		if (collided.CompareTag("Projectile"))
		{
			Shot(collided);
		}
	}
	private void OnCollisionStay2D(Collision2D collision)
	{
		// Debug.Log(collision.collider.CompareTag("Hitbox"));
		if (collision.collider.CompareTag("Hitbox"))
		{
			Collided(collision.collider);
		}
	}
	void Collided(Collider2D collided)
	{
		GameObject target = collided.gameObject;
		HitBox hitter = target.GetComponent<HitBox>();
		if (hitter.hitting && hitter.faction != 0  && invincibleTimer <= 0) {
			float damage = UnityEngine.Random.Range(hitter.damageMin, hitter.damageMax);
			TakeDamage(damage);
            KnockedBack(target, hitter.knockBack);
		}
	}
	void Shot(Collider2D collided)
	{
		GameObject target = collided.gameObject;
		ProjectileItem hitter = target.GetComponent<ProjectileItem>();
		if (hitter.faction != 0  && invincibleTimer <= 0) {
			float damage = UnityEngine.Random.Range(hitter.projectileSettings.minDamage, hitter.projectileSettings.maxDamage);
			TakeDamage(damage);
            KnockedBack(target, hitter.projectileSettings.knockBack);
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

		// run drop abilities
		for(int i = 0; i <self.ownedAbilities.Count; i++){
			if (self.ownedAbilities[i] == PassiveAbility.DropWeaponOnDeath) {
				for(int j = 0; j <self.equipped.partsBeingUsed.Count; j++){
					Player.Instance.AddPart(self.equipped.partsBeingUsed[j].id);
				}
			}
			if (self.ownedAbilities[i] == PassiveAbility.DropArmorOnDeath) {
				if (self.equipped.head != null){
					Player.Instance.AddEquipment(self.equipped.head.id);
				}
				if (self.equipped.chest != null){
					Player.Instance.AddEquipment(self.equipped.chest.id);
				}
				if (self.equipped.rightPauldron != null){
					Player.Instance.AddEquipment(self.equipped.rightPauldron.id);
				}
				if (self.equipped.leftPauldron != null){
					Player.Instance.AddEquipment(self.equipped.leftPauldron.id);
				}
				if (self.equipped.rightHand != null){
					Player.Instance.AddEquipment(self.equipped.rightHand.id);
				}
				if (self.equipped.leftHand != null){
					Player.Instance.AddEquipment(self.equipped.leftHand.id);
				}
				if (self.equipped.rightFoot != null){
					Player.Instance.AddEquipment(self.equipped.rightFoot.id);
				}
				if (self.equipped.leftFoot != null){
					Player.Instance.AddEquipment(self.equipped.leftFoot.id);
				}
			}
		}

		// make formation available again
		GameLib.Instance.MakeFormtionAvailable(self.formation);

		Destroy(transform.gameObject);
		
		
	}

	void OnMouseOver()
    {
		hovering = true;
		Player.Instance.isTeamHovered = true;
		if(!leader)shadow.GetComponent<SpriteRenderer>().color = selectionColor;
    }
	void OnMouseExit()
    {
		hovering = false;
		Player.Instance.isTeamHovered = false;
		shadow.GetComponent<SpriteRenderer>().color = shadowColor;
    }

	/**
	* don't overthink the arg name	
	*/
	void PerformAi(Personality tempPersonality) {
		
		Vector2 target;
		Vector2 currentPosition = transform.position;
		if (Player.Instance.controller == null) {
			Debug.LogError("Controller was null, which it should never be! " + Player.Instance.activeCharId+" - "+Player.Instance.characters.Count);
		}
		float speed = leader ? moveSpeed : moveSpeed + 0.1f*(float)(Vector2.Distance(currentPosition, (Vector2)Player.Instance.controller.transform.position)- Math.Abs(self.formation.x));
		if (((tempPersonality != Personality.Coward && Player.Instance.EngagedFor() > 0.3f) || Player.Instance.EngagedFor() > 2.5f) && Player.Instance.engagedMonster.Count > 0) {
			// Conditions for normally cahsing/fighting, if these are not met, behaviour will depend on personality
			GameObject engage = Player.Instance.engagedMonster[0];
			if (engage != null)
			AiFight(engage, speed);
			return;
		}
		bool justFollow = false;
		switch (tempPersonality) {
			case (Personality.Hothead):
				if (GameOverlord.Instance.nearbyMonsters.Count > 0) {
					try{
					GameObject monster = GameOverlord.Instance.nearbyMonsters[0].gameObject;
					AiFight(monster, speed);
					
					} catch (MissingReferenceException e){
						justFollow = true;
						GameOverlord.Instance.nearbyMonsters = new List<GameObject>();
					}
					catch (NullReferenceException e){
						justFollow = true;
						GameOverlord.Instance.nearbyMonsters = new List<GameObject>();
					}
				} else {justFollow = true;}
				
				break;
			case (Personality.Clingy):
			default:
				justFollow = true;
				break;
		}
		if (justFollow) {
			if ((tempPersonality != Personality.Lazy && UnityEngine.Random.Range(0, 52) > 50) || UnityEngine.Random.Range(0, 102) > 100)
				lastClick = lastNpcClick;
			nextPoint = leader ? lastClick : GameOverlord.Instance.Pathfind(currentPosition, lastClick);
			moveDirection = nextPoint - currentPosition;
			moveDirection.Normalize();
			target = moveDirection + currentPosition;
			if (Vector3.Distance(transform.position, lastClick) > 0.2f){
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
	void AiFight( GameObject engage, float speed) {
		Vector2 target;
		Vector2 currentPosition = transform.position;
		Vector2 threateningTar = engage.transform.position;
		nextPoint = GameOverlord.Instance.Pathfind(currentPosition, threateningTar);
		moveDirection = nextPoint - currentPosition;
		moveDirection.Normalize();
		target = moveDirection + currentPosition;

		float dist = Vector3.Distance(transform.position, threateningTar);
		float monsterSize;
		try {
			monsterSize = engage.GetComponent<Monster>().size;
		} catch (NullReferenceException) {
			monsterSize = 2;
		}
			// HARDCODED arrow distance
		if (weapon.damageType == DamageType.Ranged) monsterSize += 300f;
		AiSkills(dist);
		if (dist < monsterSize)AttemptAttack();
		if (dist > monsterSize * 0.5f) {
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
											
	}
	void AiSkills( float dist) {
		if (UnityEngine.Random.Range(0, 121) < 120) return;
		if (self.oddities!=null && self.oddities.Contains(Oddity.Conservative)&& UnityEngine.Random.Range(0, 171) < 170) return;
		if(self.skills!=null)for (int i = 0; i < self.skills.Count; i++)
		{
			CharSkill caste = self.skills[i];
			if (caste.id == 0) {
				// weapon skill
				caste=GameLib.Instance.getWeaponsSkill(self.equipped.primaryWeapon.id);
			} 
			if (dist < caste.npcCastRange)AttemptSkill(i);
		}
	}
	
	void Regen() {
		if (regenTimer > 0) {
			regenTimer -=Time.deltaTime;
		} else {
			regenTimer = regenInterval;
			// stat 9 is regen, so healing that much
			Heal(self.stats[9].value);
		}
	}
	void Heal (float value){
		self.life += value;
		// stat 0 is maxLife, so capping life at max
		if (self.life > self.stats[0].value) {
			self.life = self.stats[0].value;
		}
	}
	public void KnockedBack(GameObject source, float amount) {
        if (amount <0.1f || skillMoveLocked) return;
        knockBackLandPosition = Vector3.MoveTowards(transform.position,source.transform.position, amount*-2f);
        isBeingTossed = true;
		knockTimer = 2f;
    }
	void BeTossed() {
		if (!isBeingTossed) return;
		if (Vector3.Distance(transform.position, knockBackLandPosition) > 0.2f) {
			transform.position = Vector3.Lerp (transform.position, knockBackLandPosition, 3f * Time.deltaTime);
		} else {
			isBeingTossed = false;
			lastClick=transform.position;
		}
	}
	public void Stun(float duration) {
        stunTimer = duration;
		isStunned = true;
    }
	public bool IsRanged() {
		return attackDamageType == DamageType.Ranged;
	}
}
