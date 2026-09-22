using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterSelectPortrait;

public class CharacterInfoDisplay : MonoBehaviour
{
    static readonly Type charSaverType = typeof(CharactersData_SavedDataSO);
    
    [Required] public UiOrchestration uiOrchestration;
    [Required] public TextMeshProUGUI txt_Name;
    [Required] public TextMeshProUGUI txt_altName;
    [Required] public TextMeshProUGUI txt_spEffect;
    [Required] public TextMeshProUGUI txt_healthNum;
    [Required] public TextMeshProUGUI txt_apNum;
    [Required] public TextMeshProUGUI txt_dmgNum;
    [Required] public TextMeshProUGUI txt_armorNum;
    [Required] public TextMeshProUGUI txt_buttonLabel;

    [Required] public GameObject lhs_ChoseYourCharacter;
    [Required] public GameObject rhs_characterInfoDisplay;

    [Required] public CharacterConfig initallySelectedStarterCharacter;
    [ReadOnly] public CharacterConfig selectedCharacter;
    [Required] public PlayerEvents playerEvents;
    [Required] public Button selectBtn;

    public List<CharacterSelectPortrait> characterSelectPortraits = new();

    public void Awake()
    {
        selectedCharacter = null;
        selectBtn.interactable = false;
        lhs_ChoseYourCharacter.SetActive(false);
        rhs_characterInfoDisplay.SetActive(false);
    }

    public void ChangePortraits(List<CharacterSelectPortrait> portraits)
    {
        characterSelectPortraits = portraits;
    }
    
    
    #region ---------------------- Chose Starter Character ----------------------

    public void InitChoseStarterCharacter(CharacterConfig defaultCharacter)
    {
        selectedCharacter = initallySelectedStarterCharacter;
        txt_buttonLabel.text = "Select Character";
        SelectACharacter(defaultCharacter);
        lhs_ChoseYourCharacter.SetActive(true);
        rhs_characterInfoDisplay.SetActive(true);
        Debug.Log("Successfully chose starter character");
    }

    public void MainButtonPressed()
    {
        switch (uiOrchestration.currentExplorationUIState)
        {
            case UiOrchestration.ExplorationUIState.FirstStart: SelectACharacter(selectedCharacter); return;
        }

        switch (uiOrchestration.currentCharacterUIState)
        {
            case UiOrchestration.CharacterUIState.None:
                break;
            case UiOrchestration.CharacterUIState.Overview:
                break;
            case UiOrchestration.CharacterUIState.Actions:
                break;
            case UiOrchestration.CharacterUIState.Equipment:
                break;
        }
        
        
    }
    

    public void SelectACharacter(CharacterConfig character)
    {
        // Unselect all portraits
        characterSelectPortraits.ForEach(p =>
        {
            // First start does not have locked portraits
            if(uiOrchestration.currentExplorationUIState == UiOrchestration.ExplorationUIState.FirstStart)
                p.UpdateState(CharacterSelectPortraitState.Unselected);
            else
            {
                SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var data);
                var charData = data.GetCharacterData(character);
                p.UpdateState(charData.hasCharacter ? CharacterSelectPortraitState.Unselected : CharacterSelectPortraitState.Locked);
            }
        });
        
        selectedCharacter = character;
        txt_Name.text = character.occupantName;
        txt_altName.text = character.occupantName;
        txt_spEffect.text = character.specialEffectName;
        txt_healthNum.text = character.maxHP.ToString();
        txt_apNum.text = character.maxAP.ToString();
        txt_dmgNum.text = character.damage.ToString();
        txt_armorNum.text = character.maxArmor.ToString();
        selectBtn.interactable = true;
        Debug.Log("Successfully selected character: " + character.occupantName);
    }
    
    public void ChoseStarterCharacter()
    {
        playerEvents.ReEnablePlayerAndMovement();
        playerEvents.SaveCurrentPosition();
        
        // Save the starter character choice to JSON
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var data);
        if(data == null) Debug.LogError("No data found");
        var charData = data.charactersData.Find(c => c.characterConfigName == selectedCharacter.occupantName);
        if(charData == null) Debug.LogError("No character data found, search name was: " + selectedCharacter.occupantName + "");
        charData.hasCharacter = true;
        saver.Save();
        
    }
    
    #endregion ------------------------------------------------------------------
    
    
}
