using UnityEngine;

public static class CloneUtility
{
    public static T DeepClone<T>(this T obj)
    {
        return JsonUtility.FromJson<T>(JsonUtility.ToJson(obj));
    }

    public static object DeepClonePolymorph(object obj)
    {
        return obj == null ? null : JsonUtility.FromJson(JsonUtility.ToJson(obj), obj.GetType());
    }
}
