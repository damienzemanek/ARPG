using System;
using System.Collections;
using System.Collections.Generic;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Utilities
{
    [Serializable]
    public struct Deviatable
    {
        public bool deviate;
        [SerializeField, ShowIf("@!deviate")] float _value;
        [SerializeField, ShowIf("deviate")] Vector2 _valueBetween;

        public float value
        {
            get
            {
                if (deviate) return rand;
                else return _value;
            }
        }

        public float floatvalue { get => _value;  set => _value = value; }
        public Vector2 Vec2value { get => _valueBetween;  set => _valueBetween = value; }

        float rand { get => _valueBetween.Rand(); }
        public float min { get => _valueBetween.x; }
        public float max { get => _valueBetween.y; }
        public Vector2 SetValue(Vector2 _val) => _valueBetween = _val;
        public static implicit operator float(Deviatable d) => d.value;
    }    
}


