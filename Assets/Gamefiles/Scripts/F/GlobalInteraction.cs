using UnityEngine;

//[CreateAssetMenu(fileName = "New Global Interaction", menuName = "ARPG/SO/Global Interaction")]
public abstract class GlobalInteraction : ScriptableObject
{
    public abstract void Invoke();
}