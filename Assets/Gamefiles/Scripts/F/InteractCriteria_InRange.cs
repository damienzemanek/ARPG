using UnityEngine;

[CreateAssetMenu(fileName = "IsInRange", menuName = "ARPG/InteractCriteria/InRange")]
public class InteractCriteria_InRange : InteractCriteria
{
    public float range;
    public override bool IsSatisfied(InteractContext context)
    {
        var sqr = (context.sender.transform.position - context.interactable.transform.position).sqrMagnitude;
        return sqr <= range * range;
    }
}