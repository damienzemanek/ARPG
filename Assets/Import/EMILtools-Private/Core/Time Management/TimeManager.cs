using System;
using System.Threading;
using System.Threading.Tasks;
using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using UnityEngine;

public class TimeManager : PersistantReplacerSingleton<TimeManager>
{
    CancellationTokenSource cts;

    float baseTimeScale = 1f;
    float baseFixedDelta = 0.02f;
    bool inHitStop;

    public async Task SlowTimeForSeconds(float seconds, float percentage, Action postEvent)
    {
        percentage = Mathf.Clamp01(percentage);

        cts?.Cancel();
        cts = new CancellationTokenSource();
        var token = cts.Token;

        if (!inHitStop)
        {
            baseTimeScale = Time.timeScale;
            baseFixedDelta = Time.fixedDeltaTime;
            inHitStop = true;
        }

        Time.timeScale = percentage;
        Time.fixedDeltaTime = baseFixedDelta * percentage;

        await WaitForSecondsRealtimeAsync(seconds, token);

        if (!token.IsCancellationRequested)
        {
            Time.timeScale = baseTimeScale;
            Time.fixedDeltaTime = baseFixedDelta;
            inHitStop = false;
            postEvent?.Invoke();
        }
    }

    async Task WaitForSecondsRealtimeAsync(float seconds, CancellationToken token)
    {
        float endTime = Time.realtimeSinceStartup + seconds; 
        while (Time.realtimeSinceStartup < endTime) 
        {
            if (token.IsCancellationRequested) return;
            await Task.Yield();
        }
    }
}
