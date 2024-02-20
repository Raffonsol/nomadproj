using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LifeMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float disableOn;
    private Player player;
    private Image image;

    private Color fullColor;
    private Color emptyColor;
    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
        image = transform.GetComponent<Image>();
        fullColor = image.color;
        emptyColor = image.color;
        emptyColor.a=0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.activePerson.life / player.activePerson.stats[0].value >= disableOn) {
            image.color = fullColor;
        } else {
            image.color = emptyColor;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        
        UIManager.Instance.ShowSimpleToolTip(new Vector2(transform.position.x*1.03f,transform.position.y*0.93f),player.activePerson.life.ToString("0") +" / "+ player.activePerson.stats[0].value);
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideToolTips();
    }
}
