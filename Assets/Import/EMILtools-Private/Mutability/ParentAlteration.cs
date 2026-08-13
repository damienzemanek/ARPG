using System;
using UnityEngine;

public class ParentAlteration : MonoBehaviour
{
    public bool UnparentOnStart = false;

    private void Start()
    {
        if (UnparentOnStart) transform.parent = null;
    }
}
