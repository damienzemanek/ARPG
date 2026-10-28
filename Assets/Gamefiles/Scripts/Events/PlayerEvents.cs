using System;
using System.Collections;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using static UiOrchestration;

public class PlayerEvents : MonoBehaviour
{
    public enum PlayerEvent
    {
        None,
        NewPlayerSpawn,
        Respawn,
        MostRecentPosition,
        StopPlayerMovement,
        ReEnablePlayerMovement,
    }

    [Required] public Animator playerAnimator;
    [Required] public string awakenAnimName;

    [Required] public GameObject allUI;
    [Required] public UiOrchestration uiOrchestration;
    [Required] public CharacterInfoDisplay characterInfoDisplay;
    public WaitSecondsRealtime charDisplayWait; // DELAY FOR REGION ANIM
    [Required] public PlayerInstance player;

    public void NewPLayerSpawnEvent()
    {
        Debug.Log("New Player Spawned");
        allUI.gameObject.SetActive(true);
        uiOrchestration.HideAllUIs();
        player.gameObject.SetActive(false);
        StartCoroutine(C_Display());
        
        IEnumerator C_Display() 
        { 
            Debug.Log("Chosing Starter Character...");
            yield return charDisplayWait.Delay;
            Debug.Log("Chose A Starter Character");
            characterInfoDisplay.ViewChoseStarterCharacterScreen();
            uiOrchestration.ShowExplorationUIState(ExplorationUIState.FirstStart);
            SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var saver, out var data);
            SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var charSaver, out var charData);
            charData.charactersData.ForEach(c =>
            {
                c.row = -1;
                c.col = -1;
            });
            charSaver.Save();
            data.returningPlayer = true;
            saver.Save();
        }
    }

    public void RespawnningPlayerEvent()
    {
        Debug.Log("Respawning PLayer");
        allUI.gameObject.SetActive(true);
        uiOrchestration.HideAllUIs();
        player.gameObject.SetActive(false);
        SaveCurrentPosition();
        ReEnablePlayerAndMovement(() =>
        {
            uiOrchestration.ShowExplorationUIState(ExplorationUIState.Exploration);
        });
    }


    
    public void MostRecentPositionSpawnEvent()
    {
        Debug.Log("Most Recent Position Spawned");
        player.gameObject.SetActive(true);
        player.ToggleInputReading(true);
        // TODO: allow for dungeon ui to show here too with a simple check if the player is in either the dungeon or the exploration
        uiOrchestration.ShowExplorationUIState(ExplorationUIState.Exploration);
        SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out _, out var data);
        player.gameObject.transform.position = data.currentCharacterWorldLocation.position;
        player.gameObject.transform.eulerAngles = data.currentCharacterWorldLocation.rotation;
    }
    
    
    public void ReEnablePlayerAndMovement(Action cb = null)
    {
        player.gameObject.SetActive(true);
        player.ToggleInputReading(false);
        playerAnimator.PlayOnEnd(awakenAnimName, () =>
        {
            player.ToggleInputReading(true);
            cb?.Invoke();
        });
    }

    public void SaveCurrentPosition()
    {
        SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var saver, out var data);
        data.currentCharacterWorldLocation = new Pose(player.gameObject.transform);
        saver.Save();
    }
    
}
