using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class EnemyIntentions : MonoBehaviour
{
    public TextMeshProUGUI txt_intentions;
    List<string> intentions = new();

    public void DisplayIntentions(List<BattleTracker.QueuedAction> queuedActions)
    {
        intentions.Clear();

        for (int i = 0; i < queuedActions.Count && i < 3; i++)
        {
            var queuedAction = queuedActions[i];

            intentions.Add(queuedAction.actionCtx.cfg.name);

            Debug.Log(
                $"Displaying Action: [{queuedAction.actionCtx.cfg.actionName}] " +
                $"Assetname: [{queuedAction.actionCtx.cfg.name}]");
        }

        RefreshDisplay();
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

        for (int i = 0; i < intentions; i++)
        {
            if (i >= predicted)
            {
                if (i < this.intentions.Count)
                    this.intentions[i] = "???";
                else
                    this.intentions.Add("???");
            }
        }

        this.intentions.RemoveRange(
            intentions,
            Mathf.Max(0, this.intentions.Count - intentions));

        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        var sb = new StringBuilder();

        for (int i = 0; i < intentions.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {intentions[i]}");
        }

        txt_intentions.text = sb.ToString().TrimEnd();
    }
}