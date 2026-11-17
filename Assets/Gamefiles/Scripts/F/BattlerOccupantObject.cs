using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using UnityEngine;

public class BattlerOccupantObject : MonoBehaviour
{
    public enum SpriteColorType
    {
        Regular,
        Faded
    }
    
    public SpriteRenderer spriteRenderer;
    [DrawWithUnity] public SerializedDictionary<Sprite, SpriteRenderer> actionSprites = new();
    
    public Color regularColor;
    public Color fadedColor;

    public void SetSpriteColor(SpriteColorType spriteColorType)
    {
        Debug.Log("[BattlerOccupantObject] Setting sprite color to: " + spriteColorType + "");
        switch (spriteColorType)
        {
            case SpriteColorType.Regular: spriteRenderer.color = regularColor; break;
            case SpriteColorType.Faded: spriteRenderer.color = fadedColor; break;
        }
    }

    public void SetAllActionSpritesToFalse()
    {
        foreach (var pair in actionSprites)
            pair.Value.gameObject.SetActive(false);
    }
}
