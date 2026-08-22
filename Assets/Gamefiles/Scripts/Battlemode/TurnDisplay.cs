using System;
using EMILtools.Extensions;
using UnityEngine;

public class TurnDisplay : MonoBehaviour
{
    public Animator playerTurnAnimator;
    public Animator enemyTurnAnimator;
    public string startTurnAnimName;
    

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
