using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class FadeMutator : MonoBehaviour
{
    [Required] public Transform target;
    public float startValue;
    public float endValue;
    public float time;
    public bool fadeWhenEnabled;

    void OnEnable()
    {
        Fade(startValue, 0, false);
        if(fadeWhenEnabled)
            Fade(endValue, time, true);
    }
    
    void Fade(float alpha, float duration, bool enableRaycast)
    {
        var childrenThatAreImages = target.GetComponentsInChildren<Image>(true);
        foreach (var image in childrenThatAreImages)
        {
            image.raycastTarget = false;
            Debug.Log("Fading: " + image.gameObject.name);
            image.CrossFadeAlpha(alpha, duration, true);
            target.GetComponent<MonoBehaviour>().DelayedCall(() =>
            {
                image.raycastTarget = enableRaycast;
            }, duration);
        }
    }
}
