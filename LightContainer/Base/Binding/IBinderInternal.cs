using System;

namespace LightContainer.Base.Binding
{
    public interface IBinderInternal : IDisposable
    {
        event Action<IBinderInternal> Now;
        
        bool Filled  { get; }
        Type BindType { get; }
        Type ResolveType { get; }
        object Instance { get; }
        
        bool Override { get; }
        int Priority { get; }
        ParametersContext Parameters { get; }
        Func<bool> Condition { get; }

        void Resolve(object instance);
    }
}