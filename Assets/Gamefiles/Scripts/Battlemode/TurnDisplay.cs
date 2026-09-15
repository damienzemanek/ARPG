using System;
using System.Collections;
using EMILtools.Extensions;
using UnityEngine;

public class TurnDisplay : MonoBehaviour
{
    public Animator playerTurnAnimator;
    public Animator enemyTurnAnimator;
    public Animator battleStartAnimator;
    public string startTurnAnimName;
    public string startBattleAnimName;

    public IEnumerator StartBattle()
    {
        Debug.Log("Starting Battle");
        BattleTracker.Instance.currentTurn = BattleTracker.Turn.Transitioning;
        battleStartAnimator.gameObject.SetActive(true);
        playerTurnAnimator.gameObject.SetActive(false);
        enemyTurnAnimator.gameObject.SetActive(false);
        battleStartAnimator.Play(startBattleAnimName);
        yield return new WaitUntil(() =>
            battleStartAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );
    }
    
    public void StartTurn(BattleTracker.Turn turn, Action cb)
    {
        Debug.Log("Starting turn " + turn + "");
        BattleTracker.Instance.currentTurn = BattleTracker.Turn.Transitioning;
        if (turn == BattleTracker.Turn.Player)
        {
            playerTurnAnimator.gameObject.SetActive(true);
            playerTurnAnimator.PlayOnEnd(startTurnAnimName, () => OnTurnAnimComplete(turn, cb));
        }
        else
        {
            enemyTurnAnimator.gameObject.SetActive(true);
            enemyTurnAnimator.PlayOnEnd(startTurnAnimName, () => OnTurnAnimComplete(turn, cb));
        }
    }
    

    void OnTurnAnimComplete(BattleTracker.Turn turn, Action cb)
    {
        Debug.Log("Animation Complete");
        if (turn == BattleTracker.Turn.Player)
        {
            playerTurnAnimator.gameObject.SetActive(false);
            cb.Invoke();
        }
        else
        {
            enemyTurnAnimator.gameObject.SetActive(false);
            cb.Invoke();
        }
    }
}
