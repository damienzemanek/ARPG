using UnityEngine;

public abstract class InteractCriteria : ScriptableObject
{
    public struct InteractContext(object sender, Interactable interactable)
    {
        public GameObject sender;
        public GameObject interactable;
    }
    public abstract bool IsSatisfied(InteractContext context);
}