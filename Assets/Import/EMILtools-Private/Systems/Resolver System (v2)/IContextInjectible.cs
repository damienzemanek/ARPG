namespace EMILtools.Systems
{
    public interface IContextInjectible<TCtx>
    {
        public void InjectContext(TCtx ctx);
    }
}