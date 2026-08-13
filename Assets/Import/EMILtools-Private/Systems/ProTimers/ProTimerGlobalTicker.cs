using UnityEngine;

namespace ProTimers
{
 
    // public class ProTimerGlobalTicker : MonoBehaviour
    // {
    //     void Update() => TimerStack.TickActives();
    // }
    //
    // public static class ProTimerUtility
    // {
    //     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //     public static void InitTickers()
    //     {
    //         if(Object.FindAnyObjectByType(typeof(ProTimerGlobalTicker))) return;
    //         var ticker = new GameObject("ProTimerGlobalTicker").AddComponent<ProTimerGlobalTicker>();
    //         Object.DontDestroyOnLoad(ticker.gameObject);
    //         ticker.gameObject.hideFlags = HideFlags.DontSave;
    //     }
    // }
    
}