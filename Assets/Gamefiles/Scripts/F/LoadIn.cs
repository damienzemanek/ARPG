using EMILtools.Extensions;
using UnityEngine;

/// <summary>
/// Called when loading into a new scene
/// </summary>
[CreateAssetMenu(fileName = "Load In", menuName = "ARPG/SO/Method/Load In")]
public class LoadIn : SO_Method<MonoBehaviour>
{
    public override void Invoke(MonoBehaviour host)
    {
        Object targ = PlayerScreenFade.Instance.fadeTarg;
        host.StartCoroutine(FadeEX.C_FadeToTransparent(Loader.Instance.loadingFade, targ));
    }
}