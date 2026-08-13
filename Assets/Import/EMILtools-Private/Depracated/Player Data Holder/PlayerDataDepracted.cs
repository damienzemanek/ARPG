using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using static EMILtools.Extensions.PhysEX;

[CreateAssetMenu(fileName = "New Player Data", menuName = "EMILtools/Deprecated/ScriptableObjects/Player Data")]
public class PlayerDataDepracted : ScriptableObject
{
    [TitleGroup("Physics")]
    public float fallDrag;
    public float groundedDrag;
    public JumpSettings jump;
    public MoveSettings move;
}
