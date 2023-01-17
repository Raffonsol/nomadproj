using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class BonusChooser : MonoBehaviour
{
    private Image image;
    private float lvlUpCheckTimer = 2f;
    private float imageTimer = 2f;

    private bool showing = false;
    private bool awaitingChoice = false;
    List<Bonus> lib;
    // Start is called before the first frame update
    void Start()
    {
        image = transform.Find("mask").gameObject.GetComponent<Image>();
        transform.Find("mask").gameObject.SetActive(false);
        lib = GameLib.Instance.allBonuses.Cast<Bonus>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!showing) {
            // check if anyone lvlled up and show myself
            if (lvlUpCheckTimer > 0) {
                lvlUpCheckTimer -=Time.deltaTime;
            } else {
                lvlUpCheckTimer = 1.2f;
                if (UIManager.Instance.lvlUpQueue.Count > 0) {
                    Show();
                }
            }
            return;
        };
        // if showing this happens

        // blink effect
        if (imageTimer > 0) {
            imageTimer -=Time.deltaTime;
        } else {
            imageTimer = 2f;
            image.enabled = !image.enabled;
        }
    }
    void Show() {
        if (showing) return;
        showing = true;
        transform.Find("mask").gameObject.SetActive(true);
        awaitingChoice = true;
        
        // find top of lvl up que
        List<int> queue = UIManager.Instance.lvlUpQueue; 
        FriendlyChar upee = Player.Instance.GetCharById(queue[0]);

        // determine pool to select from
        List<Bonus> pool = new List<Bonus>();
        pool.AddRange(lib);

        // remove already owned bilities
        for(int i = 0; i <upee.bonuses.Count; i++){
            pool.Remove(upee.bonuses[i]);
        }
        

        // find 3 bonus options
        int opt1 = -1;
        int opt2 = -1; 
        int opt3 = -1;
        while (opt1 == -1) {
            opt1 = UnityEngine.Random.Range(0,pool.Count);
            if (pool[opt1].minLvl > upee.level) opt1 = -1;
        }
        while (opt2 == -1 || opt2 == opt1) {
            opt2 = UnityEngine.Random.Range(0,pool.Count);
            if (pool[opt2].minLvl > upee.level) opt2 = -1;
        }
        while (opt3 == -1 || opt3 == opt1 || opt3 == opt2) {
            opt3 = UnityEngine.Random.Range(0,pool.Count);
            if (pool[opt3].minLvl > upee.level) opt3 = -1;
        }
        // populate UI
        Transform panel1 = transform.Find("mask/BonusOption1");
        panel1.Find("name").GetComponent<TextMeshProUGUI>().text = pool[opt1].name;
        panel1.Find("description").GetComponent<TextMeshProUGUI>().text = pool[opt1].description;
        panel1.Find("icon").GetComponent<Image>().sprite = pool[opt1].icon;
        panel1.GetComponent<Button>().onClick.AddListener(() => ChooseOption(opt1, 1));

        Transform panel2 = transform.Find("mask/BonusOption2");
        panel2.Find("name").GetComponent<TextMeshProUGUI>().text = pool[opt2].name;
        panel2.Find("description").GetComponent<TextMeshProUGUI>().text = pool[opt2].description;
        panel2.Find("icon").GetComponent<Image>().sprite = pool[opt2].icon;
        panel2.GetComponent<Button>().onClick.AddListener(() => ChooseOption(opt2, 2));

        Transform panel3 = transform.Find("mask/BonusOption3");
        panel3.Find("name").GetComponent<TextMeshProUGUI>().text = pool[opt3].name;
        panel3.Find("description").GetComponent<TextMeshProUGUI>().text = pool[opt3].description;
        panel3.Find("icon").GetComponent<Image>().sprite = pool[opt3].icon;
        panel3.GetComponent<Button>().onClick.AddListener(() => ChooseOption(opt3, 3));
    }
    void ChooseOption( int bonus, int option) {

        if (!awaitingChoice) return;
        awaitingChoice = false;
        showing = false;

        // find top of lvl up que
        List<int> queue = UIManager.Instance.lvlUpQueue; 
        Player.Instance.ApplyBonus(queue[0], lib[bonus]);
        queue.RemoveAt(0);
        UIManager.Instance.lvlUpQueue = queue;

        transform.Find("mask").gameObject.SetActive(false);
        // remove listeners
        transform.Find("mask/BonusOption1").GetComponent<Button>().onClick.RemoveAllListeners();
        transform.Find("mask/BonusOption2").GetComponent<Button>().onClick.RemoveAllListeners();
        transform.Find("mask/BonusOption3").GetComponent<Button>().onClick.RemoveAllListeners();
    }
}

