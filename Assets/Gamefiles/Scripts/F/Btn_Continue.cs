using UnityEngine;

public class Btn_Continue : MonoBehaviour
{
    public Load loaderSo;
    public void BtnContinuePress()
    {
        if (SessionData.Instance.currentPlayerEvent == PlayerEvents.PlayerEvent.Respawn
        || SessionData.Instance.currentPlayerEvent == PlayerEvents.PlayerEvent.MostRecentPosition)
            loaderSo.LoadFade(1);
    }
}
