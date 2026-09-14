using EMILtools.Extensions;
using UnityEngine;

/// <summary>
/// Called when loading into a new scene
/// </summary>
[CreateAssetMenu(fileName = "Load In", menuName = "ARPG/SO/Method/Load In")]
public class LoadIn : SO_MethodVTable
{
    public void FadeInScreen()
    {
        Object targ = PlayerScreenFade.Instance.fadeTarg;
        CoroutineRunner.Instance.StartCoroutine(
            FadeEX.C_FadeToTransparent(Loader.Instance.loadingFade, targ));
    }
}