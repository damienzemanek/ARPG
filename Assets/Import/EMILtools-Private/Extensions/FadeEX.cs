using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;




namespace EMILtools.Extensions
{
    public static class FadeEX
    {
        public static void EnableFadeOut(this MonoBehaviour host, FadeSettings fade, Object targ)
        {
            ResetFade(fade, true, targ);
            host.StartCoroutine(C_FadeToTransparent(fade, targ, () => ResetFade(fade, false, targ)));
        }

        public static void ResetFade(FadeSettings fade, bool _active, Object targ)
        {
            Color color = fade.GetColor(targ);
            color.a = 1f;
            fade.SetColor(color, targ);

            fade.GetGO(targ)?.gameObject.SetActive(_active);
        }

        public static IEnumerator C_FadeToTransparent(FadeSettings fade, Object targ, Action postHook = null)
        {
            if (fade.delayToStartFading > 0)
                yield return new WaitForSeconds(fade.delayToStartFading);

            fade.GetGO(targ)?.gameObject.SetActive(true);

            float fadeVal = 1f;
            Color currentColor = fade.GetColor(targ);

            while (fadeVal > 0)
            {
                fadeVal -= fade.step;
                currentColor.a = fadeVal;

                fade.SetColor(currentColor, targ);
                yield return new WaitForSeconds(fade.delay);
            }

            currentColor.a = 0;

            fade.SetColor(currentColor, targ);
            postHook?.Invoke();
        }


        public static IEnumerator C_FadeToOpaque(FadeSettings fade, Object targ, Action postHook = null)
        {
            if(fade.delayToStartFading > 0)
                yield return new WaitForSeconds(fade.delayToStartFading);

            if (targ == null)
            {
                Debug.LogError("Given a null target for fadeToOpaque.");
                postHook?.Invoke();
                yield break;
            }

            fade.GetGO(targ)?.gameObject.SetActive(true);

            float fadeVal = 0;
            Color currentColor = fade.GetColor(targ);

            while (fadeVal < 1)
            {
                fadeVal += fade.step;
                currentColor.a = fadeVal;

                fade.SetColor(currentColor, targ);
                yield return new WaitForSeconds(fade.delay);
            }

            currentColor.a = 1;
            fade.SetColor(currentColor, targ);

            postHook?.Invoke();
        }
    }

}