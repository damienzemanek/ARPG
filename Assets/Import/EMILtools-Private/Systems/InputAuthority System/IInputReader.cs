using System;
using UnityEngine;

namespace EMILtools.Systems
{
    public interface IInputReader<TInputMap> : IInitializable
        where TInputMap : class, IInputMap, new()
    {
        
    }


    public interface IInputReaderSubordinate<TInputMap, TSubordinateEnum> : IInputReader<TInputMap>
        where TSubordinateEnum : Enum
        where TInputMap : class, IInputMap, new()
    {
        public TInputMap Input { get; }
        public IInputSubordinate<TInputMap, TSubordinateEnum> subordinate { get; set; }

        public abstract void OnAuthorityChange();
    }
}
