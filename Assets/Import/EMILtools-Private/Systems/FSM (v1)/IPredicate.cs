using System;
using EMILtools.Systems;

public interface IPredicate : IResolvable
{
    bool Evaluate();
}

