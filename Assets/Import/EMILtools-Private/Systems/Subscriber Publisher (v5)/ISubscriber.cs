using System.Threading.Tasks;

namespace EMILtools.Systems
{
    public interface ISubscriber
    {
        bool isActive { get; set; }
        Task Execute();
    }
    
    public interface ISubscriber<TContext> : ISubscriber
    {
        TContext cachedCtx { get; set; }
        Task Execute(TContext ctx);
    }
    
    public interface ISubscriber<T1, T2> : ISubscriber
    {
        T1 cachedCtx1 { get; set; }
        T2 cachedCtx2 { get; set; }
        Task Execute(T1 ctx1, T2 ctx2);
    }
    

}