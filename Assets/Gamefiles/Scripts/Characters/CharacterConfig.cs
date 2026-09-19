using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ARPG/SO/Character")]
public class CharacterConfig : BattlerConfig
{
    [BoxGroup("Stats")] public int maxAP = 5;
    [BoxGroup("Stats")] public int maxHandSize = 6;
    [BoxGroup("Settings")] [Required] public Sprite portraitArtSelected;
    [BoxGroup("Settings")] [Required] public Sprite portraitArtUnselected;
    [BoxGroup("Settings")] [Required] public Sprite portraitArtLocked;
    [BoxGroup("Settings")] [Required] public Sprite mainArt;
    public string specialEffectName; // Mabye later characters can have multiple sp effects
    // i would have to make the char info display sp effect multi scalable tho
}



