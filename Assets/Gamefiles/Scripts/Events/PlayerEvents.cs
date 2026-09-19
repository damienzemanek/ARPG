using System.Collections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

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
    
    [Required] public GameObject GUI;
    [Required] public CharacterInfoDisplay characterInfoDisplay;
    public WaitSecondsRealtime charDisplayWait; // DELAY FOR REGION ANIM
    [Required] public PlayerInstance player;

    public void NewPLayerSpawnEvent()
    {
        Debug.Log("New Player Spawned");
        GUI.SetActive(false);
        player.gameObject.SetActive(false);
        StartCoroutine(C_Display());
        
        IEnumerator C_Display() 
        { 
            yield return charDisplayWait.Delay;
            characterInfoDisplay.InitChoseStarterCharacter(characterInfoDisplay.initallySelectedStarterCharacter);
            GUI.SetActive(true);
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
        GUI.SetActive(false);
        SaverService.Instance.GetSaverAndData<ARPG_SavedDataSO>(out var saver, out var data);
        player.gameObject.transform.position = data.currentCharacterWorldLocation.position;
        player.gameObject.transform.eulerAngles = data.currentCharacterWorldLocation.rotation;
    }

    public void ReEnablePlayerAndMovement()
    {
        GUI.SetActive(false);
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
