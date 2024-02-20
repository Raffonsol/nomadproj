using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public float tutorialStartTime = 5f;
    public string[] panels;
    private int panelsShown = 0;
    private GameObject mask;

    // Start is called before the first frame update
    void Start()
    {
        mask = transform.Find("mask").gameObject;
        mask.SetActive(false);
    }
    void DoStart()
    {
        
        if (GameOverlord.Instance.progress > 0) {
            Destroy(gameObject);
        }
        mask.SetActive(true);
        tutorialStartTime = 5f;
        panelsShown++;
    }
    void Step2() {
        mask.SetActive(true);
        tutorialStartTime = 5f;
        mask.transform.Find("learn").GetComponent<TextMeshProUGUI>().text = panels[1];
        // UIManager.Instance.ShowTab(Tab.Weapons);


        // For now this is the last tutorial so mark the first progress done
        GameOverlord.Instance.progress = 1;
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // closing menu
        if (tutorialStartTime < 1f && Input.GetButtonDown("Fire1")) {
            mask.SetActive(false);
        } else if (tutorialStartTime > 0) {
            tutorialStartTime -= Time.deltaTime;
        }

        if (panelsShown > 0) {
            // move to second tuorial
            if (Player.Instance.parts.Count > 0 && panelsShown == 1) {
                panelsShown++;
                Step2();
            }
            return;
        }

        // starting the tutorial
        if (tutorialStartTime > 0) {
            tutorialStartTime -= Time.deltaTime;
        } else {
            tutorialStartTime = 20f;
           DoStart();
        }
        
    }
}
