using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public float damageMin;
    public float damageMax;
    public float knockBack;
    public bool hitting = false;
    public bool playerParty = false;
    public DamageRsrcType damageRsrcType;
    public ZombieController friendlyOwner;
    public bool recordHits = false;

    // 0 for in group 1 for hostile
    public int faction;
    // Start is called before the first frame update
    public void SetFriendlyOwner(ZombieController owner)
    {
        friendlyOwner = owner;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
