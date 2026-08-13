using System;
using System.Collections.Generic;
using EMILtools.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Systems
{
    [Serializable]
    public abstract class InputAuthority<TInputReader, TInputMap, TSubordinateEnum> : MonoBehaviour, 
        IInputAuthority<TInputMap, TSubordinateEnum>
        where TInputMap : class, IInputMap, new()
        where TInputReader : IInputReaderSubordinate<TInputMap, TSubordinateEnum>, new()
        where TSubordinateEnum : Enum
    {
        [SerializeField, Required] protected TInputReader Reader;
        [ShowInInspector, ReadOnly] public IInputSubordinate<TInputMap, TSubordinateEnum> subordinate { get; set; }
        [ShowInInspector, ReadOnly] bool initializedReader = false;
     
        [FoldoutGroup("Presetting & Initial Subordinate Settings")] [SerializeField] protected bool presetWithInitialSubordinate;
        [FoldoutGroup("Presetting & Initial Subordinate Settings")] [ShowIf("presetWithInitialSubordinate")] public InterfaceRef<IInputSubordinate<TInputMap, TSubordinateEnum>, MonoBehaviour> InitialSubordinate;


        protected virtual void Awake()
        {
            Reader = new TInputReader();
            if(InitialSubordinate.Value == null) 
                Debug.LogError($"Input Authority Initial Subordinate is null! Please assign initial subordinate in InputAuthority {gameObject.name}");
            if (presetWithInitialSubordinate) InitialSubordinate.Value.SetupFirstAuthority(this);
        }
      
        void IInputAuthority<TInputMap, TSubordinateEnum>.ReceiveRequest(IInputSubordinate<TInputMap, TSubordinateEnum> subordinate)
        {
            Reader.subordinate = subordinate;
          
            if(!initializedReader) 
            {
                Reader.Init();
                if (Reader is IActionsAware aware && subordinate is IFacade facade)
                    aware.InjectActions(facade.GetActions);

                initializedReader = true; 
            }
          
            Reader.OnAuthorityChange();
        }

      
      
    
    }
}
