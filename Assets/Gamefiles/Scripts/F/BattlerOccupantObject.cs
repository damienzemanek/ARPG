using UnityEngine;

public class BattlerOccupantObject : MonoBehaviour
{
    public enum SpriteColorType
    {
        Regular,
        Faded
    }
    
    public SpriteRenderer spriteRenderer;
    
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
}
