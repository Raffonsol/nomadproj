using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public int x;
    public int y;

    public bool hasRoadOrRiver;

    public int contentLimit = 5;
    public int contentCurrent = 0;

    private float disappearDistance = 0;
    private float checkTimer = 3f;
    // Start is called before the first frame update
    void Start()
    {
        if (disappearDistance == 0)disappearDistance = BerkeleyManager.Instance.disappearDistance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, Camera.main.transform.position) > disappearDistance) {
            MapMaker.Instance.DeleteTileAt(x, y);
            Destroy(gameObject);
        }
    }
}
