using Bootstrap.Base;

namespace Bootstrap.Feature
{
    public interface IFeatureLauncher<in T>
    {
        int Order { get; }
        
        // May be null
        IOperation Launch(int handle, T data);
    }
}