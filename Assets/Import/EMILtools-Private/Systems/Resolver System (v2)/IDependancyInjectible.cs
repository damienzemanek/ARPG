namespace EMILtools.Systems
{
    public interface IDependancyInjectible<TDependency>
    {
        public void InjectDependency(TDependency dependency);
    }
}