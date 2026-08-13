using System;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Systems
{
    
    public interface IInputSubordinate<TInputMap, TSubordnateEnumType>
        where TInputMap : class, IInputMap, new()
        where TSubordnateEnumType : Enum
    {
        /// <summary>
        /// Used to store a subordinate's (1) Authority and (2) Current Delegate Enum State
        /// </summary>
        [Serializable] 
        public class SubordinateContext
        {
            [SerializeField] [InlineProperty]
            public InterfaceRef<IInputSubordinate<TInputMap, TSubordnateEnumType>, MonoBehaviour> Subordinate;
            [SerializeField, ReadOnly] [InlineProperty]
            public InterfaceRef<IInputAuthority<TInputMap, TSubordnateEnumType>, MonoBehaviour> Authority;
            [SerializeField] public TSubordnateEnumType key;
        }
        
        public IInputAuthority<TInputMap, TSubordnateEnumType> Authority => inputSubordinateContext.Authority.Value;
        
        public TInputMap Input { get; set; }
        public SubordinateContext inputSubordinateContext { get; set; }  // Ensure this is Serialized
        public abstract TInputMap InjectInputMap();
        public abstract void InitSubordinate();
        public abstract void OnAuthorityReceived();
        public abstract void OnAuthorityLost();
        
        
        /// <summary>a
        ///  Delegation of Authority
        /// </summary>
        /// <param name="inputMap"></param>
        bool SendRequest()
        {
            if(inputSubordinateContext.Subordinate.Value == null) Debug.LogError("Did not set self as Subordinate");
            //Retrive the Input map stored in the Subordinate
            if (inputSubordinateContext.Subordinate.Value.Input == null)
            {
                inputSubordinateContext.Subordinate.Value.Input = inputSubordinateContext.Subordinate.Value.InjectInputMap();
                inputSubordinateContext.Subordinate.Value.InitSubordinate();
            }
                
            // Register that InputMap with the Authority
            bool accepted = inputSubordinateContext.Authority.Value.ConsiderRequest(inputSubordinateContext.Subordinate.Value);
            if (accepted) inputSubordinateContext.Subordinate.Value.OnAuthorityReceived();
            return accepted;
        }
        
        public void SetupFirstAuthority(IInputAuthority<TInputMap, TSubordnateEnumType> authority)
        {
            inputSubordinateContext.Authority.Value = authority;
            SendRequest();
        }
        
        public bool RequestAuthorityFrom(IInputSubordinate<TInputMap, TSubordnateEnumType> former)
        {
            IInputAuthority<TInputMap, TSubordnateEnumType> formerAuthority = former.Authority;
            inputSubordinateContext.Authority.Value = former.Authority;
            bool successful = SendRequest();
            if(successful) former.OnAuthorityLost();
            else inputSubordinateContext.Authority.Value = formerAuthority;
            return successful;
        }

    }

}
