using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GeneratePlacer
{
    N,
    E,
    NE,
}
public class MapGenWaiter : MonoBehaviour
{
    public GeneratePlacer waitFor;

    private Transform cameraTransform;

    private float checkTime = 2f;
    private float checkTimer;
    private bool runMap = false;
    // Start is called before the first frame update
    void Start()
    {
        checkTimer = checkTime;
        cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (runMap) {
            runMap = false;
            StartCoroutine(OneUpdate());
            return;
        }
        checkTimer -= Time.deltaTime;
        if (checkTimer < 0) {
            checkTimer = checkTime;
            runMap = true;
        }
       
    }
    IEnumerator OneUpdate(){
 
        // if (waitFor == GeneratePlacer.N) {

        //     if (transform.position.y - cameraTransform.position.y < 10f) {
        //         MapManager.Instance.Map(transform.position.x, transform.position.y);
        //         Destroy(gameObject);
        //     }
        // }
        // if (waitFor == GeneratePlacer.E) {
        //     if (transform.position.x - cameraTransform.position.x < 10f) {
        //         MapManager.Instance.Map(transform.position.x, transform.position.y);
        //         Destroy(gameObject);
        //     }
        // }
        // if (waitFor == GeneratePlacer.NE) {
            // Debug.Log(Vector3.Distance(transform.position, cameraTransform.position));
            if (Vector3.Distance(transform.position, cameraTransform.position) < 30f) {
                MapManager.Instance.Map(transform.position.x, transform.position.y);
                Destroy(gameObject);
            }
        // }
        yield return null;
 
    }
}
