using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class AudioEX
    {
        [Serializable]
        public struct AFX_Single
        {
            public AudioSource source;
            public AudioClip clip;
        }

        public static void Play(this AFX_Single afx, bool once = false)
        {
            if (afx.source == null) return;
            if (once && afx.source.isPlaying) return;
            afx.source.Play(afx.clip);
        }

        public static void Stop(this AFX_Single afx)
        {
            if (afx.source == null) return;
            afx.source.Stop();
        }

        public static void Play(this AudioSource source, AudioClip clip, bool oneShot = true, bool once = false)
        {
            if (once && source.isPlaying) return;
            
            if (oneShot)
                source.PlayOneShot(clip);
            else
            {
                if (source.isPlaying) source.Stop();
                source.clip = clip;
                if(source && source.isActiveAndEnabled) source.Play();
            }
        }

        public static void PlayForSeconds(this MonoBehaviour host, AudioSource source, AudioClip clip, float time, float fadeAtPercent)
        {
            source.clip = clip;
            source.Play();
            host.StartCoroutine(C_CutshortFade(source, time, fadeAtPercent));
        }

        public static IEnumerator C_CutshortFade(AudioSource source, float time, float startFadingAtPercent)
        {
            startFadingAtPercent = Mathf.Clamp(startFadingAtPercent, 0, 100);

            float vol = 1f;
            source.volume = vol;
            float startFadingAtSeconds = time * (startFadingAtPercent / 100f);
            float fadeOverSeconds = time - startFadingAtSeconds;
            float delay = fadeOverSeconds / 100;

            yield return new WaitForSeconds(startFadingAtSeconds);

            while (vol > 0)
            {
                yield return new WaitForSeconds(delay);
                vol -= 0.01f;
                source.volume = vol;
            }

            source.Stop();
        }


    }
}