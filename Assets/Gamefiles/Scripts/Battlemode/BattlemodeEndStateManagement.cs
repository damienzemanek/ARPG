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

    private void Awake()
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
        winAnimator.PlayOnEnd(winStartAnimationName, () => StartCoroutine(C_DistributeRewards()));
    }

    IEnumerator C_DistributeRewards()
    {
        var rewards = SessionData.Instance.currentBattlemodePotentialRewards != null
            ? SessionData.Instance.currentBattlemodePotentialRewards.rewards
            : SessionData.Instance.defaultRewards.rewards;
        
        foreach (var reward in rewards)
        {
            yield return distributionDelay.Delay;
            var newItemHolder = Instantiate(itemHolderPrefab, itemHLG);
            newItemHolder.Get<ItemHolder>().Init(reward.ProvideReward());
            // play a sound
        }
        
        yield return distributionDelay.Delay;
        continueButton.gameObject.SetActive(true);
    }

    void Lose()
    {
        dispLose.SetActive(true);
        loseAnimator.Play(loseStartAnimationName);
    }
}
