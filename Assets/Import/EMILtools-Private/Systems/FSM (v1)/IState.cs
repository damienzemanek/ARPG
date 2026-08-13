using EMILtools.Systems;
using UnityEngine;

public interface IState
{
    virtual void OnEnterState(IContextViewImmutable ctx) { }
    virtual void OnExitState(IContextViewImmutable ctx) { }
}