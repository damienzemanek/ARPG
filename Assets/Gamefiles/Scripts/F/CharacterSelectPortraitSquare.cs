using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPortraitSquare : MonoBehaviour
{
    public bool contested;
    
    public enum CharacterSelectPortraitSquareState
    {
        None,
        Contested,
        Unselected,
        Selected,
    }

    [ReadOnly] public CharacterSelectPortraitSquareState state;
    [ReadOnly] public CharacterSelectPortrait currentlyEquippedPortrait;

    [Required] public TeamGrid teamGrid;
    [Required] public Image image;
    
    [Required] public Sprite spr_contested;
    [Required] public Sprite spr_none;

    void Awake()
    {
        if(contested) state = CharacterSelectPortraitSquareState.Contested;
    }

    void OnEnable()
    {
        UpdateSquareState(CharacterSelectPortraitSquareState.None);
    }

    public void UpdateSquareState(CharacterSelectPortraitSquareState optionalToState)
    {
        if (contested) {
            image.sprite = spr_contested;
            return; }
        
        state = optionalToState;
        switch (state)
        {
            case CharacterSelectPortraitSquareState.None:
                currentlyEquippedPortrait = null;
                image.sprite = spr_none; break;
            case CharacterSelectPortraitSquareState.Unselected:
                image.sprite = currentlyEquippedPortrait.character.portraitArtUnselected; break;
            case CharacterSelectPortraitSquareState.Selected:
                image.sprite = currentlyEquippedPortrait.character.portraitArtSelected; break;
            default: Debug.LogError("Invalid Character Select Portrait Square State: " + state); break;
        }
    }

    public void SelectSquare()
    {
        if (contested) return;
        
        if (teamGrid.currentOperation == TeamGrid.CurrentOperation.MovingOrDequipping
            && teamGrid.currentlySelectedSquare != this)
        {
            currentlyEquippedPortrait = teamGrid.currentlySelectedSquare.currentlyEquippedPortrait;
            UpdateSquareState(CharacterSelectPortraitSquareState.Unselected);
            teamGrid.currentlySelectedSquare.UpdateSquareState(CharacterSelectPortraitSquareState.None);
            teamGrid.currentlySelectedSquare = null;
            teamGrid.ResetState();
            return;
        }
        
        if (currentlyEquippedPortrait != null &&
            teamGrid.currentOperation == TeamGrid.CurrentOperation.MovingOrDequipping)
        {
            if(this == teamGrid.currentlySelectedSquare) teamGrid.ResetState();
            return;
        }
        
        if (teamGrid.currentOperation == TeamGrid.CurrentOperation.None)
        {
            if (currentlyEquippedPortrait == null)
                return;
            teamGrid.InteractWithCharacterSelectPortraitSquare(this);
            return;
        }
        
        if (teamGrid.currentOperation == TeamGrid.CurrentOperation.Equipping &&
            teamGrid.currentlyEquippingPortrait != null)
        {
            currentlyEquippedPortrait = teamGrid.currentlyEquippingPortrait;
            UpdateSquareState(CharacterSelectPortraitSquareState.Selected);
            teamGrid.EquipCharacter();
        }
    }
}