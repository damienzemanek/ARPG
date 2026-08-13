// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always 
// nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)
// Edited by EMIL

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;


[Serializable, InlineProperty]
public struct TypeSerialized<TLookingForType> 
{
    TLookingForType lookingFor;
    [ShowInInspector, ReadOnly] [SerializeField] TypeSerializedCore _type;
    
    [ShowInInspector, HideLabel] public System.Type Type => _type.SystemType;
    public TLookingForType LookingForType => lookingFor;

    
    [Button] public void SetType([ValueDropdown("GetLookingForTypes"), HideLabel] System.Type type) => _type = new TypeSerializedCore(type);
    
    static IEnumerable<Type> GetLookingForTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(TLookingForType).IsAssignableFrom(t) &&
                !t.IsAbstract);
    }
    
    public TypeSerialized(System.Type type) => _type = new TypeSerializedCore(type);
}

public class NoType { }

[System.Serializable]
public class TypeSerializedCore
{
    [SerializeField] string m_Name;
    public string Name => m_Name;

    [SerializeField] string m_AssemblyQualifiedName;
    public string AssemblyQualifiedName => m_AssemblyQualifiedName;

    [SerializeField] string m_AssemblyName;
    public string AssemblyName => m_AssemblyName;

    System.Type m_SystemType;	
    public System.Type SystemType
    {
        get
        {
            if (m_SystemType == null)
                GetSystemType();

            return m_SystemType ?? typeof(NoType);
        }
    }

    void GetSystemType()
    {
        m_SystemType = string.IsNullOrEmpty(m_AssemblyQualifiedName)
            ? null
            : System.Type.GetType(m_AssemblyQualifiedName);
    }

    public TypeSerializedCore(System.Type _SystemType)
    {
        if (_SystemType == null)
        {
            m_SystemType = typeof(NoType);
            m_Name = nameof(NoType);
            return;
        }

        m_SystemType = _SystemType;
        m_Name = _SystemType.Name;
        m_AssemblyQualifiedName = _SystemType.AssemblyQualifiedName;
        m_AssemblyName = _SystemType.Assembly.FullName;
    }
	
    public override bool Equals( System.Object obj )
    {
        TypeSerializedCore temp = obj as TypeSerializedCore;
        if ((object)temp == null) return false;
        return this.Equals(temp);
    }
    //return m_AssemblyQualifiedName.Equals(_Object.m_AssemblyQualifiedName);
    public bool Equals(TypeSerializedCore _Object)
    {
        if (_Object == null) return false;

        return _Object.SystemType == SystemType;
    }
    public static bool operator ==( TypeSerializedCore a, TypeSerializedCore b )
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(a, b)) return true;
	
        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null)) return false;
        
        return a.Equals(b);
    }
	
    public static bool operator !=( TypeSerializedCore a, TypeSerializedCore b )
        => !(a == b);
    
    public static implicit operator TypeSerializedCore(System.Type type) 
        => type != null ? new TypeSerializedCore(type) : null;
}