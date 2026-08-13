using EMILtools.Systems;

public interface IEntryPointFM { }

public interface TickFM<TViewCtx> : IEntryPointFM
    where TViewCtx : class, IContextViewImmutable
{
    void Execute(TViewCtx ctx);
}

public interface UPDATE<TViewCtx> : TickFM<TViewCtx>
    where TViewCtx : class, IContextViewImmutable { }

public interface FIXED_UPDATE<TViewCtx> : TickFM<TViewCtx> 
    where TViewCtx : class, IContextViewImmutable { }

public interface LATE_UPDATE<TViewCtx> : TickFM<TViewCtx> 
    where TViewCtx : class, IContextViewImmutable { }


public interface ON_SET : IEntryPointFM
{
    void MutateUsingNewSetValues();
}

public interface FSM_AVALIABLE : IState { }

public interface FSM_STATE_ENTER<TViewCtx> : IEntryPointFM, FSM_AVALIABLE
    where TViewCtx : IContextViewImmutable
{
    abstract void OnEnterState(TViewCtx ctx);
    void IState.OnEnterState(IContextViewImmutable ctx) => OnEnterState((TViewCtx)ctx);
}

public interface FSM_STATE_EXIT<TViewCtx> : IEntryPointFM, FSM_AVALIABLE
    where TViewCtx : IContextViewImmutable

{
    abstract void OnExitState(TViewCtx ctx);
    void IState.OnExitState(IContextViewImmutable ctx) => OnExitState((TViewCtx)ctx);
}