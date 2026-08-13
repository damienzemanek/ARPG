using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : OdinEditor
{
    public override bool RequiresConstantRepaint() => true;
}