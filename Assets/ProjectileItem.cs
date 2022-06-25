using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileItem : MonoBehaviour
{
    public Projectile projectileSettings;
    // 0 for in group 1 for hostile
    public int faction;
    // mark who shot so they can be picked up by enemies
    public GameObject shooter;

    private bool going = false;
    private Vector3 firingPoint;
    // Start is called before the first frame update
    public void Go()
    {
        transform.position += transform.right * -0.6f;
        going = true;
        firingPoint = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Run();
        Destroy();
    }
    private void Run () {
        if (!going) return;
        transform.position += transform.right * -2.5f * projectileSettings.speed * Time.deltaTime;
    }
    private void Destroy() {
        if (going && Vector3.Distance(firingPoint, transform.position) > projectileSettings.maxDistance) {
            Destroy(this.gameObject);
        }
    }
}
