using System;
using UnityEngine;

[Serializable]
public abstract class MarkStrategy : ScriptableObject
{
    public string markStratDescDetail;
    public abstract void ResolveMarkStrategy(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx);
}