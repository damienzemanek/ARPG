using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyIntentions : MonoBehaviour
{
    public GameObject action1;
    public GameObject action2;
    public GameObject action3;
    public GameObject bufferBtwx1and2;
    public GameObject bufferBtwx2and3;

    public TextMeshProUGUI txt_action1Intention;
    public TextMeshProUGUI txt_action2Intention;
    public TextMeshProUGUI txt_action3Intention;

    public void DisplayIntentions(List<BattleTracker.QueuedAction> queuedActions)
    {
        var currentIndex = 0;
        foreach (var queuedAction in queuedActions)
        {
            switch (currentIndex)
            {
                case 0: txt_action1Intention.text = queuedAction.actionCtx.cfg.name; break;
                case 1: txt_action2Intention.text = queuedAction.actionCtx.cfg.name; break;
                case 2: txt_action3Intention.text = queuedAction.actionCtx.cfg.name; break;
            }
            currentIndex++;
        }
    }
    public void HideUnpredictedIntentions(int intentions, int predicted)
    {
        if (intentions < 0 || intentions > 3)
        {
            Debug.LogError("Invalid amount of Intentions, must be between 0 and 3.");
            return;
        }

        if (predicted < 0 || predicted > intentions)
        {
            Debug.LogError(
                $"Invalid amount of Predicted Intentions ({predicted}). " +
                $"Must be between 0 and {intentions}.");
            return;
        }

        action1.SetActive(intentions >= 1);
        action2.SetActive(intentions >= 2);
        action3.SetActive(intentions >= 3);

        bufferBtwx1and2.SetActive(intentions >= 2);
        bufferBtwx2and3.SetActive(intentions >= 3);

        if (intentions >= 1 && predicted < 1) txt_action1Intention.text = "???";
        if (intentions >= 2 && predicted < 2) txt_action2Intention.text = "???";
        if (intentions >= 3 && predicted < 3) txt_action3Intention.text = "???";
    }
}
