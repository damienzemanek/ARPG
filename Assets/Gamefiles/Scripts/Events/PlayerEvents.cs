using System.Collections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using static UiOrchestration;

public class PlayerEvents : MonoBehaviour
{
    public enum SpawnEvent
    {
        None,
        NewPlayerSpawn,
        MostRecentPosition,
        Death,
    }

    [Required] public Animator playerAnimator;
    [Required] public string awakenAnimName;
    
    [Required] public UiOrchestration uiOrchestration;
    [Required] public CharacterInfoDisplay characterInfoDisplay;
    public WaitSecondsRealtime charDisplayWait; // DELAY FOR REGION ANIM
    [Required] public PlayerInstance player;

    public void NewPLayerSpawnEvent()
    {
        Debug.Log("New Player Spawned");
        uiOrchestration.gameObject.SetActive(false);
        player.gameObject.SetActive(false);
        StartCoroutine(C_Display());
        
        IEnumerator C_Display() 
        { 
            yield return charDisplayWait.Delay;
            characterInfoDisplay.InitChoseStarterCharacter(characterInfoDisplay.initallySelectedStarterCharacter);
            uiOrchestration.gameObject.SetActive(true);
            uiOrchestration.ShowExplorationUIState(ExplorationUIState.FirstStart);
            SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var saver, out var data);
            data.returningPlayer = true;
            saver.Save();
        }
    }

    public void MostRecentPositionSpawnEvent()
    {
        Debug.Log("Most Recent Position Spawned");
        player.gameObject.SetActive(true);
        player.ToggleInputReading(true);
        uiOrchestration.gameObject.SetActive(false);
        // TODO: allow for dungeon ui to show here too with a simple check if the player is in either the dungeon or the exploration
        uiOrchestration.ShowExplorationUIState(ExplorationUIState.Exploration);
        SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var saver, out var data);
        player.gameObject.transform.position = data.currentCharacterWorldLocation.position;
        player.gameObject.transform.eulerAngles = data.currentCharacterWorldLocation.rotation;
    }

    public void ReEnablePlayerAndMovement()
    {
        uiOrchestration.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
        player.ToggleInputReading(false);
        playerAnimator.PlayOnEnd(awakenAnimName, () => player.ToggleInputReading(true));
    }

    public void SaveCurrentPosition()
    {
        SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var saver, out var data);
        data.currentCharacterWorldLocation = new Pose(player.gameObject.transform);
        saver.Save();
    }
    
}
