using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPortrait : MonoBehaviour
{
    [Required] public CharacterConfig character;
    public CharacterInfoDisplay infoDisplay;
    [Required] public Image image;
    
    void OnEnable() => image.sprite = character.mainArt;

    public void SelectNewCharacter() => infoDisplay.SelectACharacter(character);
}
