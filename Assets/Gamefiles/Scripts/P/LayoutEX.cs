using UnityEngine;
using UnityEngine.UI;

public static class LayoutEX
{
    public static void RefreshLayoutGroupsImmediateAndRecursive(this RectTransform root)
    {
        foreach (var layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
        {
            if(layoutGroup.gameObject.activeInHierarchy)
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
    }
}
