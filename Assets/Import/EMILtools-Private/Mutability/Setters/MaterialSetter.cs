using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using EMILtools.Extensions;
using UnityEngine;

[Serializable]
public struct MaterialSetter
{
    [SerializeField] Material[] mats;
    [SerializeField] public Renderer obj;

    public void SetRandMat()
    {
        obj.material = mats.Rand();
    }

    public void SetMat(Material mat)
    {
        obj.material = mat;
    }

    public void SetMatToIndex(int indx)
    {
        obj.material = mats[indx];
    }
}
