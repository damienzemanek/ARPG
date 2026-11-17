using System;
using System.Collections;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class Alerter : MonoBehaviour
{
    [Required] public GameObject go_alert;
    [Required] public TextMeshPro alertText;
    [Required] public Animator alertAnimator;
    public string alertAnimName = "Alert";
    public int alertCount;

    void Awake()
    {
        go_alert.gameObject.SetActive(false);
    }

    public void SendAlert(string text)
    {
        alertCount++;
        alertText.text = text;
        go_alert.SetActive(true);
        alertAnimator.PlayOnEnd(alertAnimName, () => {
            alertCount--;
            if (alertCount == 0)
                go_alert.SetActive(false);
        });
    }

    public IEnumerator C_SendAlert(string text)
    {
        alertCount++;
        alertText.text = text;
        go_alert.SetActive(true);
        yield return alertAnimator.C_PlayOnEnd(alertAnimName, () => {
            alertCount--;
            if (alertCount == 0)
                go_alert.SetActive(false);
        });
    }
}
