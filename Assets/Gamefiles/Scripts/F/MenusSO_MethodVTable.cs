using UnityEngine;

[CreateAssetMenu(fileName = "MenusSO Method VTable", menuName = "ARPG/SO/MenusSO Method VTable")]
public class MenusSO_MethodVTable : SO_MethodVTable
{
    public void QuitGame() => Application.Quit();
}