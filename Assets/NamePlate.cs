using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NamePlate : MonoBehaviour
{
    private TextMeshPro textMesh;

    private List<string> lineQueue;
    public float chatDuration = 2f;
    private float chatTimer = 2f;
    private float chatCooldown = 0f;

    // Start is called before the first frame update
    void Awake()
    {
        textMesh = gameObject.GetComponent<TextMeshPro>();
        lineQueue = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        if (UIManager.Instance.openMenu == Menu.Inventory) {
            if (textMesh.text.Equals("") || textMesh.text.Equals("---"))
            textMesh.text = transform.parent.gameObject.name;
        } else {

            if (chatCooldown > 0) {
                chatCooldown -= Time.deltaTime;
            } else if (lineQueue.Count > 0) {
                textMesh.text = lineQueue[0];
                chatTimer = chatDuration;
                Debug.Log("showing line "+lineQueue[0]);
                lineQueue.RemoveAt(0);
            }
            
            if (chatTimer > 0) {
                chatTimer -= Time.deltaTime;
                if (chatTimer <= 0) {
                    textMesh.text = "";
                    Debug.Log("ending line");
                    chatCooldown = chatDuration;
                }
            } else if (!textMesh.text.Equals("")) {
                textMesh.text = "";
            }
        }
    }

    public void LineUpLine(string line) {
        lineQueue.Add(line);

    }
}
