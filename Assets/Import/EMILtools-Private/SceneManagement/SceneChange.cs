using UnityEngine;
using UnityEngine.SceneManagement;
using EMILtools.Extensions;

public class SceneChange : MonoBehaviour
{

    #region Privates
    
    [SerializeField] FadeSettings fade;
    #endregion

    public void ChangeSceneImmediate(int num)
    {
        SceneManager.LoadScene(num);
    }

    public void ChangeScreenFadeScreenToOpaque(int num)
    {
        StartCoroutine(FadeEX.C_FadeToOpaque(fade, this, () => ChangeSceneImmediate(num)));
    }


    #region Methods
        
    #endregion

}
