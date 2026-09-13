using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ARPG/SO/Character")]
public class CharacterConfig : BattlerConfig
{
    [BoxGroup("Stats")] public int maxAP = 5;
    [BoxGroup("Stats")] public int maxHandSize = 6;
    [BoxGroup("Settings")] [Required] public Sprite characterSprite;
    [BoxGroup("Settings")] [Required] public Sprite characterPortrait;
}



