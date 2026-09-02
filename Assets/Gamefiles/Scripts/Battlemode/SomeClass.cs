using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class SomeClass
{
    // Declaration Order
    
    // Private Properties
    private bool someProperty => intVariable1 == 1;

    // Enum Declarations
    public enum SomeEnum
    {
        First,
        Second,
    }

    // Class & struct sub-declarations
    public class SomeSubClass
    {
        
    }

    public struct SomeSubStruct
    {
        
    }

    // public & SerializeField variables
    public int intVariable1;
    [SerializeField] int intVariable2;
    
    // ReadOnly variables & public properties
    [ReadOnly] public int intVariable3;
    public bool someOtherProperty => intVariable1 == 2;
    
    // Private Variables
    int intVariable4;
    int intVariable5;
}


