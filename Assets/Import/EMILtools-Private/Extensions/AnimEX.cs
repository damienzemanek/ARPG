using System;
using System.Collections;
using UnityEngine;

namespace EMILtools.Extensions;

public static class AnimEX
{
    public static void PlayOnEnd(this Animator animator, string stateName, Action callback, int layer = 0)
    {
        Debug.Log("Playing " + stateName);
        animator.Play(stateName, layer, 0f);
        CoroutineRunner.Instance.StartCoroutine(Wait());

        IEnumerator Wait()
        {
            Debug.Log("Waiting for " + stateName);
            // Wait until we've actually entered the requested state.
            while (!animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName)) yield return null;
            Debug.Log("State entered");
            // Wait until it's done.
            while (animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f) yield return null;
            Debug.Log("State finished, calling callback");
            callback?.Invoke();
        }
    }
    
    public static void CrossFadeOnEnd( this Animator animator, string stateName, float transitionDuration,
        Action callback, int layer = 0)
    {
        animator.CrossFadeInFixedTime(stateName, transitionDuration, layer);
        CoroutineRunner.Instance.StartCoroutine(Wait());

        IEnumerator Wait()
        {
            // Wait for the Animator to evaluate the crossfade.
            yield return null;

            while (true)
            {
                var state = animator.GetCurrentAnimatorStateInfo(layer);

                if (state.IsName(stateName) &&
                    state.normalizedTime >= 1f &&
                    !animator.IsInTransition(layer))
                    break;

                yield return null;
            }

            callback?.Invoke();
        }
    }

}