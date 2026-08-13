using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMILtools.Systems;
using Sirenix.OdinInspector;
using UnityEngine;


public interface IFSM
{
    public IState CurrentStateIState { get; }
    public Type CurrentStateType { get; }
    public void AddTransition(IState from, IState to, IPredicate condition, string condName, Resolves Resolves = default);
    public void AddAnyTransition(IState to, IPredicate condition, string condName, Resolves Resolves = default);
    public Task PollTransitionsAsync();
    public IState State<TState>() where TState : IState;
}

public class StateMachine<TViewCtx> : IFSM
    where TViewCtx : IContextViewImmutable
{
    [ShowInInspector] StateNode CurrentNode;
    [ShowInInspector] List<ITransition> AnyTransitions;
    Dictionary<Type, StateNode> Nodes;

    public IState CurrentStateIState => CurrentNode?.State;
    public Type CurrentStateType => CurrentNode?.State?.GetType();
    
    readonly TViewCtx Context;
    [ShowInInspector, ReadOnly] List<string> states; 
    
    public StateMachine(TViewCtx ctx, IState initialState)
    {
        Context = ctx;
        Nodes = new Dictionary<Type, StateNode>();
        CurrentNode = GetOrAddNode(initialState);
        CurrentNode.State?.OnEnterState(Context);
        AnyTransitions = new List<ITransition>();
    }

    bool isPollingTransitions = false;
    bool blockTransitions = false;
    
    public void SetBlockTransitions(bool _blockTransitions) => blockTransitions = _blockTransitions;
    
    public async Task PollTransitionsAsync()
    {
        if (isPollingTransitions) return;
        isPollingTransitions = true;

        try
        {
            var transition = await TryResolveAndGetTransition();
            if(transition != null) ChangeState(transition);
        }
        finally
        {
            isPollingTransitions = false;
        }
    }

    public void CheckTransitions()
    {
        var transition = TryResolveAndGetTransition().Result;
        if (transition != null)
        {
            ChangeState(transition);
            Debug.Log($"Transitioning to {transition.To.GetType().Name}");
        }
    }

    /// <summary>
    /// Only use if you know the state has been added
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public IState State<TState>() where TState: IState
    {
        var type = typeof(TState);
        if(!Nodes.TryGetValue(type, out var node)) throw new Exception($"State {type} not found");
        return node.State;
    }

    async Task<ITransition> TryResolveAndGetTransition()
    {
        var current = CurrentNode?.State;
        
        for (int i = 0; i < AnyTransitions.Count; i++)
        {
            var t = AnyTransitions[i];
            //if (current == t.To) continue;
            if (await Resolver<TViewCtx>.ResolveContainer(
                    t.Resolves,
                    t.Condition,
                    canShortCircuit: true,
                    Context))
            {
                return t;
            }
        }

        for (int i = 0; i < CurrentNode.Transitions.Count; i++)
        {
            var t = CurrentNode.Transitions[i];
            //if (current == t.To) continue;
            if (await Resolver<TViewCtx>.ResolveContainer(
                    t.Resolves,
                    t.Condition,
                    canShortCircuit: true,
                    Context))
            {
                return t;
            }
        }

        return null;
    }
    
    
    /// <summary>
    /// Only use if you know the state has been added
    /// </summary>
    /// <param name="condition"></param>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="TTo"></typeparam>
    public void AddTransition<TFrom, TTo>(IPredicate condition, string condName, Resolves Resolves = default)
        where TFrom : IState
        where TTo : IState
        => AddTransition(State<TFrom>(), State<TTo>(), condition, condName, Resolves);
    
    /// <summary>
    /// Only use if you know the state has been added
    /// </summary>
    /// <param name="condition"></param>
    /// <typeparam name="TTo"></typeparam>
    public void AddAnyTransition<TTo>(IPredicate condition, string condName, Resolves Resolves = default)
        where TTo : IState
        => AddAnyTransition(State<TTo>(), condition, condName, Resolves);
    
    public void AddTransition(IState from, IState to, IPredicate condition, string condName, Resolves Resolves = default) 
        => GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition, condName, Resolves);
    
    public void AddAnyTransition(IState to, IPredicate condition, string condName, Resolves Resolves = default)
        => AnyTransitions.Add(new Transition(GetOrAddNode(to).State, condition, Resolves, condName));

    public void AddNode(IState state) => GetOrAddNode(state);

    StateNode GetOrAddNode(IState state)
    {
        var node = Nodes.GetValueOrDefault(state.GetType());

        if (node == null)
        {
            node = new StateNode(state);
            Nodes.Add(state.GetType(), node);
            if(states == null) states = new List<string>();
            states.Add(node.name);
        }
        return node;
    }

    void ChangeState(ITransition transition)
    {
        if (CurrentNode.State == transition.To) return;

        var prev = CurrentNode.State;
        var next = Nodes[transition.To.GetType()].State;
        
        prev?.OnExitState(Context);
        next?.OnEnterState(Context);
        
        CurrentNode = Nodes[transition.To.GetType()];
    }
    
    [Serializable]
    public class StateNode
    {
        [ShowInInspector] public readonly string name;
        [HideInInspector] public IState State;
        public List<ITransition> Transitions;
        
        public StateNode(IState state)
        {
            State = state;
            Transitions = new List<ITransition>();
            name = state.GetType().Name;
        }
        
        public void AddTransition(IState to, IPredicate condition, string condName, Resolves Resolves = default)
            => Transitions.Add(new Transition(to, condition, Resolves, condName));
        
    }
}