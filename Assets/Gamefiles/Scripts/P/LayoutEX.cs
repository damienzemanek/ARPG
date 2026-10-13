using UnityEngine;
using UnityEngine.UI;

public static class LayoutEX
{
    public static void RefreshLayoutGroupsImmediateAndRecursive(this RectTransform root)
    {
        var children = root.GetComponentsInChildren<RectTransform>();
        for (int i = children.Length - 1; i >= 0; i--)
        {
            if(children[i].gameObject.activeInHierarchy) LayoutRebuilder.ForceRebuildLayoutImmediate(children[i]);
        }
    }
}
