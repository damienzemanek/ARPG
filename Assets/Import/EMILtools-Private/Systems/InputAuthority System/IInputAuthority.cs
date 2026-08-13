using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Systems
{
    public interface IInputAuthority<TInputMap, TSubordinateEnum>
        where TSubordinateEnum : Enum
        where TInputMap : class, IInputMap, new()
    {
        public TInputMap currentInputMap => subordinate != null ? subordinate.Input : null;
        public TSubordinateEnum currentSubordinate => subordinate != null ? subordinate.inputSubordinateContext.key : default;
        IInputSubordinate<TInputMap, TSubordinateEnum> subordinate { get; set; }
    
        /// <summary>
        /// For now Requests are defaulted to TRUE until I need to develop it further
        /// (I don't want to sink endless time into a system I donte need yet)
        /// </summary>
        /// <param name="subordinate"></param>
        /// <returns></returns>
        public bool ConsiderRequest(IInputSubordinate<TInputMap, TSubordinateEnum> subordinate)
        {
            this.subordinate = subordinate;
            ReceiveRequest(subordinate);
            return true;
        }
        protected abstract void ReceiveRequest(IInputSubordinate<TInputMap, TSubordinateEnum> subordinate);
    
    }
}




