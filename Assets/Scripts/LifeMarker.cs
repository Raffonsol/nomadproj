using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeMarker : MonoBehaviour
{
    public float disableOn;
    private Player player;
    private Image image;
    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
        image = transform.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.life / player.maxLife >= disableOn) {
            image.enabled = true;
        } else {
            image.enabled = false;
        }
    }
}
