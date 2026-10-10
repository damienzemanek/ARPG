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

    public int row, col;
    
    [Required] public TeamGrid teamGrid;
    [Required] public Image image;
    
    [Required] public Sprite spr_contested;
    [Required] public Sprite spr_none;

    void Awake()
    {
        if(contested) state = CharacterSelectPortraitSquareState.Contested;
    }

    public void InitFromTeamGrid() => UpdateSquareState(CharacterSelectPortraitSquareState.None);

    public void UpdateSquareState(CharacterSelectPortraitSquareState _state)
    {
        if (contested) {
            image.sprite = spr_contested;
            return; }
        
        state = _state;
        switch (state)
        {
            case CharacterSelectPortraitSquareState.None:
                currentlyEquippedPortrait = null;
                image.sprite = spr_none;
                return;
            
            
            case CharacterSelectPortraitSquareState.Unselected:
                image.sprite = currentlyEquippedPortrait.characterCfg.portraitArtUnselected; break;
            case CharacterSelectPortraitSquareState.Selected:
                image.sprite = currentlyEquippedPortrait.characterCfg.portraitArtSelected; break;
            default: Debug.LogError("Invalid Character Select Portrait Square State: " + state); break;
        }
        
        
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var charSaver, out var charData);
        var matchingCharacterData = charData.charactersData.Find(c => c.characterConfigName == currentlyEquippedPortrait.characterCfg.occupantName);
        if (matchingCharacterData == null) return;
        matchingCharacterData.row = row;
        matchingCharacterData.col = col;
        charSaver.Save();
    }

    public void SelectSquare()
    {
        if (contested) return;

        // TODO: Swapping when clicked on another occupied, this just makes it idempotent (tech debt)
        if (teamGrid.currentOperation == TeamGrid.CurrentOperation.MovingOrDequipping &&
            currentlyEquippedPortrait != null)
            return;
        
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
            if (currentlyEquippedPortrait == null) return;
            teamGrid.InteractWithCharacterSelectPortraitSquare(this);
            return;
        }
        
        if (teamGrid.currentOperation == TeamGrid.CurrentOperation.Equipping &&
            teamGrid.currentlyEquippingPortrait != null)
        {
            currentlyEquippedPortrait = teamGrid.currentlyEquippingPortrait;
            UpdateSquareState(CharacterSelectPortraitSquareState.Selected);
            teamGrid.EquipCharacter(this);
        }
    }
}