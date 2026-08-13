using UnityEngine;

public class LoadSceneConnector : MonoBehaviour
{
    public Object fadeTarget;
    
    public void Load(int sceneIndx)
    {
        Debug.Log("Attempting to load new level: " + sceneIndx);

        Loader loader = FindAnyObjectByType<Loader>();
        if (loader != null)
        {
            loader.LoadSceneFadeScreenToOpaque(fadeTarget, sceneIndx);
        }
        else
        {
            Debug.LogError($"LoadSceneConnector on {name} failed: No LoadScene component found in the scene to handle loading scene index {sceneIndx}.");
        }
    }
}
