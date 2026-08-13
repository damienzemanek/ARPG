using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct EnumTagged<TEnum, TObject>
    where TEnum : Enum
{
    [FormerlySerializedAs("label")] public TEnum tag;
    public TObject obj;
}


public static class EnumTaggedEX
{
    public static EnumTagged<TEnum, TObject> GetInArray<TEnum, TObject>(this EnumTagged<TEnum, TObject>[] arr, TEnum tag)
        where TEnum : Enum
    {
        foreach (var item in arr)
        {
            if (item.tag.Equals(tag)) return item;
        }
        Debug.LogError("Tag not found in array");
        return default;
    }
}