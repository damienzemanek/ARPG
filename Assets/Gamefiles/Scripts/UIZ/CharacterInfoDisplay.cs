using System;
using System.Collections.Generic;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterSelectPortrait;
using static UiOrchestration;

public class CharacterInfoDisplay : MonoBehaviour
{
    static readonly Type charSaverType = typeof(CharactersData_SavedDataSO);
    
    [Required] public UiOrchestration uiOrchestration;
    [Required] public MenuBar menuBarDetails;
    [Required] public TextMeshProUGUI txt_Name;
    [Required] public TextMeshProUGUI txt_altName;
    [Required] public TextMeshProUGUI txt_spEffect;
    [Required] public TextMeshProUGUI txt_healthNum;
    [Required] public TextMeshProUGUI txt_apNum;
    [Required] public TextMeshProUGUI txt_dmgNum;
    [Required] public TextMeshProUGUI txt_armorNum;
    [Required] public TextMeshProUGUI txt_buttonLabel;
    [Required] public Image img_mainArt;

    [Required] public GameObject lhs_ChoseYourCharacter;
    [Required] public GameObject rhs_characterInfoDisplay;

    [Required] public CharacterSelectPortrait initiallySelectedPortrait;
    [ReadOnly] public CharacterConfig selectedCharacter;
    [Required] public PlayerEvents playerEvents;
    [Required] public Button selectBtn;
    [Required] public TeamGrid teamGrid;

    [Required] public Animator characterInfoPanelAnimator;
    public string openCharInfoPanelAnimName = "Open";
    public string closeCharInfoPanelAnimName = "Close";
    
    [Required] public Animator mainArtAnimator;
    public string toDefaultMainArtAnimName = "ToDefault";
    public string toActionsOrEquipmentAnimName = "ActionsOrEquipment";
    public string defaultActionsOrEquipmentAnimName = "ActionsOrEquipmentDefault";


    [Required] public TextMeshProUGUI txt_descTactics;
    [Required] public TextMeshProUGUI txt_descAbout;


    [ReadOnly] public List<CharacterSelectPortrait> characterSelectPortraits = new();
    public List<CharacterSelectPortrait> glgCharacterSelectPortraits = new();

    public void Awake()
    {
        selectedCharacter = null;
        lhs_ChoseYourCharacter.SetActive(false);
        rhs_characterInfoDisplay.SetActive(false);
    }
    
    
    #region ---------------------- Chose Starter Character ----------------------

    public void ViewChoseStarterCharacterScreen()
    {
        selectBtn.interactable = false;
        selectedCharacter = initiallySelectedPortrait.characterCfg;
        txt_buttonLabel.text = "Select Character";
        uiOrchestration.currentExplorationUIState = ExplorationUIState.FirstStart;
        PreviewCharacter(initiallySelectedPortrait.characterCfg);
        lhs_ChoseYourCharacter.SetActive(true);
        rhs_characterInfoDisplay.SetActive(true);
        Debug.Log("Successfully chose starter character");
    }

    public void ViewCharacterScreen()
    {
        teamGrid.ResetState();
        characterSelectPortraits.Clear();
        characterSelectPortraits.AddRange(glgCharacterSelectPortraits);
        txt_buttonLabel.text = "DETAILS";
        characterSelectPortraits.ForEach(p =>
        {
            p.portraitType = PortraitType.InfoDisplay;
            p.UpdateState();
        });
        characterSelectPortraits[0].SelectNewCharacter();
    }
    
    public void ViewManageTeamScreen()
    {
        characterSelectPortraits.Clear();
        characterSelectPortraits.AddRange(glgCharacterSelectPortraits);
        characterSelectPortraits.ForEach(p =>
        {
            p.portraitType = PortraitType.TeamGrid;
            p.UpdateState();
        });
    }

    public void ResetDetailsMenus()
    {
        if (onActionsOrEquipmentScreen)
            mainArtAnimator.Play(toDefaultMainArtAnimName);

        
        onCharacterScreen = false;
        onActionsOrEquipmentScreen = false;
    }
    
    public bool onCharacterScreen;
    public bool onActionsOrEquipmentScreen;

    public void ViewOverviewScreen()
    {
        Debug.Log("OVERVIEW SCREEN");

        // If already had started to inspect this character
        if (onActionsOrEquipmentScreen)
            mainArtAnimator.Play(toDefaultMainArtAnimName);
        
        onActionsOrEquipmentScreen = false;

        if (onCharacterScreen) // already on
            return;
        
        
        // if not, play the animation (Newly inspected character)
        txt_descTactics.text = selectedCharacter.desc_tactics;
        txt_descAbout.text = selectedCharacter.desc_about;
        onCharacterScreen = true;
        characterInfoPanelAnimator.gameObject.Get<RectTransform>().RefreshLayoutGroupsImmediateAndRecursive();
    }
    

    
    public void ViewActionsOrEquipmentScreen()
    {
        Debug.Log("ACTIONS OR EQUIPMENT SCREEN");
        
        if (onCharacterScreen)
            mainArtAnimator.Play(toActionsOrEquipmentAnimName);
        
        onCharacterScreen = false;
        
        if (onActionsOrEquipmentScreen) // already on
            return;
        
        
        onActionsOrEquipmentScreen = true;
    }

    public void MainButtonPressed()
    {
        switch (uiOrchestration.currentMainUIState)
        {
            case MainUIState.Characters:
                ResetDetailsMenus();
                menuBarDetails.SelectOption("Overview");
                return;
        }
        
        switch (uiOrchestration.currentExplorationUIState)
        {
            case ExplorationUIState.FirstStart: ChoseStarterCharacter(); return;
        }
        
    }
    

    /// <summary>
    /// Call p.UpdateState(CharacterSelectPortraitState.Selected) Afterword
    /// </summary>
    /// <param name="character"></param>
    public void PreviewCharacter(CharacterConfig character)
    {
        // Unselect all portraits
        characterSelectPortraits.ForEach(p =>
        {
            // First start does not have locked portraits
            if(uiOrchestration.currentExplorationUIState == ExplorationUIState.FirstStart)
                p.UpdateState(CharacterSelectPortraitState.Selected);
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
        img_mainArt.sprite = character.mainArt;
        selectBtn.interactable = true;
        Debug.Log("Successfully selected character: " + character.occupantName);
    }
    
    public void ChoseStarterCharacter()
    {
        Debug.Log("CHARACTER WAS CHOSEN");
        playerEvents.ReEnablePlayerAndMovement();
        playerEvents.SaveCurrentPosition();
        uiOrchestration.ShowExplorationUIState(ExplorationUIState.Exploration);
        
        // Save the starter character choice to JSON
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var charSaver, out var data);
        if(data == null) Debug.LogError("No data found");
        var charData = data.charactersData.Find(c => c.characterConfigName == selectedCharacter.occupantName);
        if(charData == null) Debug.LogError("No character data found, search name was: " + selectedCharacter.occupantName + "");
        charData.hasCharacter = true;
        charData.row = 1;
        charData.col = 1;
        charData.equipped = true;
        charSaver.Save();
    }

    public void OpenCharacterInfoPanel()
    {
        characterInfoPanelAnimator.Play(openCharInfoPanelAnimName);
    }

    public void CloseCharacterInfoPanel()
    {
        characterInfoPanelAnimator.Play(closeCharInfoPanelAnimName);
    }

}
    
#endregion ------------------------------------------------------------------
    
    

