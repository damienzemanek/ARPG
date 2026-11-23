using System;
using System.Collections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class BattlemodeEndStateManagement : MonoBehaviour
{
    public enum EndStates
    {
        Win,
        Lose,
    }

    public GameObject continueButton;

    
    public GameObject dispWin;
    public Animator winAnimator;
    public string winStartAnimationName;
    public Transform itemHLG;
    public GameObject itemHolderPrefab;
    public WaitSeconds distributionDelay;
    
    public GameObject dispLose;
    public Animator loseAnimator;
    public string loseStartAnimationName;

    void Awake()
    {
        continueButton.gameObject.SetActive(false);
        dispLose.SetActive(false);
        dispWin.SetActive(false);
    }

    [Button]
    public void EndStateWith(EndStates endState)
    {
        switch (endState)
        {
            case EndStates.Win: Win(); break;
            case EndStates.Lose: Lose(); break;
        }
    }

    void Win()
    {
        dispWin.SetActive(true);
        SessionData.Instance.currentPlayerEvent = PlayerEvents.PlayerEvent.MostRecentPosition;
        winAnimator.PlayOnEnd(winStartAnimationName, () => StartCoroutine(C_DistributeRewards()));
    }

    IEnumerator C_DistributeRewards()
    {
        var rewards = SessionData.Instance.currentBattlemodePotentialRewards;
        
        
        foreach (var reward in rewards)
        {
            yield return distributionDelay.Delay;
            var newItemHolder = Instantiate(itemHolderPrefab, itemHLG).Get<ItemHolder>();
            newItemHolder.Init(reward.item, reward.amount);
            ProvideReward(reward);
            // TODO: SOUND
        }
        
        yield return distributionDelay.Delay;
        continueButton.gameObject.SetActive(true);
    }

    public void ProvideReward(RewardConfigData reward)
    {
        SaverService.Instance.GetSaverAndData<Items_SavedDataSO>(out var saver, out var data);
        data.AddItemData(reward.item.itemData.itemName, reward.item.itemType, reward.amount);
        saver.Save();
    }

    void Lose()
    {
        dispLose.SetActive(true);
        SessionData.Instance.currentPlayerEvent = PlayerEvents.PlayerEvent.Respawn;
        loseAnimator.PlayOnEnd(loseStartAnimationName, () => continueButton.gameObject.SetActive(true));
    }
}
