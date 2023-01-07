using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Neutral : Combatant
{

    public string name;
    [SerializeField]
    public Appearance appearance;

    public int startingWeaponId;
    public int[] startingWeaponParts;
    public int arrowId;

    public GameObject leftFoot;
	public GameObject rightFoot;

	public GameObject rightHand;
    public GameObject leftHand;
	public GameObject weaponObject;

    public DamageType attackDamageType;

    private float attackTime;
    public Weapon weapon;
    private HitBox weaponHitBox;
    
	private GameObject shadow;
	private Color shadowColor;
	private Color selectionColor;

	private float feetY;
	private float feetX;
	private float feetZ;

    private bool attacking = false;
	private bool hovering = false;

    void Update() {
        ListenForClick();
    }

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
        shadow = transform.Find("Player/shadow").gameObject;
        shadowColor = Color.black;
		shadowColor.a = 0.2f;
		selectionColor = Color.white;
        
        Equip(startingWeaponId, parts);
    }

    public void ListenForClick() {
        // Debug.Log(hovering, Input.GetButtonUp("Fire1"));
		if (Input.GetButtonUp("Fire1") && hovering) {
            Debug.Log("convert");
            Player.Instance.ConvertNeutral(gameObject);
		}
	}

    protected override void Attack()
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
    
    protected override void StepAnim()
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
	protected override void StopAnim()
	{
        rightFoot.transform.localPosition = new Vector3(0.3f, 0.13f, feetZ);
		leftFoot.transform.localPosition =  new Vector3(-0.3f, 0.13f, feetZ);
	}

    void OnMouseOver()
    {
		hovering = true;
		shadow.GetComponent<SpriteRenderer>().color = selectionColor;
    }
	void OnMouseExit()
    {
		hovering = false;
		shadow.GetComponent<SpriteRenderer>().color = shadowColor;
    }
}
    