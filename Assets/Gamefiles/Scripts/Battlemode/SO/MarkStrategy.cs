using System;
using UnityEngine;

[Serializable]
public abstract class MarkStrategy : ScriptableObject
{
    public abstract void ResolveMarkStrategy(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx);
}