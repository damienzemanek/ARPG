using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using EMILtools;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterSelectPortrait;
using static UiOrchestration;

public class CharacterInfoDisplay : MonoBehaviour
{
    const string k_LevelFormat = "Lvl {0}/60";
    static readonly Type charSaverType = typeof(CharactersData_SavedDataSO);
    
    [BoxGroup("Dependencies")] [Required] public UiOrchestration uiOrchestration;
    [BoxGroup("Dependencies")] [Required] public MenuBar menuBarDetails;
    [BoxGroup("Dependencies")] [Required] public ActionsMenuActionsDisplay menuActionsDisplay;
    
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_Name;
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_lvl;
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_altName;
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_spEffect;
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_healthNum;
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_apNum;
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_dmgNum;
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_armorNum;
    [BoxGroup("Character Info")] [Required] public TextMeshProUGUI txt_buttonLabel;
    [BoxGroup("Character Info")] [Required] public Image img_mainArt;

    [Required] public CharacterSelectPortrait initiallySelectedPortrait;
    [ReadOnly] public CharacterConfig selectedCharacter;
    [Required] public PlayerEvents playerEvents;
    [Required] public Button selectBtn;
    [Required] public TeamGrid teamGrid;
    public Color mainBtnTxtColorRegular = new Color(1, 1, 1, 1);
    public Color mainBtnTxtColorDisabled = new Color(0.5f, 0.5f, 0.5f, 1);

    [BoxGroup("Character Info Panel")] [Required] public Transform characterInfoPanelTransform;
    [BoxGroup("Character Info Panel")] [Required] public Transform openCharInfoPanelPos;
    [BoxGroup("Character Info Panel")] [Required] public Transform closeCharInfoPanelPos;
    
    [BoxGroup("Main Art")] [Required] public Transform mainArtTransform;
    [BoxGroup("Main Art")] public float mainArtMoveDuration = 0.3f;
    [BoxGroup("Main Art")] [Required] public Transform overviewMainArtPos;
    [BoxGroup("Main Art")] [Required] public Transform actionsOrEquipmentMainArtPos;
    [BoxGroup("Main Art")] [Required] public Transform characterSelectMainArtPos;
    [BoxGroup("Main Art")] [Required] public Transform upgradeLevelMainArtPos;


    [BoxGroup("Upgrade Screen")] [Required] public Transform hlg_materialsBtnParent;
    [BoxGroup("Upgrade Screen")] [Required] public GameObject pr_materialBtn;
    [BoxGroup("Upgrade Screen")] [Required] public Button btn_autoAdd;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_autoAdd;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_currLevl;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_nxtLevel;
    [BoxGroup("Upgrade Screen")] [Required] public GameObject go_lvlArrowDisp;
    [BoxGroup("Upgrade Screen")] [Required] public SliderUI slider_lvlBar;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_healthNumLvl;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_healthDeltaLvl;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_apNumLvl;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_apDeltaLvl;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_dmgNumLvl;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_dmgDeltaLvl;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_armorNumLvl;
    [BoxGroup("Upgrade Screen")] [Required] public TextMeshProUGUI txt_armorDeltaLvl;
    [BoxGroup("Upgrade Screen")] [Required] public RectTransform rect_lvlUpDisp;
    [BoxGroup("Upgrade Screen")] [Required] public Upgrader upgrader;
    [BoxGroup("Upgrade Screen")] [Required] public GameObject go_upgrader;


    [Required] public TextMeshProUGUI txt_descTactics;
    [Required] public TextMeshProUGUI txt_descAbout;
    [Required] public GameObject go_frontalDarken;



    [ReadOnly] public List<CharacterSelectPortrait> characterSelectPortraits = new();
    public List<CharacterSelectPortrait> glgCharacterSelectPortraits = new();

    public void Awake()
    {
        selectedCharacter = null;
    }
    
    void SetMainBtnTxtColor(string txt, Color color)
    {
        txt_buttonLabel.text = txt;
        txt_buttonLabel.color = color;
    }
    
    
    #region ---------------------- Chose Starter Character ----------------------

    public void ViewChoseStarterCharacterScreen()
    {
        selectBtn.interactable = false;
        selectedCharacter = initiallySelectedPortrait.characterCfg;
        SetMainBtnTxtColor("Select Character", mainBtnTxtColorRegular);
        txt_altName.text = "";
        txt_Name.text = "";
        txt_lvl.text = "";
        txt_spEffect.text = "";
        txt_healthNum.text = "";
        txt_apNum.text = "";
        txt_dmgNum.text = "";
        txt_armorNum.text = "";
        uiOrchestration.currentExplorationUIState = ExplorationUIState.FirstStart;
        mainArtTransform.DOMove(characterSelectMainArtPos.position, mainArtMoveDuration);
        Debug.Log("Successfully chose starter character");
    }

    public void ViewCharacterScreen()
    {
        teamGrid.ResetState();
        characterSelectPortraits.Clear();
        characterSelectPortraits.AddRange(glgCharacterSelectPortraits);
        SetMainBtnTxtColor("DETAILS", mainBtnTxtColorRegular);
        characterSelectPortraits.ForEach(p =>
        {
            p.portraitType = PortraitType.InfoDisplay;
            p.UpdateState();
        });
        
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var data);
        string firstUnlockedChar = data.charactersData.Find(c => c.hasCharacter).characterConfigName;
        characterSelectPortraits.Find(c => c.characterCfg.occupantName == firstUnlockedChar).SelectNewCharacter();
        
        mainArtTransform.DOMove(characterSelectMainArtPos.position, mainArtMoveDuration);
        
        UpdateCharacterInfoDisplayStats();
    }

    List<MaterialBtn> materialButtonBuffer = new List<MaterialBtn>();
    public void ViewUpgradeLevelScreen()
    {
        go_upgrader.gameObject.SetActive(false);
        Debug.Log("UPGRADE LEVEL SCREEN");
        // transition & setup
        uiOrchestration.currentCharacterUIState = CharacterUIState.Upgrading;
        SetMainBtnTxtColor("LEVEL UP", mainBtnTxtColorDisabled);
        mainArtTransform.DOMove(upgradeLevelMainArtPos.position, mainArtMoveDuration);
        txt_autoAdd.text = "AUTO ADD";

        // vars
        var charData = SaverService.Instance.GetData<CharactersData_SavedDataSO>(out _)
            .GetCharacterData(selectedCharacter.occupantName);
        var charLevel = charData.level;
        var nxtLevel = charLevel + 1;
        bool reachedMaxLvl = charLevel >= PersistentConfigurationDataHolder.Instance.characterConfigs.maxLevel;
        
        // levl bar
        txt_currLevl.text = "Lvl. " + charLevel;
        txt_nxtLevel.text = reachedMaxLvl ? "MAX" : "Lvl. " + nxtLevel;
        go_lvlArrowDisp.SetActive(!reachedMaxLvl);
        slider_lvlBar.SetValue01(reachedMaxLvl ? 1 : 0);
        
        UpdateCharacterInfoDisplayStats();

        txt_healthDeltaLvl.text = "";
        txt_apDeltaLvl.text = "";
        txt_dmgDeltaLvl.text = "";
        txt_armorDeltaLvl.text = "";
        rect_lvlUpDisp.RefreshLayoutGroupsImmediateAndRecursive();
        
        // show materials
        hlg_materialsBtnParent.transform.Children().DestroyAll();
        materialButtonBuffer.Clear();
        btn_autoAdd.interactable = !reachedMaxLvl;
        if (reachedMaxLvl) return;
        
        var nxtLvlData = selectedCharacter.levelData[nxtLevel];
        foreach (var materialReq in nxtLvlData.MaterialRequirements)
        {
            var item = materialReq.item;
            var requiredAmount = materialReq.amount;
            var hasAmount = SaverService.Instance.GetData<Items_SavedDataSO>(out _)
                .GetItemData(item).amount;
            var materialBtn = Instantiate(pr_materialBtn, hlg_materialsBtnParent).Get<MaterialBtn>();
            materialBtn.Init(item, requiredAmount, hasAmount, TestForSatisfied);
            materialButtonBuffer.Add(materialBtn);
        }

        selectBtn.interactable = false;
    }

    bool autoAddButtonAnyAdded = false;
    // doesnt need max lvl checks cause interactability is setup in the show screen
    public void PressedAutoAdd()
    {
        // for auto add all
        if (autoAddButtonAnyAdded == false)
        {
            bool addedAny = false;
            var charData = SaverService.Instance.GetData<CharactersData_SavedDataSO>(out _)
                .GetCharacterData(selectedCharacter.occupantName);
            var nxtLevel = charData.level + 1;
        
            var nxtLevelData = selectedCharacter.levelData[nxtLevel];
            var requirementsAmount = nxtLevelData.MaterialRequirements.Count;
            for (var i = 0; i < requirementsAmount; i++)
            {
                var materialReq = nxtLevelData.MaterialRequirements[i];
                var item = materialReq.item;
                var hasAmount = SaverService.Instance.GetData<Items_SavedDataSO>(out _)
                    .GetItemData(item).amount;
                if (hasAmount > 0) addedAny = true;

                materialButtonBuffer[i].AutoCommitAsMuchAsPossible();
            }

            if (addedAny)
            {
                txt_autoAdd.text = "REMOVE ALL";
                autoAddButtonAnyAdded = true;
            }
            TestForSatisfied();
        }

        // for remove all
        else
        {
            materialButtonBuffer.ForEach(mb => mb.UnCommitAsMuchAsPossible());
            autoAddButtonAnyAdded = false;
            txt_autoAdd.text = "AUTO ADD";
        }
    }

    public void TestForSatisfied()
    {
        bool allSatisfied = materialButtonBuffer.All(b => b.satisfied);
        selectBtn.interactable = allSatisfied;
        Color color = allSatisfied ? mainBtnTxtColorRegular : mainBtnTxtColorDisabled;
        SetMainBtnTxtColor("LEVEL UP", color);

        if (allSatisfied)
        {
            autoAddButtonAnyAdded = true;
            txt_autoAdd.text = "REMOVE ALL";
        }
        else
        {
            autoAddButtonAnyAdded = false;
            txt_autoAdd.text = "AUTO ADD";
        }
        
        // lvl bar
        float allNeeded = materialButtonBuffer.Sum(b => b.requiredAmount);
        float allCommitted = materialButtonBuffer.Sum(b => b.commitedAmount);
        slider_lvlBar.SetValue(allCommitted, 0, allNeeded);
        
        // added stats
        var charData = SaverService.Instance.GetData<CharactersData_SavedDataSO>(out _)
            .GetCharacterData(selectedCharacter.occupantName);
        var charCfg = PersistentConfigurationDataHolder.Instance.characterConfigs.characterConfigs[selectedCharacter.occupantName];
        var nxtLevelDeltas = charCfg.levelData[charData.level + 1].statDeltas;
        txt_healthDeltaLvl.text = nxtLevelDeltas.deltas.health > 0 && allCommitted > 0 ? "+" + nxtLevelDeltas.deltas.health.ToString() : "";
        txt_apDeltaLvl.text = nxtLevelDeltas.deltas.ap > 0 && allCommitted > 0 ? "+" + nxtLevelDeltas.deltas.ap.ToString() : "";
        txt_dmgDeltaLvl.text = nxtLevelDeltas.deltas.damage > 0 && allCommitted > 0 ? "+" + nxtLevelDeltas.deltas.damage.ToString() : "";
        txt_armorDeltaLvl.text = nxtLevelDeltas.deltas.armor > 0 && allCommitted > 0 ? "+" + nxtLevelDeltas.deltas.armor.ToString() : "";
        
        rect_lvlUpDisp.RefreshLayoutGroupsImmediateAndRecursive();
    }

    public void UseUpgrader()
    {
        go_upgrader.gameObject.SetActive(true);
        
        // vars
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var data);
        var charData = data.GetCharacterData(selectedCharacter.occupantName);
        var newLvl = charData.level + 1;
        
        var charCfg = PersistentConfigurationDataHolder.Instance.characterConfigs.characterConfigs[selectedCharacter.occupantName];
        charCfg.GetStats(charData.level, out var stats);
        
        upgrader.Upgrade(newLvl, stats, charCfg.levelData[newLvl].statDeltas.deltas);

        charData.level++;
        saver.Save();
        
        go_frontalDarken.gameObject.SetActive(true);
    }

    public void ContinueUpgrader()
    {
        go_upgrader.gameObject.SetActive(false);
        go_frontalDarken.gameObject.SetActive(false);
        autoAddButtonAnyAdded = false;
        ViewUpgradeLevelScreen();
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
        onOverviewScreen = false;
        onActionsOrEquipmentScreen = false;
        CloseCharacterInfoPanel();
    }
    
    public bool onOverviewScreen;
    public bool onActionsOrEquipmentScreen;

    public void ViewOverviewScreen()
    {
        Debug.Log("OVERVIEW SCREEN");
        // If already had started to inspect this character
        CloseCharacterInfoPanel();
        mainArtTransform.DOMove(overviewMainArtPos.position, mainArtMoveDuration);
        
        onActionsOrEquipmentScreen = false;

        if (onOverviewScreen) // already on
            return;
        
        
        // if not, play the animation (Newly inspected character)
        txt_descTactics.text = selectedCharacter.desc_tactics;
        txt_descAbout.text = selectedCharacter.desc_about;
        onOverviewScreen = true;
        characterInfoPanelTransform.gameObject.Get<RectTransform>().RefreshLayoutGroupsImmediateAndRecursive();
        
        SetMainBtnTxtColor("LEVEL UP", mainBtnTxtColorRegular);
    }
    
    
    public void ViewActionsOrEquipmentScreen()
    {
        Debug.Log("ACTIONS OR EQUIPMENT SCREEN");
        CloseCharacterInfoPanel();
        mainArtTransform.DOMove(actionsOrEquipmentMainArtPos.position, mainArtMoveDuration);
        
        onOverviewScreen = false;
        
        if (onActionsOrEquipmentScreen) // already on
            return;
        
        onActionsOrEquipmentScreen = true;
    }

    public void ViewActionsScreen()
    {
        ViewActionsOrEquipmentScreen();
        if(selectedCharacter == null) this.Error("Selected Character is null when viewing actions display menu");
        menuActionsDisplay.DisplayActionsFor(selectedCharacter);
    }

    public void MainButtonPressed()
    {
        if (uiOrchestration.currentUIState == UIState.Main)
        {
            // Insppect character
            switch (uiOrchestration.currentMainUIState)
            {
                case MainUIState.Characters: 
                    ResetDetailsMenus();
                    menuBarDetails.SelectOption("Overview");
                    Debug.Log("Overview Selected");
                    return;

            }
        }
        else if (uiOrchestration.currentUIState == UIState.Exploration)
        {
            // chose start character
            switch (uiOrchestration.currentExplorationUIState)
            {
                case ExplorationUIState.FirstStart: ChoseStarterCharacter(); return;
            }
        }
        else if (uiOrchestration.currentUIState == UIState.Character)
        {
            
            Debug.Log("ON CHAR");
            // Level up button
            switch (uiOrchestration.currentCharacterUIState)
            {
                case CharacterUIState.Overview: 
                    uiOrchestration.ShowCharacterUIState(CharacterUIState.Upgrading);
                    return;
                case CharacterUIState.Upgrading:
                    UseUpgrader();
                    return;
            }
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
        txt_lvl.text = string.Format(k_LevelFormat, SaverService.Instance.GetData<CharactersData_SavedDataSO>(out _).GetCharacterData(character.occupantName).level);
        txt_healthNum.text = character.maxHP.ToString();
        txt_apNum.text = character.maxAP.ToString();
        txt_dmgNum.text = character.damage.ToString();
        txt_armorNum.text = character.maxArmor.ToString();
        img_mainArt.sprite = character.mainArt;
        img_mainArt.gameObject.SetActive(true);
        selectBtn.interactable = true;
        Debug.Log("Successfully selected character: " + character.occupantName);
    }

    public void UpdateCharacterInfoDisplayStats()
    {
        var charData = SaverService.Instance.GetData<CharactersData_SavedDataSO>(out _)
        .GetCharacterData(selectedCharacter.occupantName);
        var charLevel = charData.level;
        
        // stats
        var charCfg = PersistentConfigurationDataHolder.Instance.characterConfigs.characterConfigs[selectedCharacter.occupantName];
        charCfg.GetStats(charLevel, out var stats);
        txt_healthNumLvl.text = stats.health.ToString();
        txt_apNumLvl.text = stats.ap.ToString();
        txt_dmgNumLvl.text = stats.damage.ToString();
        txt_armorNumLvl.text = stats.armor.ToString();
    }
    
    public void ChoseStarterCharacter()
    {
        SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var gameSaver, out var gameData);
        gameData.returningPlayer = true;
        gameSaver.Save();
        
        
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
        charData.level = 1;
        charData.equipped = true;
        
        SaverService.Instance.GetSaverAndData<Items_SavedDataSO>(out var itemSaver, out var itemData);
        foreach (var startingKitAction in selectedCharacter.startingKitActions)
        {
            itemData.AddItemData(startingKitAction.actionName, Items_SavedDataSO.ItemType.CharacterAction, 1);
            charData.equippedActions.Add(startingKitAction.actionName);
        }
        
        charSaver.Save();
        itemSaver.Save();
        
        foreach (var newStartingKitAction in selectedCharacter.startingKitActions)
        {
            Debug.Log("Added starting kit action: " + newStartingKitAction.actionName + " to character: " + selectedCharacter.occupantName + " amount is " 
                      + itemData.GetItemData(Items_SavedDataSO.ItemType.CharacterAction, newStartingKitAction.actionName).amount);
        }
        
        menuActionsDisplay.DisplayActionsFor(selectedCharacter);
    }

    public void OpenCharacterInfoPanel() => characterInfoPanelTransform.DOMove(openCharInfoPanelPos.position, mainArtMoveDuration);
    public void CloseCharacterInfoPanel() => characterInfoPanelTransform.DOMove(closeCharInfoPanelPos.position, mainArtMoveDuration);
}
    
#endregion ------------------------------------------------------------------
    
    

