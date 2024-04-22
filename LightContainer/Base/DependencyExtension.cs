using System;
using LightContainer.Base.Binding;

namespace LightContainer.Base
{
    public static class DependencyExtension
    {
        public static IBinder<T> Bind<T>(this IDependencyBinder binder)
        {
            IBinder binding = binder.Bind();
            
            binding.As(typeof(T));
            binding.From(typeof(T));
            
            return new Binder<T>(binding);
        }
        
        public static IBinder Bind(this IDependencyBinder binder, Type type)
        {
            IBinder binding = binder.Bind();
            
            binding.As(type);
            binding.From(type);
            
            return binding;
        }
 
        public static void Enforce(this IDependencyContainer container)
        {
            container.Inject(container, null);
        }
        
        public static IDisposable AssignBefore(this IDependencyAssigner assigner, IDependencyStorage assignTarget)
        {
            return assigner.Assign(assignTarget, eAssignPosition.Before);
        }
        
        public static IDisposable AssignAfter(this IDependencyAssigner assigner, IDependencyStorage assignTarget)
        {
            return assigner.Assign(assignTarget, eAssignPosition.After);
        }
        
        public static bool Exist<T>(this IDependencyStorage storage)
        {
            return storage.Get(typeof(T)) != null;
        }
        
        public static T Get<T>(this IDependencyStorage storage)
        {
            return (T)storage.Get(typeof(T));
        }
        
        public static void Inject<T>(this IDependencyInjector injector, object target, ParametersContext parameters = null)
        {
            injector.Inject(target, parameters);
        }
        
        public static T Resolve<T>(this IDependencyResolver resolver, ParametersContext parameters = null)
        {
            return (T)resolver.Resolve(typeof(T), parameters);
        }
    }
}