// using System;
// using EMILtools.Core;
// using Sirenix.OdinInspector;
//
//
//
// /// <summary>
// /// Inheritance Facade version of BasicFunctionalityModule
// /// 1 Args
// /// </summary>
// /// <typeparam name="T"></typeparam>
// /// <typeparam name="TCoreFacade"></typeparam>
// /// <typeparam name="TExecuteGuarder"></typeparam>
// public abstract class BasicFunctionalityModuleFacade<T, TCoreFacade> : BasicFunctionalityModule<T>
//     where TCoreFacade : class, IFacade
// {
//     [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }
//
//     protected BasicFunctionalityModuleFacade(PersistentAction<T> action, TCoreFacade facade, bool initGuarder) : base(action, initGuarder) 
//         => this.facade = facade;
// }
//
//
// /// <summary>
// /// Inheritance Facade version of BasicFunctionalityModule
// /// No Args
// /// </summary>
// /// <typeparam name="TCoreFacade"></typeparam>
// public abstract class BasicFunctionalityModuleFacade<TCoreFacade> : BasicFunctionalityModule
//     where TCoreFacade : class, IFacade
// {
//     [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }
//
//     protected BasicFunctionalityModuleFacade(PersistentAction action, TCoreFacade facade, bool initGuarder) : base(action, initGuarder) 
//         => this.facade = facade;
// }
//
