using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileItem : MonoBehaviour
{
    public Projectile projectileSettings;
    // 0 for in group 1 for hostile
    public int faction;
    public int consumableId;
    // mark who shot so they can be picked up by enemies
    public GameObject shooter;
    public bool playerParty = false;

    private bool going = false;
    private Vector3 firingPoint;
    private float lifeTime;
    // Start is called before the first frame update
    public void Go()
    {
        transform.position += transform.right * -0.6f;
        firingPoint = transform.position;
        lifeTime = projectileSettings.maxLife;
        going = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!going) return;
        Run();
       Destroy();
        if (lifeTime > 0) {
            lifeTime -= Time.deltaTime;
        } else {
            Destroy(this.gameObject);
        }
    }
    private void Run () {
        transform.position += transform.right * -2.5f * projectileSettings.speed * Time.deltaTime;
    }
    private void Destroy() {
        if (going && Vector3.Distance(firingPoint, transform.position) > projectileSettings.maxDistance) {
            Destroy(this.gameObject);
        }
    }
}
