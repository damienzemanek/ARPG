using UnityEngine;

[CreateAssetMenu(fileName = "IsMouseOver", menuName = "ARPG/InteractCriteria/MouseOver")]
public class InteractCriteria_MouseOver : InteractCriteria
{
    public override bool IsSatisfied(InteractContext context)
    {
        var cam = Camera.main;
        var mousePos = Input.mousePosition;

        Ray ray = cam.ScreenPointToRay(mousePos);
        Physics.Raycast(ray, out RaycastHit hit);
        return hit.transform.IsChildOf(context.interactable.transform);
    }
}