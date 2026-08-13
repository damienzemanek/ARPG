using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[InlineProperty]
[HideReferenceObjectPicker]
public class ClassRef<T> where T : struct
{
    [HideLabel, InlineProperty] public T val;
    public virtual ref T ValueRef => ref val;
    public ClassRef(T initialValue) => val = initialValue;
    public ClassRef(ref T initialValue) => val = initialValue;
    public static implicit operator T(ClassRef<T> r) => (r != null) ? r.val : default;
    public static implicit operator ClassRef<T>(T val) => new ClassRef<T>(val);

    public void Set(T val) => this.val = val;
    public ClassRef<T> SetReturnThis(T val)
    {
        this.val = val;
        return this;
    }
}