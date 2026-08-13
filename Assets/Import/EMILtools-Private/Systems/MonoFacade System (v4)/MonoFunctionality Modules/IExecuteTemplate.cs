
namespace EMILtools.Systems
{
    /// <summary>
    /// Optionally Context Dependant
    /// Template Method Pattern
    /// ValueType Execution Hook
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IExecuteTemplate<TContext> 
        where TContext : class
    {
        public void Execute(TContext ctx);
        public bool ExecutionImplementation(TContext ctx);
    }
}
