using UnityEngine;

public class PauseOnMiddleClick : MonoBehaviour
{
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(3)) 
        {
            Debug.Break();
        }
#endif
    }
}