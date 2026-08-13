using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class SoundUser : MonoBehaviour
{
    [Serializable]
    [InlineProperty]
    [HideLabel]
    public struct ClipContainer
    {
        [HideIf("singularOrNoClips")] public int index;
        public AudioClip[] clips;

        public AudioClip GetSingle() => clips[0];

        public AudioClip GetLinear()
        {
            var clip = clips[index];
            index++;
            if (index >= clips.Length) index = 0;
            return clip;
        }

        public AudioClip GetRandom() => singularOrNoClips ? GetSingle() : clips[UnityEngine.Random.Range(0, clips.Length)];

        public bool singularOrNoClips => clips.Length <= 1;
    }

    public EnumSO soundConfig;

    [Serializable]
    public class SoundFX
    {
        [ReadOnly] public string name;
        [ReadOnly] public bool oneShot => !fade && !loop;

        public bool loop;
        public bool fade;

        [ShowIf("fade")] public FadeData fadeData;

        [ShowIf("oneShot")] public float oneShotVolume;

        [HideIf("oneShot")] [Range(0, 1)] public float startPercentage;

        [Space]
        public ClipContainer clips;
    }

    public List<SoundFX> soundFXs;
    public AudioSource audioSource;

    public bool playOnAwake;

    [ShowIf("playOnAwake")]
    [ValueDropdown("GetSoundOptions")]
    public string awakeSound;

    Coroutine activeRoutine;

    float tempTargetVolume = 1f;

    // =========================
    // SETUP
    // =========================

    public void PopulateSounds()
    {
        soundFXs ??= new List<SoundFX>();

        foreach (var sound in soundConfig.GetEnumVals())
        {
            if (!soundFXs.Any(s => s.name == sound))
                soundFXs.Add(new SoundFX { name = sound });
        }
    }

    IEnumerable GetSoundOptions()
    {
        if (soundConfig == null) return null;
        return soundConfig.GetEnumVals();
    }

    bool GetSoundFX(string name, out SoundFX soundFX)
    {
        soundFX = soundFXs.FirstOrDefault(s => s.name == name);
        return soundFX != null;
    }

    void Awake()
    {
        if (playOnAwake)
            PlayCore(awakeSound);
    }

    // =========================
    // BUTTON / TEST
    // =========================

    string PlayButtonLabel =>
        (audioSource != null && audioSource.isPlaying) ? "Stop Preview" : "Play Preview";

    [Button("$PlayButtonLabel")]
    public void PlayToggle([ValueDropdown(nameof(GetSoundOptions))] string sound)
    {
        if (audioSource == null) return;
        if (!GetSoundFX(sound, out var sfx)) return;

        if (sfx.fadeData.fadingOut && activeRoutine != null)
        {
            float currentTime = audioSource.time;
            float currentVolume = audioSource.volume;

            StopCoroutine(activeRoutine);
            activeRoutine = null;

            sfx.fadeData.fadingOut = false;

            PlayCore(
                sound,
                currentTime / Mathf.Max(audioSource.clip.length, 0.0001f),
                currentVolume);

            return;
        }

        if (activeRoutine != null)
            Stop(sound);
        else
            PlayCore(sound);
    }

    // =========================
    // PUBLIC API
    // =========================

    public void Play(Enum soundName, bool loop)
    {
        var sound = soundName.ToString();
        Debug.Log("Attempting to play: " + sound + "");
        if (audioSource == null) return;
        Debug.Log("AudioSource is not null");
        if (!GetSoundFX(sound, out var sfx))
        {
            Debug.LogError($"SoundFX not found: {sound}");
            return;
        }

        if (sfx.fadeData.fadingOut && activeRoutine != null)
        {
            float currentTime = audioSource.time;
            float currentVolume = audioSource.volume;

            StopCoroutine(activeRoutine);
            activeRoutine = null;

            sfx.fadeData.fadingOut = false;

            PlayCore(
                sound,
                currentTime / Mathf.Max(audioSource.clip.length, 0.0001f),
                currentVolume);

            return;
        }

        if (activeRoutine == null)
        {
            PlayCore(sound);
            Debug.Log("Sound played");
        }
        else
        {
            Debug.LogWarning("Sound already playing");
        }
    }
    

    // =========================
    // CORE
    // =========================

    void PlayCore(
        string sound,
        float overrideStartingPercentage = -1f,
        float overrideStartingVolume = -1f)
    {
        if (soundConfig == null || audioSource == null) return;

        Debug.Log("Playing: " + sound);
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        Debug.Log("Active routine stopped");
        if (!GetSoundFX(sound, out var sfx)) return;
        Debug.Log("SoundFX found");
        var clip = sfx.clips.GetRandom();
        bool overridingStartPercentage = Mathf.Approximately(overrideStartingPercentage, -1f);
        bool overridingStartingVolume = Mathf.Approximately(overrideStartingVolume, -1f);

        float startTime = overridingStartPercentage
            ? clip.length * sfx.startPercentage
            : clip.length * overrideStartingPercentage;

        float startVolume = overridingStartingVolume
            ? (sfx.fade ? 0f : 1f)
            : overrideStartingVolume;

        Debug.Log("Starting at: " + startTime);
        if (sfx.loop && !sfx.fade)
        {
            Debug.Log("SoundFX is looping");
            audioSource.clip = clip;
            audioSource.time = startTime;
            audioSource.volume = startVolume;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (sfx.loop && sfx.fade)
        {
            Debug.Log("SoundFX is looping with fade");
            audioSource.clip = clip;
            audioSource.time = startTime;
            audioSource.volume = startVolume;
            audioSource.Play();
            Debug.Log("Is AudioSource playing: " + audioSource.isPlaying);

            float routineStartVolume = overridingStartPercentage ? startVolume : startVolume;

            activeRoutine = StartCoroutine(
                FadeRoutine(
                    sfx,
                    startTime,
                    routineStartVolume));
        }
        else if (sfx.oneShot)
        {
            Debug.Log("SoundFX is one-shot");
            audioSource.PlayOneShot(clip, sfx.oneShotVolume);
        }
        Debug.Log("SoundFX Complete");
    }

    public void FixedUpdate()
    {
        Debug.Log(audioSource.volume);
    }

    // =========================
    // STOP / FADE OUT
    // =========================

    public void Stop(string sound)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (!GetSoundFX(sound, out var sfx)) return;

        activeRoutine = StartCoroutine(FadeOutAndStop(sfx));
    }

    IEnumerator FadeOutAndStop(SoundFX sfx)
    {
        if (audioSource == null) yield break;

        // Immediately update fade states
        sfx.fadeData.fadingIn = false;
        sfx.fadeData.fadingOut = true;

        // startVol is in perceptual space (0..1)
        float startVol = Mathf.Pow(audioSource.volume, 0.25f);

        float baseDuration = sfx.fadeData.useFadeOut
            ? Mathf.Abs(sfx.fadeData.fadeOutRange.x - sfx.fadeData.fadeOutRange.y)
            : 0f;

        // Scale duration based on the ratio of current volume to target volume.
        // We use perceptual volume for scaling.
        float duration = baseDuration * (startVol / Mathf.Max(tempTargetVolume, 0.0001f));

        // Increase minimum duration for very short fades to prevent clicking
        duration = Mathf.Max(duration, 0.5f);

        float progress = 0f;

        // Only run the loop if there is a duration to fade over
        if (duration > 0)
        {
            while (progress < 1f)
            {
                progress += Time.unscaledDeltaTime / duration;

                // Using a 'Slow Start' Quintic Easing for an even gentler dip at the beginning.
                // This ensures the transition starts almost imperceptibly.
                float t = 1f - Mathf.Pow(1f - progress, 3f); // Cubic ease out for volume reduction
                
                float v = Mathf.Lerp(startVol, 0f, t);
                audioSource.volume = v * v * v * v;

                yield return null;
            }
        }

        sfx.fadeData.fadingOut = false;

        audioSource.volume = 0f;
        audioSource.Stop();

        activeRoutine = null;
    }

    // =========================
    // FADE RUNTIME
    // =========================

    IEnumerator FadeRoutine(
        SoundFX sfx,
        float soundStartTime = 0f,
        float startingVolume = 0f)
    {
        float elapsed = 0f;
        // Significantly increased catch-up duration for a very slow, smooth entry
        float catchUpDuration = 1.5f;

        // Convert startingVolume to perceptual space (quartic root)
        float startingPerceptualVol = Mathf.Pow(startingVolume, 0.25f);

        Debug.Log("Starting fade routine");
        while (true)
        {
            elapsed += Time.unscaledDeltaTime;
            float currentTime = elapsed + soundStartTime;

            // 1. Calculate the 'Timeline Volume' (where the sound SHOULD be)
            float timelineVol = tempTargetVolume;

            // FADE IN calculation
            if (sfx.fadeData.useFadeIn)
            {
                if (currentTime < sfx.fadeData.fadeInRange.y)
                {
                    sfx.fadeData.fadingIn = true;
                    float t = Mathf.InverseLerp(
                        sfx.fadeData.fadeInRange.x,
                        sfx.fadeData.fadeInRange.y,
                        currentTime);
                    // Using SmoothStep for the timeline target
                    timelineVol = Mathf.SmoothStep(0f, 1f, t) * tempTargetVolume;
                }
                else
                {
                    Debug.Log("Fade in complete");
                    sfx.fadeData.fadingIn = false;
                }
            }

            // FADE OUT (non-loop) calculation
            if (!sfx.loop && sfx.fadeData.useFadeOut)
            {
                if (currentTime > sfx.fadeData.fadeOutRange.x)
                {
                    sfx.fadeData.fadingOut = true;
                    float fadeOutT = Mathf.InverseLerp(
                        sfx.fadeData.fadeOutRange.x,
                        sfx.fadeData.fadeOutRange.y,
                        currentTime);
                    timelineVol = Mathf.Min(timelineVol, Mathf.SmoothStep(tempTargetVolume, 0f, fadeOutT));
                }
            }

            // 2. Smoothly transition from startingVolume to the timelineVol
            // Using a quintic-in easing for the catch-up to make the start extremely slow
            float catchUpProgress = catchUpDuration > 0 ? Mathf.Clamp01(elapsed / catchUpDuration) : 1f;
            float t_catchup = catchUpProgress * catchUpProgress * catchUpProgress; // Cubic Ease In
            
            float targetRawVol = Mathf.Lerp(startingPerceptualVol, timelineVol, t_catchup);
            
            // Quartic volume curve
            audioSource.volume = targetRawVol * targetRawVol * targetRawVol * targetRawVol;

            // Termination conditions
            if (!sfx.loop && sfx.fadeData.useFadeOut && currentTime >= sfx.fadeData.fadeOutRange.y)
            {
                sfx.fadeData.fadingOut = false;
                audioSource.Stop();
                activeRoutine = null;
                yield break;
            }

            if (audioSource.clip != null && !sfx.loop && currentTime >= audioSource.clip.length)
            {
                activeRoutine = null;
                yield break;
            }

            yield return null;
        }
    }

    // =========================
    // VALIDATION
    // =========================

    void OnValidate()
    {
        PopulateSounds();
    }

    [System.Serializable]
    public struct FadeData
    {
        public bool useFadeIn;

        [ReadOnly] public bool fadingIn;

        [ShowIf("useFadeIn")]
        [MinMaxSlider(0, 60, true)]
        public Vector2 fadeInRange;

        public bool useFadeOut;

        [ReadOnly] public bool fadingOut;

        [ShowIf("useFadeOut")]
        [MinMaxSlider(0, 60, true)]
        public Vector2 fadeOutRange;
    }
}