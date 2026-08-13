using System;
using UnityEngine;

[Serializable]
public struct Pose
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public Pose()
    {
        position = Vector3.zero;
        rotation = Vector3.zero;
        scale = Vector3.one;
    }
}
