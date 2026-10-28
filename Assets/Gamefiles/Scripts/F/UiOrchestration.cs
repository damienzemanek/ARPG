using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
        Pause,
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
    [DrawWithUnity] public SerializedDictionary<ExplorationUIState, UIStateData> explorationUIs = new();
    [DrawWithUnity] public SerializedDictionary<MainUIState, UIStateData> mainUIs = new();
    [DrawWithUnity] public SerializedDictionary<CharacterUIState, UIStateData> characterUIs = new();

    [Serializable]
    public class UIStateData
    {
        public List<GameObject> uiObjects;
        public UnityEvent onDisplay;
    }

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
        explorationUIs[explorationUIState].uiObjects.ForEach(g => g.SetActive(true));
        explorationUIs[explorationUIState].onDisplay?.Invoke();
        onUseExplorationUI?.Invoke(playerGO);
    }
    
    public void ShowMainUIState(int state) => ShowMainUIState((MainUIState)state);
    public void ShowMainUIState(MainUIState mainUIState)
    {
        currentUIState = UIState.Main;
        currentMainUIState = mainUIState;
        HideAllUIs();
        mainUIs[mainUIState].uiObjects.ForEach(g =>
        {
            g.SetActive(true);
            Debug.Log("Showing Main UI State on Obj " + g.name);
        });
        mainUIs[mainUIState].onDisplay?.Invoke();
        onUseMainUI?.Invoke(playerGO);
        Debug.Log("Showed Main UI State");
    }
    
    public void ShowCharacterUIState(int state) => ShowCharacterUIState((CharacterUIState)state);
    public void ShowCharacterUIState(CharacterUIState characterUIState)
    {
        currentUIState = UIState.Character;
        currentCharacterUIState = characterUIState;
        HideAllUIs();
        characterUIs[characterUIState].uiObjects.ForEach(g => g.SetActive(true));
        characterUIs[characterUIState].onDisplay?.Invoke();
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
        Debug.Log("Back");
        if (currentExplorationUIState == ExplorationUIState.FirstStart ||
            currentExplorationUIState == ExplorationUIState.None) return;
        switch (currentUIState)
        {
            case UIState.Main: ShowExplorationUIState(ExplorationUIState.Exploration); break;
            case UIState.Character: ShowMainUIState(MainUIState.Characters); break;
            case UIState.Exploration: ShowMainUIState(MainUIState.Pause); Debug.Log("Pausing! "); break;
            default: Debug.Log("Did not move to a different UI State"); break;
        }
    }
}
