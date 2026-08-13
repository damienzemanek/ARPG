using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using static EMILtools.Extensions.PhysEX;

[Serializable]
public class AugmentPhysEX : MonoBehaviour
{
    public FallSettings2D fallSettings;

    public bool fallFaster = true;

    [BoxGroup("Rigidbody")] [SerializeField, Required] Rigidbody2D rb;
    [BoxGroup("ReadOnly")] [ReadOnly] public ReactiveIntercept<bool> isGrounded;

    void FixedUpdate()
    {
        if(!isGrounded && fallFaster) rb.FallFaster2D(fallSettings);
    }
}
