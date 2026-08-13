using UnityEngine;

public interface ICommand<TContext, TReturn>
{
    public TReturn Execute(TContext ctx);
}
