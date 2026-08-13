using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ARPG/SO/Character")]
public class CharacterConfig : BattlerConfig
{
    public int maxAP = 5;
    public int maxHandSize = 6;
    [Required] public Sprite characterSprite;
    [Required] public Sprite characterPortrait;
}



