using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GrassScript : MonoBehaviour
{
    public int x;
    public int y;

    private float tileSize = 0;

    private float disappearDistance = 0;
    private float checkTimer = 2f;

    bool started = false;
    // Start is called before the first frame update
    void Start()
    {
        if (disappearDistance == 0)disappearDistance = BerkeleyManager.Instance.disappearDistance;
        if (tileSize == 0)tileSize = MapMaker.Instance.grassLength;
        started = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!started) return;
        // if (checkTimer > 0) checkTimer -= Time.deltaTime;
        // else {

            int playerX = (int)Math.Round((Camera.main.transform.position.x)/tileSize);
            int playerY = (int)Math.Round((Camera.main.transform.position.y)/tileSize);
            // Debug.Log(playerX+", "+playerY);
            if (playerX > x+1) {
                float newX = (x+3)*tileSize;
                transform.position = new Vector2(newX, transform.position.y);
                x+=3;
            }
            if (playerY > y+1) {
                float newY = (y+3)*tileSize;
                transform.position = new Vector2(transform.position.x, newY);
                y+=3;
            }
            // playerX = (int)Math.Round((Camera.main.transform.position.x-0.5f)/tileSize);
            // playerY = (int)Math.Round((Camera.main.transform.position.y-0.5f)/tileSize);
            
            if (playerY < y-1) {
                float newY = (y-3)*tileSize;
                transform.position = new Vector2(transform.position.x, newY);
                y-=3;
            }
            if (playerX < x-1) {
                float newX = (x-3)*tileSize;
                transform.position = new Vector2(newX, transform.position.y);
                x-=3;
            }
        //     checkTimer = 01f;
        // }
    }
}
