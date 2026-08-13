using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class EmilEventBase
{
    [SerializeField]
    protected bool useUnityEvent;

    [SerializeField]
    protected bool useVTable;

    protected abstract UnityEvent GetUnityEvent();
    public UnityEvent UnityEvent => GetUnityEvent();

    [Serializable, InlineProperty]
    public abstract class ParameterBase
    {
        [HideInInspector] public string name;
        [HideInInspector] public bool isAutoSupplied;
        public abstract object GetValue();

        public static ParameterBase Create(string name, Type type, bool isAutoSupplied = false)
        {
            Type genericType = typeof(TypedParameter<>).MakeGenericType(type);
            var param = (ParameterBase)Activator.CreateInstance(genericType);
            param.name = name;
            param.isAutoSupplied = isAutoSupplied;
            return param;
        }
    }

    [Serializable]
    public class TypedParameter<T> : ParameterBase
    {
        [ReadOnly, ShowIf(nameof(isAutoSupplied))]
        [LabelText("@name + \" (Auto)\"")]
        public T autoValue;

        [HideIf(nameof(isAutoSupplied))]
        [LabelText("@name")]
        public T value;

        public override object GetValue() => isAutoSupplied ? autoValue : value;
    }

    [Serializable]
    public class VTableEntry
    {
        [Required] public SO_MethodVTable vTable;

        [OnValueChanged(nameof(UpdateParameters))]
        [ValueDropdown(nameof(GetMethodNames))]
        [ShowIf(nameof(vTable))]
        public string methodName;

        [ShowIf(nameof(HasParams))]
        [ListDrawerSettings(IsReadOnly = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [SerializeReference, HideReferenceObjectPicker, InlineProperty]
        public ParameterBase[] parameters;

        public Type[] suppliedTypes;

        [NonSerialized] private MethodInfo _cachedMethod;

        IEnumerable<string> GetMethodNames()
        {
            if (vTable == null) return Enumerable.Empty<string>();
            return vTable.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Select(m => m.Name);
        }

        bool HasParams() => !string.IsNullOrEmpty(methodName) && vTable != null;

        [OnValueChanged(nameof(UpdateParameters))]
        void OnMethodChanged() => UpdateParameters();

        public void UpdateParameters()
        {
            Debug.Log($"[EmilEvent VTable] UpdateParameters called. VTable={vTable}, Method={methodName}");
            _cachedMethod = null;
            if (vTable == null || string.IsNullOrEmpty(methodName))
            {
                parameters = Array.Empty<ParameterBase>();
                return;
            }

            var method = vTable.GetType().GetMethod(methodName);
            Debug.Log($"[EmilEvent VTable] Reflection lookup result={method}");
            if (method == null)
            {
                parameters = Array.Empty<ParameterBase>();
                return;
            }

            var methodParams = method.GetParameters();
            var newParams = new List<ParameterBase>();
            int suppliedIndex = 0;

            for (int i = 0; i < methodParams.Length; i++)
            {
                var p = methodParams[i];
                bool isAuto = false;

                if (suppliedTypes != null && suppliedIndex < suppliedTypes.Length && suppliedTypes[suppliedIndex].IsAssignableFrom(p.ParameterType))
                {
                    isAuto = true;
                    suppliedIndex++;
                }

                var existing = parameters?.FirstOrDefault(x => x != null && x.name == p.Name && x.isAutoSupplied == isAuto && x.GetValue() != null && p.ParameterType.IsAssignableFrom(x.GetValue().GetType()));
                if (existing != null)
                {
                    newParams.Add(existing);
                }
                else
                {
                    newParams.Add(ParameterBase.Create(p.Name, p.ParameterType, isAuto));
                }
            }
            parameters = newParams.ToArray();
        }

        public void Invoke(params object[] suppliedArgs)
        {
            Debug.Log($"[EmilEvent VTable] Invoke called. Method={methodName}, VTable={vTable}, SuppliedArgs={suppliedArgs?.Length ?? 0}");

            if (vTable == null)
            {
                Debug.LogError("[EmilEvent VTable] Missing vTable reference");
                return;
            }

            if (string.IsNullOrEmpty(methodName))
            {
                Debug.LogError("[EmilEvent VTable] Missing method name");
                return;
            }

            if (_cachedMethod == null || _cachedMethod.Name != methodName)
            {
                _cachedMethod = vTable.GetType().GetMethod(methodName);

                Debug.Log($"[EmilEvent VTable] Searching method {methodName}. Found={_cachedMethod != null}");
            }

            if (_cachedMethod == null)
            {
                Debug.LogError($"[EmilEvent VTable] Could not find {methodName} on {vTable.GetType()}");
                return;
            }


            var methodParams = _cachedMethod.GetParameters();

            Debug.Log($"[EmilEvent VTable] Method params count={methodParams.Length}");
            Debug.Log($"[EmilEvent VTable] Stored parameters count={parameters?.Length ?? 0}");


            object[] args = new object[methodParams.Length];

            int suppliedIdx = 0;

            for (int i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];

                Debug.Log(
                    $"[EmilEvent VTable] Param {i}: {param.Name} Type={param.ParameterType} Auto={(parameters != null && parameters.Length > i ? parameters[i].isAutoSupplied : false)}"
                );


                if (parameters[i].isAutoSupplied)
                {
                    if (suppliedArgs != null && suppliedIdx < suppliedArgs.Length)
                    {
                        args[i] = suppliedArgs[suppliedIdx++];
                        Debug.Log($"[EmilEvent VTable] Auto supplied {args[i]}");
                    }
                    else
                    {
                        Debug.LogWarning($"[EmilEvent VTable] Missing supplied argument for {param.Name}");
                        args[i] = null;
                    }
                }
                else
                {
                    args[i] = parameters[i].GetValue();
                    Debug.Log($"[EmilEvent VTable] Manual value {args[i]}");
                }
            }


            try
            {
                Debug.Log($"[EmilEvent VTable] Invoking {_cachedMethod.Name}");

                Debug.Log($"[EmilEvent VTable] Target: {vTable}");
                Debug.Log($"[EmilEvent VTable] Method: {_cachedMethod.DeclaringType.FullName}.{_cachedMethod.Name}");

                var invokeParams = _cachedMethod.GetParameters();

                for (int i = 0; i < args.Length; i++)
                {
                    var expectedType = invokeParams[i].ParameterType;
                    var value = args[i];

                    Debug.Log(
                        $"[EmilEvent VTable] Arg[{i}] " +
                        $"Name={invokeParams[i].Name} " +
                        $"ExpectedType={expectedType.FullName} " +
                        $"ActualType={(value != null ? value.GetType().FullName : "NULL")} " +
                        $"Value={value}"
                    );
                }

                try
                {
                    _cachedMethod.Invoke(vTable, args);
                    Debug.Log($"[EmilEvent VTable] Invoke successful");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EmilEvent VTable] Invoke failed:\n{e}");
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"[EmilEvent VTable] Invoke failed:\n{e}");
            }
        }
    }
}

[Serializable]
public class EmilEvent : EmilEventBase
{
    [SerializeField, ShowIf(nameof(useUnityEvent))]
    UnityEvent _unityEvent;

    protected override UnityEvent GetUnityEvent() => _unityEvent ??= new UnityEvent();

    [SerializeField, ShowIf(nameof(useVTable))]
    [ListDrawerSettings(CustomAddFunction = nameof(AddVTableEntry))]
    List<VTableEntry> vTableEntries = new List<VTableEntry>();

    public void Invoke(GameObject user)
    {
        Debug.Log($"[EmilEvent] Invoke(GameObject) called. user={user}");

        Debug.Log($"[EmilEvent] useUnityEvent={useUnityEvent}");
        Debug.Log($"[EmilEvent] useVTable={useVTable}");

        if (useUnityEvent)
        {
            Debug.Log($"[EmilEvent] UnityEvent listeners={_unityEvent?.GetPersistentEventCount() ?? 0}");

            if (_unityEvent != null)
                _unityEvent.Invoke();
        }


        if (!useVTable)
        {
            Debug.Log("[EmilEvent] VTable disabled");
            return;
        }


        Debug.Log($"[EmilEvent] VTable entries={vTableEntries.Count}");


        foreach (var entry in vTableEntries)
        {
            Debug.Log($"[EmilEvent] Calling VTable entry {entry.methodName}");

            entry.suppliedTypes = new[] { typeof(GameObject) };
            entry.Invoke(user);
        }
    }

    void AddVTableEntry()
    {
        var entry = new VTableEntry { suppliedTypes = new[] { typeof(GameObject) } };
        vTableEntries.Add(entry);
    }
}

[Serializable]
public class EmilEvent<T> : EmilEventBase
{
    [SerializeField, ShowIf(nameof(useUnityEvent))]
    private UnityEvent<T> _unityEvent;

    protected override UnityEvent GetUnityEvent() => throw new NotSupportedException("Generic EmilEvent uses UnityEvent<T>");
    public UnityEvent<T> UnityEventT => _unityEvent ??= new UnityEvent<T>();

    [SerializeField, ShowIf(nameof(useVTable))]
    [ListDrawerSettings(CustomAddFunction = nameof(AddVTableEntry))]
    private List<VTableEntry> vTableEntries = new List<VTableEntry>();

    public void Invoke(T arg)
    {
        if (useUnityEvent && _unityEvent != null) _unityEvent.Invoke(arg);
        if (!useVTable) return;
        foreach (var entry in vTableEntries)
        {
            entry.suppliedTypes = new[] { typeof(T) };
            entry.Invoke(arg);
        }
    }

    void AddVTableEntry()
    {
        var entry = new VTableEntry { suppliedTypes = new[] { typeof(T) } };
        vTableEntries.Add(entry);
    }
}

[Serializable]
public class EmilEvent<T1, T2> : EmilEventBase
{
    [SerializeField, ShowIf(nameof(useUnityEvent))]
    private UnityEvent<T1, T2> _unityEvent;

    protected override UnityEvent GetUnityEvent() => throw new NotSupportedException();
    public UnityEvent<T1, T2> UnityEventT => _unityEvent ??= new UnityEvent<T1, T2>();

    [SerializeField, ShowIf(nameof(useVTable))]
    [ListDrawerSettings(CustomAddFunction = nameof(AddVTableEntry))]
    private List<VTableEntry> vTableEntries = new List<VTableEntry>();

    public void Invoke(T1 arg1, T2 arg2)
    {
        if (useUnityEvent && _unityEvent != null) _unityEvent.Invoke(arg1, arg2);
        if (!useVTable) return;
        foreach (var entry in vTableEntries)
        {
            entry.suppliedTypes = new[] { typeof(T1), typeof(T2) };
            entry.Invoke(arg1, arg2);
        }
    }

    void AddVTableEntry()
    {
        var entry = new VTableEntry { suppliedTypes = new[] { typeof(T1), typeof(T2) } };
        vTableEntries.Add(entry);
    }
}

[Serializable]
public class EmilEvent<T1, T2, T3> : EmilEventBase
{
    [SerializeField, ShowIf(nameof(useUnityEvent))]
    private UnityEvent<T1, T2, T3> _unityEvent;

    protected override UnityEvent GetUnityEvent() => throw new NotSupportedException();
    public UnityEvent<T1, T2, T3> UnityEventT => _unityEvent ??= new UnityEvent<T1, T2, T3>();

    [SerializeField, ShowIf(nameof(useVTable))]
    [ListDrawerSettings(CustomAddFunction = nameof(AddVTableEntry))]
    private List<VTableEntry> vTableEntries = new List<VTableEntry>();

    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        if (useUnityEvent && _unityEvent != null) _unityEvent.Invoke(arg1, arg2, arg3);
        if (!useVTable) return;
        foreach (var entry in vTableEntries)
        {
            entry.suppliedTypes = new[] { typeof(T1), typeof(T2), typeof(T3) };
            entry.Invoke(arg1, arg2, arg3);
        }
    }

    void AddVTableEntry()
    {
        var entry = new VTableEntry { suppliedTypes = new[] { typeof(T1), typeof(T2), typeof(T3) } };
        vTableEntries.Add(entry);
    }
}

[Serializable]
public class EmilEvent<T1, T2, T3, T4> : EmilEventBase
{
    [SerializeField, ShowIf(nameof(useUnityEvent))]
    private UnityEvent<T1, T2, T3, T4> _unityEvent;

    protected override UnityEvent GetUnityEvent() => throw new NotSupportedException();
    public UnityEvent<T1, T2, T3, T4> UnityEventT => _unityEvent ??= new UnityEvent<T1, T2, T3, T4>();

    [SerializeField, ShowIf(nameof(useVTable))]
    [ListDrawerSettings(CustomAddFunction = nameof(AddVTableEntry))]
    private List<VTableEntry> vTableEntries = new List<VTableEntry>();

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (useUnityEvent && _unityEvent != null) _unityEvent.Invoke(arg1, arg2, arg3, arg4);
        if (!useVTable) return;
        foreach (var entry in vTableEntries)
        {
            entry.suppliedTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
            entry.Invoke(arg1, arg2, arg3, arg4);
        }
    }

    void AddVTableEntry()
    {
        var entry = new VTableEntry { suppliedTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) } };
        vTableEntries.Add(entry);
    }
}
