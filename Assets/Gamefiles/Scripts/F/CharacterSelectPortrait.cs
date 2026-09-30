using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
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
        Equipped,
        Empty
    }

    public bool useUnlockData = true;
    public CharacterSelectPortraitState state;
    [FormerlySerializedAs("character")] public CharacterConfig characterCfg;
    [Required] public Image image;
    
    public PortraitType portraitType;
    public CharacterInfoDisplay infoDisplay;
    public TeamGrid teamGrid;
    [Required] public GameObject equippedOverlay;
    [Required] public GameObject lockedOverlay;


    void OnEnable()
    {
        InfoDisplayRegisterPortraits();
        if (characterCfg == null) UpdateState(CharacterSelectPortraitState.Empty);
        else UpdateState();
    }

    void OnDisable()
    {
        InfoDisplayUnRegisterPortraits();
    }

    public void InfoDisplayRegisterPortraits()
    {
        if(portraitType == PortraitType.InfoDisplay) infoDisplay.characterSelectPortraits.Add(this);
    }
    
    public void InfoDisplayUnRegisterPortraits()
    {
        if(portraitType == PortraitType.InfoDisplay) infoDisplay.characterSelectPortraits.Remove(this);
    }

    public void UpdateState(CharacterSelectPortraitState optionalToState = CharacterSelectPortraitState.None)
    {
        if(optionalToState != CharacterSelectPortraitState.None) state = optionalToState;
        else if (useUnlockData) // is this is false, the portrait states are manual via inspector
        {
            SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out _, out var data);
            var charData = data.GetCharacterData(characterCfg);
            state = charData.hasCharacter 
                ? CharacterSelectPortraitState.Unselected 
                : CharacterSelectPortraitState.Locked;

            if (portraitType == PortraitType.TeamGrid)
            {
                Debug.Log($"{gameObject.name}: Is Team Grid");
                var equipped = data.charactersData.Find(c => c.characterConfigName == characterCfg.occupantName);
                if (equipped != null)
                {
                    Debug.Log($"{gameObject.name}: Found a match: {equipped.characterConfigName}");
                    if (equipped.equipped)
                    {
                        Debug.Log($"{gameObject.name}: Is equipped");
                        state = CharacterSelectPortraitState.Equipped;
                    }
                }
            }
        }
        
        switch (state)
        {
            case CharacterSelectPortraitState.Locked:
                image.sprite = characterCfg.portraitArtUnselected;
                lockedOverlay.SetActive(true);
                equippedOverlay.SetActive(false);
                break;
            case CharacterSelectPortraitState.Unselected:
                image.sprite = characterCfg.portraitArtUnselected;
                lockedOverlay.SetActive(false);
                equippedOverlay.SetActive(false);
                break;
            case CharacterSelectPortraitState.Selected:
                image.sprite = characterCfg.portraitArtSelected;
                lockedOverlay.SetActive(false);
                equippedOverlay.SetActive(false);
                break;
            case CharacterSelectPortraitState.Equipped:
                image.sprite = characterCfg.portraitArtUnselected;
                lockedOverlay.SetActive(false);
                equippedOverlay.SetActive(true);
                break;
            case CharacterSelectPortraitState.Empty:
                image.sprite = null;
                lockedOverlay.SetActive(false);
                equippedOverlay.SetActive(false);
                break;
        }
        Debug.Log("Char Portrait: State is now: " + state);
    }
    

    public void SelectNewCharacter()
    {
        Debug.Log("CharacterSelectPortrait " + name + " selected");
        infoDisplay.PreviewCharacter(characterCfg);
        UpdateState(CharacterSelectPortraitState.Selected);
    }

    public void TeamUX_ClickOnCharacter()
    {
        if(state == CharacterSelectPortraitState.Unselected)
            teamGrid.InteractWithCharacterSelectPortrait(this);
    }
}
