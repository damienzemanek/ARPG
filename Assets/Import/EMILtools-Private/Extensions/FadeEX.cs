using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EMILtools.Extensions
{
    public static class FadeEX
    {
        public static void EnableFadeOut(
            this MonoBehaviour host,
            FadeSettings fade,
            Object targ)
        {
            ResetFade(fade, true, targ);

            FadeToTransparent(
                fade,
                targ,
                () => ResetFade(fade, false, targ)
            );
        }

        public static void ResetFade(
            FadeSettings fade,
            bool _active,
            Object targ)
        {
            Color color = fade.GetColor(targ);
            color.a = 1f;

            fade.SetColor(color, targ);
            fade.GetGO(targ)?.gameObject.SetActive(_active);
        }

        public static void FadeToTransparent(
            FadeSettings fade,
            Object targ,
            Action postHook = null)
        {
            CoroutineRunner.Instance.StartCoroutine(
                C_FadeToTransparent(fade, targ, postHook)
            );
        }

        public static void FadeToOpaque(
            FadeSettings fade,
            Object targ,
            Action postHook = null)
        {
            CoroutineRunner.Instance.StartCoroutine(
                C_FadeToOpaque(fade, targ, postHook)
            );
        }

        public static void FadeToAlphaValueOf(
            FadeSettings fade,
            Object targ,
            float targetAlpha,
            Action postHook = null)
        {
            CoroutineRunner.Instance.StartCoroutine(
                C_FadeToAlphaValueOf(
                    fade,
                    targ,
                    targetAlpha,
                    postHook
                )
            );
        }

        public static IEnumerator C_FadeToTransparent(
            FadeSettings fade,
            Object targ,
            Action postHook = null)
        {
            if (fade.delayToStartFading > 0)
                yield return new WaitForSeconds(
                    fade.delayToStartFading
                );

            if (targ == null)
            {
                Debug.LogError(
                    "Given a null target for FadeToTransparent."
                );

                postHook?.Invoke();
                yield break;
            }

            fade.GetGO(targ)?.gameObject.SetActive(true);

            float fadeVal = 1f;
            Color currentColor = fade.GetColor(targ);

            while (fadeVal > 0f)
            {
                fadeVal -= fade.Step;
                fadeVal = Mathf.Max(fadeVal, 0f);

                currentColor.a = fadeVal;
                fade.SetColor(currentColor, targ);

                yield return new WaitForSeconds(fade.Delay);
            }

            currentColor.a = 0f;
            fade.SetColor(currentColor, targ);

            postHook?.Invoke();
        }

        public static IEnumerator C_FadeToOpaque(
            FadeSettings fade,
            Object targ,
            Action postHook = null)
        {
            if (fade.delayToStartFading > 0)
                yield return new WaitForSeconds(
                    fade.delayToStartFading
                );

            if (targ == null)
            {
                Debug.LogError(
                    "Given a null target for FadeToOpaque."
                );

                postHook?.Invoke();
                yield break;
            }

            fade.GetGO(targ)?.gameObject.SetActive(true);

            float fadeVal = 0f;
            Color currentColor = fade.GetColor(targ);

            while (fadeVal < 1f)
            {
                fadeVal += fade.Step;
                fadeVal = Mathf.Min(fadeVal, 1f);

                currentColor.a = fadeVal;
                fade.SetColor(currentColor, targ);

                yield return new WaitForSeconds(fade.Delay);
            }

            currentColor.a = 1f;
            fade.SetColor(currentColor, targ);

            postHook?.Invoke();
        }

        public static IEnumerator C_FadeToAlphaValueOf(
            FadeSettings fade,
            Object targ,
            float targetAlpha,
            Action postHook = null)
        {
            if (fade.delayToStartFading > 0)
                yield return new WaitForSeconds(
                    fade.delayToStartFading
                );

            if (targ == null)
            {
                Debug.LogError(
                    "Given a null target for FadeToAlphaValueOf."
                );

                postHook?.Invoke();
                yield break;
            }

            fade.GetGO(targ)?.gameObject.SetActive(true);

            targetAlpha = Mathf.Clamp01(targetAlpha);

            Color currentColor = fade.GetColor(targ);
            float fadeVal = currentColor.a;

            float step = fade.Step;

            while (!Mathf.Approximately(fadeVal, targetAlpha))
            {
                fadeVal = Mathf.MoveTowards(
                    fadeVal,
                    targetAlpha,
                    step
                );

                currentColor.a = fadeVal;
                fade.SetColor(currentColor, targ);

                yield return new WaitForSeconds(fade.Delay);
            }

            currentColor.a = targetAlpha;
            fade.SetColor(currentColor, targ);

            postHook?.Invoke();
        }
    }
}