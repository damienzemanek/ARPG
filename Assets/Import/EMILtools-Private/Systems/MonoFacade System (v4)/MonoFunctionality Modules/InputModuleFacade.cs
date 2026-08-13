// using System;
// using System.Collections.Generic;
// using EMILtools.Core;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.InputSystem;
//
//
//
// public abstract class InputHeldModuleFacade<TPublisherArgs, TCoreFacade> : InputHeldModule<TPublisherArgs>
//     where TCoreFacade : class, IFacade
// {
//     [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }
//
//     protected InputHeldModuleFacade(PersistentAction<TPublisherArgs, bool> action, TCoreFacade facade, bool useIsActiveGuard)
//         : base(action, useIsActiveGuard: useIsActiveGuard)
//     => this.facade = facade;
// }
//
// public abstract class InputHeldModuleFacade<TCoreFacade> : InputHeldModule
//     where TCoreFacade : class, IFacade
// {
//     [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }
//
//     protected InputHeldModuleFacade(PersistentAction<bool> action, TCoreFacade facade ) : base(action, true)
//         => this.facade = facade;
//     protected InputHeldModuleFacade(PersistentAction<bool> action, TCoreFacade facade, bool useIsActiveGuard) : base(action, useIsActiveGuard)
//         => this.facade = facade;
// }
//
//
// /// <summary>
// /// Not Guarded
// /// No Args
// /// </summary>
// /// <typeparam name="TCoreFacade"></typeparam>
// public abstract class InputPressedModuleUGFacade<TCoreFacade> : InputPressedModule
//     where TCoreFacade : class, IFacade
// {
//     [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }
//
//     protected InputPressedModuleUGFacade(PersistentAction action, TCoreFacade facade)
//         : base(action)
//         => this.facade = facade;
// }
//
//
// /// <summary>
// /// Guarded
// /// No Args
// /// </summary>
// /// <typeparam name="TSetActionGuarder"></typeparam>
// /// <typeparam name="TCoreFacade"></typeparam>
// public abstract class InputPressedModuleFacade<TCoreFacade> : InputPressedModule
//     where TCoreFacade : class, IFacade
// {
//     [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }
//
//     protected InputPressedModuleFacade(PersistentAction action, TCoreFacade facade)
//         : base(action)
//     => this.facade = facade;
// }
//
// /// <summary>
// /// Guarded
// /// 1 Args
// /// </summary>
// /// <typeparam name="T"></typeparam>
// /// <typeparam name="TSetActionGuarder"></typeparam>
// /// <typeparam name="TCoreFacade"></typeparam>
// public abstract class InputPressedModuleFacade<T, TCoreFacade> : InputPressedModule<T>
//     where TCoreFacade : class, IFacade
// {
//     [field:ReadOnly] [field:ShowInInspector] [field:NonSerialized] protected TCoreFacade facade { get; set; }
//
//     protected InputPressedModuleFacade(PersistentAction<T> action, TCoreFacade facade)
//         : base(action)
//         => this.facade = facade;
// }
//
//
