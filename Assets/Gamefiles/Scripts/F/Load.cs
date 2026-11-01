using UnityEngine;


/// <summary>
/// Called when wanting to load out of this scene, into a new one
/// </summary>
[CreateAssetMenu(fileName = "Load Out", menuName = "ARPG/SO/Method/Load Out")]
public class Load : SO_MethodVTable
{

    public void LoadFade(int index)
    {
        Debug.Log("Starting LoadFade to index: " + index);
        Loader.Instance.LoadSceneFadeScreenToOpaque(PlayerScreenFade.Instance.fadeTarg, index);
    }
    public void LoadNoFade(int index) => Loader.Instance.LoadSceneImmediate(index);
}