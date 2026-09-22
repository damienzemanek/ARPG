using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPortrait : MonoBehaviour
{
    public enum PortraitType
    {
        None,
        InfoDisplay,
        TeamGrid,
    }
    
    public enum CharacterSelectPortraitState
    {
        None,
        Locked,
        Unselected,
        Selected,
    }

    public bool useUnlockData = true;
    public CharacterSelectPortraitState state;
    [Required] public CharacterConfig character;
    [Required] public Image image;
    
    public PortraitType portraitType;
    public CharacterInfoDisplay infoDisplay;
    public TeamGrid teamGrid;

    void OnEnable()
    {
        RegisterPortraits();
        UpdateState();
    }

    void OnDisable()
    {
        UnRegisterPortraits();
    }

    public void RegisterPortraits() => infoDisplay.characterSelectPortraits.Add(this);
    public void UnRegisterPortraits() => infoDisplay.characterSelectPortraits.Remove(this);

    public void UpdateState(CharacterSelectPortraitState optionalToState = CharacterSelectPortraitState.None)
    {
        if(optionalToState != CharacterSelectPortraitState.None) state = optionalToState;
        else if (useUnlockData) // is this is false, the portrait states are manual via inspector
        {
            SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out _, out var data);
            var charData = data.GetCharacterData(character);
            state = charData.hasCharacter 
                ? CharacterSelectPortraitState.Unselected 
                : CharacterSelectPortraitState.Locked;
        }
        
        switch (state)
        {
            case CharacterSelectPortraitState.Locked:
                image.sprite = character.portraitArtLocked;
                break;
            case CharacterSelectPortraitState.Unselected:
                image.sprite = character.portraitArtUnselected;
                break;
            case CharacterSelectPortraitState.Selected:
                image.sprite = character.portraitArtSelected;
                break;
            default: Debug.LogError("Invalid Character Select Portrait State: " + state); break;
        }
    }

    public void SelectNewCharacter()
    {
        Debug.Log("CharacterSelectPortrait " + name + " selected");
        infoDisplay.SelectACharacter(character);
        UpdateState(CharacterSelectPortraitState.Selected);

    }
}
