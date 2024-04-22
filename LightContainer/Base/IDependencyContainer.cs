using System;
using LightContainer.Base.Binding;

namespace LightContainer.Base
{
    public interface IDependencyContainer : IDependencyWriter, IDependencyReader { }
    public interface IDependencyWriter : IDependencyBinder, IDependencyAssigner, IDependencyDisposer { }
    public interface IDependencyReader : IDependencyObserver, IDependencyStorage, IDependencyInjector, IDependencyResolver { }
    
    // Write contracts
    public interface IDependencyBinder
    {
        IBinder Bind();
    }
    
    public interface IDependencyAssigner
    {
        IDisposable Assign(IDependencyStorage container, eAssignPosition position);
    }
    
    public interface IDependencyDisposer
    {
        void Dispose();
    }
    
    // Read contracts
    public interface IDependencyObserver
    {
        event Action<object> Inject;
        event Action<object> Resolve;
        event Action Dispose;
    }
    
    public interface IDependencyStorage
    {
        object Get(Type type);
    }
    
    public interface IDependencyInjector
    {
        void Inject(object target, ParametersContext context);
    }
    
    public interface IDependencyResolver
    {
        object Resolve(Type type, ParametersContext context);
    }
    
    public enum eAssignPosition
    {
        Before = 0,
        After = 1
    }
}