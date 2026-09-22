using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPortraitSquare : MonoBehaviour
{
    public enum CharacterSelectPortraitSquareState
    {
        None,
        Contested,
        Unselected,
        Selected,
    }

    [ReadOnly] public CharacterSelectPortraitSquareState state;
    [ReadOnly] public CharacterConfig character;

    [Required] public TeamGrid teamGrid;
    [Required] public Image image;
    
    [Required] public Sprite spr_contested;
    [Required] public Sprite spr_none;
    
    
    public void UpdateState(CharacterSelectPortraitSquareState optionalToState)
    {
        state = optionalToState;
        switch (state)
        {
            case CharacterSelectPortraitSquareState.Contested: 
                image.sprite = spr_contested; break;
            case CharacterSelectPortraitSquareState.None:
                character = null;
                image.sprite = spr_none; break;
            case CharacterSelectPortraitSquareState.Unselected:
                image.sprite = character.portraitArtUnselected; break;
            case CharacterSelectPortraitSquareState.Selected:
                image.sprite = character.portraitArtSelected; break;
            default: Debug.LogError("Invalid Character Select Portrait Square State: " + state); break;
        }
    }

    public void SelectSquare()
    {
        if (teamGrid.currentOperation == TeamGrid.CurrentOperation.Equipping &&
            teamGrid.currentEquippingCharacter != null)
        {
            character = teamGrid.currentEquippingCharacter;
            UpdateState(CharacterSelectPortraitSquareState.Selected);
        }
    }
}