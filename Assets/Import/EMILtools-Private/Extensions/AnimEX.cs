using System;
using System.Collections;
using UnityEngine;

namespace EMILtools.Extensions;

public static class AnimEX
{
    public static void PlayOnEnd(this Animator animator, string stateName, Action callback, int layer = 0)
    {
        animator.Play(stateName, layer, 0f);
        CoroutineRunner.Instance.StartCoroutine(Wait());

        IEnumerator Wait()
        {
            // Wait until we've actually entered the requested state.
            while (!animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName)) yield return null;

            // Wait until it's done.
            while (animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1f) yield return null;

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