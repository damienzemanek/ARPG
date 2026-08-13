using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderAlteration : MonoBehaviour
{
    [SerializeField] public MaterialSetter mats;

    private void Start()
    {
        mats.SetRandMat();
    }
}
