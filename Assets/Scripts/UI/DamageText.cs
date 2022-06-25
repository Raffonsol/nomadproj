using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public string textToDisplay = "0";
    public float fadeDuration = 1f;
    private float fadeStartTime;
    private TextMeshPro textMesh;
    private Vector2 inisitalPos;
    private Vector2 finalPos;
    private Color transparent;
    
    // Start is called before the first frame update
    void Start()
    {
        transparent = Color.white;
        transparent.a = 0;
        textMesh = gameObject.GetComponent<TextMeshPro>();
        fadeStartTime = Time.time;
        inisitalPos = transform.position;
        finalPos = inisitalPos;
        finalPos.y += 5f;
    }

    // Update is called once per frame
    void Update()
    {
        textMesh.SetText(textToDisplay);
        // transform.position.moveTowards(Vector2.up);

        float progress = (Time.time-fadeStartTime)/fadeDuration;
        if(progress <= 1){
            //lerp factor is from 0 to 1, so we use (FadeExitTime-Time.time)/fadeDuration
            transform.position = Vector3.Lerp(inisitalPos, finalPos, progress / 5f);
            textMesh.color = Color.Lerp(textMesh.color, transparent, progress /10f );
        }
        else Destroy(gameObject);
    }
}
