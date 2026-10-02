using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UiOrchestration : MonoBehaviour
{
    public enum UIState
    {
        Exploration,
        Main,
        Character,
    }
    
    public enum ExplorationUIState
    {
        None,
        FirstStart,
        Exploration,
        Dungeon,
    }
    
    public enum MainUIState
    {
        None,
        Characters,
        Team,
    }
    
    public enum CharacterUIState
    {
        None,
        Overview,
        Actions,
        Equipment
    }
    
    [Required] public Animator alertAnimator;
    [Required] public TextMeshProUGUI alertText;
    [Required] public GameObject playerGO;
    public EmilEvent<GameObject> onUseMainUI;
    public EmilEvent<GameObject> onUseExplorationUI;

    [ReadOnly] public UIState currentUIState = UIState.Exploration;
    [ReadOnly] public MainUIState currentMainUIState = MainUIState.None;
    [ReadOnly] public CharacterUIState currentCharacterUIState = CharacterUIState.None;
    [ReadOnly] public ExplorationUIState currentExplorationUIState = ExplorationUIState.None;

    public List<GameObject> allUIs = new();
    [DrawWithUnity] public SerializedDictionary<ExplorationUIState, List<GameObject>> explorationUIs = new();
    [DrawWithUnity] public SerializedDictionary<MainUIState, List<GameObject>> mainUIs = new();
    [DrawWithUnity] public SerializedDictionary<CharacterUIState, List<GameObject>> characterUIs = new();

    void Awake()
    {
        alertAnimator.gameObject.SetActive(false);
    }
    
    public void HideAllUIs() => allUIs.ForEach(x => x.SetActive(false));
    
    public void ShowExplorationUIState(ExplorationUIState explorationUIState)
    {
        currentUIState = UIState.Exploration;
        currentExplorationUIState = explorationUIState;
        HideAllUIs();
        explorationUIs[explorationUIState].ForEach(g => g.SetActive(true));
        onUseExplorationUI?.Invoke(playerGO);
    }
    
    public void ShowMainUIState(int state) => ShowMainUIState((MainUIState)state);
    public void ShowMainUIState(MainUIState mainUIState)
    {
        currentUIState = UIState.Main;
        currentMainUIState = mainUIState;
        HideAllUIs();
        mainUIs[mainUIState].ForEach(g => g.SetActive(true));
        onUseMainUI?.Invoke(playerGO);
    }
    
    public void ShowCharacterUIState(CharacterUIState characterUIState)
    {
        currentUIState = UIState.Character;
        currentCharacterUIState = characterUIState;
        HideAllUIs();
        characterUIs[characterUIState].ForEach(g => g.SetActive(true));
    }

    public void Alert(string text, Action cb = null)
    {
        alertAnimator.gameObject.SetActive(true);
        alertText.text = text;
        alertAnimator.PlayOnEnd("AlertStart", () =>
        {
            alertText.text = string.Empty;
            alertAnimator.gameObject.SetActive(false);
            cb?.Invoke();
        });
    }

    public void Back()
    {
        if (currentUIState == UIState.Main)
            ShowExplorationUIState(ExplorationUIState.Exploration);
        else if (currentUIState == UIState.Character)
            ShowMainUIState(MainUIState.Characters);
    }
}
