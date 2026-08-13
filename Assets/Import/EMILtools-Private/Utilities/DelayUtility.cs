using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class DelayUtility 
{
    public static async Task Delay(Action method, float delay, CancellationToken token = default)
    {
        if (method == null) return;
        if (delay < 0) delay = 0;

        try
        {
            int ms = (int)(delay * 1000f);

            await Awaitable.WaitForSecondsAsync(delay, token);

            if (!token.IsCancellationRequested)
                method?.Invoke();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Debug.LogException(ex); }
    }
}

