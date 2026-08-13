using System.Collections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    public enum PlayerEvent
    {
        NewPlayerSpawn,
    }

    [Required] public Animator playerAnimator;
    [Required] public string awakenAnimName;
    
    [Required] public GameObject characterSelectDisplay;
    public CharacterInfoDisplay characterInfoDisplay;
    public WaitSecondsRealtime charDisplayWait;
    [Required] public PlayerInstance player;

    public void NewPLayerSpawnEvent()
    {
        Debug.Log("New Player Spawned");
        characterSelectDisplay.SetActive(false);
        player.gameObject.SetActive(false);
        StartCoroutine(C_Display());
        
        IEnumerator C_Display() { yield return charDisplayWait.Delay;
            characterSelectDisplay.SetActive(true); }
    }

    public void SelectedNewPlayerCharacter(CharacterConfig character)
    {
        Debug.Log("Selected New Player Character " + character.occupantName);
        characterSelectDisplay.SetActive(false);
        player.gameObject.SetActive(true);
        player.ToggleInputReading(false);
        playerAnimator.PlayOnEnd(awakenAnimName, () => player.ToggleInputReading(true));
    }
    
}
