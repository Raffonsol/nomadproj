using System.Collections;
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
	
    public float invincibilityTime = 0.3f;
    public float invincibleTimer;

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
			// Attack confirmed (but it might be ranged with no arrows)
			boredTimer = boredTime;
			attackCooldownTimer = attackCooldown;
			attackTimer = attackTime;

			attacking = true;
			
			if (attackDamageType == DamageType.Melee) {
				Player.Instance.GetCharById(charId).hitbox.hitting = true;
				Player.Instance.GetCharById(charId).hitbox.playerParty = true;
				hitBox.isTrigger = false;
			} else if (attackDamageType == DamageType.Ranged) {
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
					Debug.Log("no arrows");
					if (!leader) {
						Player.Instance.UnequipWeapon(self.id);
						// TODO autoequip
						// TODO make it not equip bow if there are no arrows
					}
					return;
				}
				// turn towards mouse
				Vector2 moveTowards = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				moveDirection = moveTowards - (Vector2)transform.position;
				moveDirection.Normalize();
				float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.Euler (0, 0, targetAngle + 180);
					
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
		hovering = false;
		UIManager.Instance.armorNeedsUpdate = true;
		UIManager.Instance.weaponNeedsUpdate = true;
	}
	void Walk()
	{
		if (isBeingTossed) return;
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
			if (leader || UnityEngine.Random.Range(0, 52) > 50)
			lastClick = moveTowards;
			
		}
		nextPoint = leader ? lastClick : GameOverlord.Instance.Pathfind(currentPosition, lastClick);
		moveDirection = nextPoint - currentPosition;
		moveDirection.Normalize();
		if (!leader)PerformAi(self.personality);
		else if (isLClickHeld || boredTimer > 0) {
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
	void TakeDamage(float damage) {
		// TODO: knockback
        if (invincibleTimer < 0) {
            // TODO: localize [12 = armor]
            float minDmg = damage - self.stats[12].value;
            if (minDmg < 0) minDmg = 0;
            self.life -= UnityEngine.Random.Range(minDmg,damage);
            invincibleTimer = invincibilityTime; 
            GameObject DamageText = Instantiate(GameOverlord.Instance.damagePrefab, transform);
            DamageText.GetComponent<DamageText>().textToDisplay = damage.ToString("0.00");
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
			Debug.Log("Arrow Shot");
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
		float speed = leader ? moveSpeed : moveSpeed + 0.1f*(float)(Vector2.Distance(currentPosition, (Vector2)Player.Instance.controller.transform.position)- Math.Abs(self.formation.x));
		if (((tempPersonality != Personality.Coward && Player.Instance.engagementTimer > 0.3f) || Player.Instance.engagementTimer > 3f) && Player.Instance.engagedMonster.Count > 0) {
			GameObject engage = Player.Instance.engagedMonster[0];
			if (engage == null) return;
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
			if (weapon.damageType == DamageType.Ranged) monsterSize += 300f;
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
					float monsterSize;
					try {
						monsterSize = monster.GetComponent<Monster>().size;
					} catch (NullReferenceException) {
						monsterSize = monster.GetComponent<Neutral>().size;
					}
					if (weapon.damageType == DamageType.Ranged) monsterSize += 200f;
					if (dist < monsterSize)AttemptAttack();
					if (dist > monsterSize* 0.5) {
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
	void KnockedBack(GameObject source, float amount) {
        if (amount <0.1f) return;
        knockBackLandPosition = Vector3.MoveTowards(transform.position,source.transform.position, amount*-2f);
        isBeingTossed = true;
        // TODO: Implemenent knockback
    }
	void BeTossed() {
		if (!isBeingTossed) return;
		if (Vector3.Distance(transform.position, knockBackLandPosition) > 0.2f) {
			transform.position = Vector3.Lerp (transform.position, knockBackLandPosition, 3f * Time.deltaTime);
		} else {
			isBeingTossed = false;
		}
	}
}
