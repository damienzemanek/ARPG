using UnityEngine;

[CreateAssetMenu(fileName = "MenusSO Method VTable", menuName = "ARPG/SO/MenusSO Method VTable")]
public class MenusSO_MethodVTable : SO_MethodVTable
{
    public void QuitGame() => Application.Quit();

    public void Pause(bool val)
    {
        Time.timeScale = val ? 0f : 1f;
    }
}