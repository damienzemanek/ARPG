using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public interface IPositionModule
{
    public bool active { get; set; }
    public void Execute();
}
public class PositionModule : MonoBehaviour
{
    
    [Serializable]
    public struct Constrain : IPositionModule
    {
        [field: SerializeField] public bool active { get; set; } 
        public Transform transform;
        public bool local;
        public bool x, y, z;
        [ShowIf("x")] public float constraintX;
        [ShowIf("y")] public float constraintY;
        [ShowIf("z")] public float constraintZ;
        public void Execute()
        {
            Vector3 position = transform.position;
            if (x) position.x = constraintX;
            if (y) position.y = constraintY;
            if (z) position.z = constraintZ;
            if(!local) transform.position = position;
            else transform.localPosition = position;
        }
    }
    
    [Serializable]
    public struct ForceMaintainPosition : IPositionModule
    {
        [field: SerializeField] public bool active { get; set; }
        public Transform transform;
        public bool local;
        [ReadOnly] public Vector3 savedPos;

        public void Setup()
        {
            if(!local) savedPos = transform.position;
            else savedPos = transform.localPosition;
        }

        public void Execute()
        {
            if(!local) transform.position = savedPos;
            else transform.localPosition = savedPos;
        }
    }

    public Constrain constrain;
    public ForceMaintainPosition forceMaintainPosition;

    void Start()
    {
        if(forceMaintainPosition.active) forceMaintainPosition.Setup();
    }

    public void Update()
    {
        if(constrain.active) constrain.Execute();
        if(forceMaintainPosition.active) forceMaintainPosition.Execute();
    }
}
