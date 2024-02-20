using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NamePlate : MonoBehaviour
{
    private TextMeshPro textMesh;


    // Start is called before the first frame update
    void Start()
    {
        textMesh = gameObject.GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        if (UIManager.Instance.openMenu == Menu.Inventory) {
            if (textMesh.text.Equals("") || textMesh.text.Equals("name"))
            textMesh.text = transform.parent.gameObject.name;
        } else {
            textMesh.text = "";
        }
    }
}
