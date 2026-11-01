using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BattlemodeActionVisu : MonoBehaviour
{
    [Required] public Image img;
    [Required] public Sprite defaultSprite;
    [Required] public Sprite hoverSprite;
    
    public void Hover()
    {
        img.sprite = hoverSprite;
    }

    public void Default()
    {
        img.sprite = defaultSprite;
    }
    
}
