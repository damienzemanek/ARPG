
using EMILtools.Systems;
using Sirenix.OdinInspector;

public interface ITransition
{
    IState To { get; }
    IPredicate Condition { get; }
    Resolves Resolves { get; }
}

// public class Transition : ITransition
// {
//     [ShowInInspector] readonly string toName;
//     public IState To { get; }
//     public IPredicate Condition { get; }
//     
//     public Transition(IState to, IPredicate condition)
//     {
//         To = to;
//         Condition = condition;
//         toName = to.GetType().Name;
//     }
// }

public class Transition : ITransition
{
    [ShowInInspector] readonly string toName;
    [ShowInInspector] readonly string ifCondition;
    public IState To { get; }
    public IPredicate Condition { get; }
    public Resolves Resolves { get; set; }
    public Transition(IState to, IPredicate condition, Resolves resolves, string ifCondition = null)
    {
        To = to;
        Condition = condition;
        Resolves = resolves.allNull
            ? new Resolves(true)
            : resolves;
        toName = to.GetType().Name;
        if(ifCondition == null) this.ifCondition = condition.ToString();
        else this.ifCondition = ifCondition;
    }
}

