using System;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskEX
{
    public static async void Forget(this Task task, string tag)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            Debug.LogException(new Exception($"[FireAndForget:{tag}]", ex));
        }
    }
    
}