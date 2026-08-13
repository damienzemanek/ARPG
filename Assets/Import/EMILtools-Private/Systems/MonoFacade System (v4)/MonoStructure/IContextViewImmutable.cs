
namespace EMILtools.Systems
{
    
    public interface IContext { }
    
    /// <summary>
    /// Allows for context to be passed around as an Immutable View
    /// </summary>
    public interface IContextViewImmutable { }
    
    public interface IViewableCtx : IContext, IContextViewImmutable { }

}
