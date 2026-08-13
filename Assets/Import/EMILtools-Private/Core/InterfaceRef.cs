using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
[InlineProperty]
public class InterfaceRef<TInterface, TObject> where TObject : Object where TInterface : class
{
    // Member variable that stores the value
    [SerializeField, InlineProperty, HideLabel] TObject underlyingValue;
    
    // Provide access to the TInterface
    public TInterface Value
    {
        get => underlyingValue switch
        {
            null => null,
            TInterface @interface => @interface,
            _ => throw new InvalidOperationException($"{typeof(TInterface).Name} needs to be typeof {typeof(TObject)}")
        };
        set => underlyingValue = value switch
        {
            null => null,
            TObject newValue => newValue,
            _ => throw new ArgumentException($"{value} needs to be typeof {typeof(TObject)}")
        };

    }
    
    // Provide access to the underlying variable TObject
    public TObject UnderlyingValue
    {
        get => underlyingValue;
        set => underlyingValue = value;
    }
    
    // Ctor Default (Empty)
    public InterfaceRef() { }
    
    // Ctor TObject
    public InterfaceRef(TObject newValue) => underlyingValue = newValue;
    
    // Ctor TInterface
    public InterfaceRef(TInterface newValue) => Value = newValue;
    
    public static implicit operator TInterface(InterfaceRef<TInterface, TObject> rv) => rv.Value;
}

[Serializable]
public class InterfaceRef<TInterface> : InterfaceRef<TInterface, Object> where TInterface : class
{
    public InterfaceRef() { }
    public InterfaceRef(TInterface newValue) : base(newValue) { }
}
