using System.Collections;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerInstance : MonoBehaviour
{
    [BoxGroup("RotComposer")] float lookaheadTimeCache;
    [BoxGroup("RotComposer")] public CinemachineRotationComposer rotComposer;
    [BoxGroup("RotComposer")] public WaitSeconds disableWaitSeconds = new WaitSeconds { time = 0.5f };
    [BoxGroup("RotComposer")] public WaitSecondsRealtime smoothLookahead = new WaitSecondsRealtime { time = 0.01f };


    [BoxGroup("WeatherEffects")] [Required] public ParticleSystem rainPS;
    [BoxGroup("WeatherEffects")] public float originalRateOverTime;
    [BoxGroup("WeatherEffects")] public float rainBuildDuration = 5f;
    Coroutine rainBuildingCoroutine;

    [BoxGroup("WeatherEffects")] [Required] public ParticleSystem fogPS;
    [BoxGroup("WeatherEffects")] public float fogDensity; // alpha of the PS's start color
    
    [BoxGroup("PlayerEvents")] [Required] public PlayerEvents playerEvents;
    
    [BoxGroup("Input")] [Required] public InputReader_TopDown inputReader;
    
    void Awake()
    {
        // For RotComposer
        lookaheadTimeCache = rotComposer.Lookahead.Time;
        
        // WeatherEffects
        var emission = rainPS.emission;
        originalRateOverTime = emission.rateOverTime.constant;
        //emission.rateOverTime = 0;
    }

    #region ------------------- RotComposer -------------------

    public void DisableEnableRotComposer()
    {
        StopCoroutine(C_DisableEnableRotComposer());
        StartCoroutine(C_DisableEnableRotComposer());
    }
    
    IEnumerator C_DisableEnableRotComposer()
    {
        rotComposer.Lookahead.Enabled = false;
        yield return disableWaitSeconds.Delay;
        rotComposer.Lookahead.Enabled = true;
        rotComposer.Lookahead.Time = 0;
        while (rotComposer.Lookahead.Time < lookaheadTimeCache)
        {
            rotComposer.Lookahead.Time += 0.0005f;
            yield return smoothLookahead.Delay;
        }
    }

    #endregion

    #region ------------------- Weather -------------------

    public void ToggleFog(bool v, float fogDensity = 0.2f)
    {
        if (v)
        {
            fogPS.Play();
            fogPS.startColor = new Color(1, 1, 1, fogDensity);
        }
        else
            fogPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void ToggleRain(bool v)
    {
        Debug.Log("A");
        //if (rainBuildingCoroutine != null) StopCoroutine(rainBuildingCoroutine);
        Debug.Log("B");
        if (v)
        {
            Debug.Log("C");
            rainPS.Play();
            Debug.Log("D");
            //rainBuildingCoroutine = StartCoroutine(C_RainBuilding());
            Debug.Log("E");
        }
        else
        {
            rainPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            //var emission = rainPS.emission;
            //emission.rateOverTime = 0;
        }
    }

    IEnumerator C_RainBuilding()
    {
        Debug.Log("F");
        var emission = rainPS.emission;
        float elapsed = 0;

        while (elapsed < rainBuildDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / rainBuildDuration;
            emission.rateOverTime = Mathf.Lerp(0, originalRateOverTime, normalizedTime);
            yield return null;
        }

        emission.rateOverTime = originalRateOverTime;
    }
    
    #endregion

    public void CallPlayerEvent(PlayerEvents.PlayerEvent playerEvent)
    {
        switch (playerEvent)
        {
            case PlayerEvents.PlayerEvent.NewPlayerSpawn:
                playerEvents.NewPLayerSpawnEvent(); break;
            
        }
    }

    public void ToggleInputReading(bool v) => inputReader.enabled = v;
}
