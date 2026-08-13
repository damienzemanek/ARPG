using System;
using UnityEngine;


namespace EMILtools.Systems
{
    public interface IInputMap { }
    
    [Serializable]
    public abstract class InputMap : IInputMap
    {
        [SerializeField] public string ownerName;
        protected InputMap() { }
        protected InputMap(string ownerName) => this.ownerName = ownerName;
    }
}
