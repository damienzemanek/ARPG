using System;

public interface IGuarder { }

public interface IActionGuarder : IGuarder
{
    IGuardAction CurrentBlocker { get; }
    bool TryEarlyExit();
}


// Conditions
public interface IGuardCondition
{
    string If { get; }
    bool Blocked { get; }
}


public interface IGuardAction : IGuardCondition
{
    public Action branchingAction { get; }
}