using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDeath : MonoBehaviour
{
    public List<Sprite> steps;
    public float timePerStep = 0.4f;
    private float stepTimer;
    private int listStep = 0;
    // Start is called before the first frame update
    void Start()
    {
        stepTimer = timePerStep;
    }

    // Update is called once per frame
    void Update()
    {
        stepTimer -= Time.deltaTime;
        if (stepTimer < 0) {
            stepTimer = timePerStep;
            listStep++;
            try {
                gameObject.GetComponent<SpriteRenderer>().sprite = steps[listStep];
            }
            catch { // Oh my god how lazy was I when I made this
                Destroy(gameObject);
            }
        }
    }
}
