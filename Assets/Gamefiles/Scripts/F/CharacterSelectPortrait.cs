using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPortrait : MonoBehaviour
{
    [Required] public CharacterConfig character;
    public CharacterInfoDisplay infoDisplay;
    [Required] public Image image;
    
    void OnEnable() => image.sprite = character.characterPortrait;

    public void SelectNewCharacter() => infoDisplay.ViewCharacter(character);
}
