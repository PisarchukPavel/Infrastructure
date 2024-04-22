using System;

namespace LightContainer.Base.Binding
{
    public interface IBinder
    {
        IBinder As(Type type);
        IBinder From(object instance);
        IBinder From(Type type);
        IBinder From(Func<object> creator);
        
        IBinder With(ParametersContext parameters);
        IBinder Preprocess(Action<object> processor);
        IBinder Block(Func<bool> condition);
        IBinder Override(int priority);
        
        void Now();
    }
    
    public interface IBinder<T>
    {
        IBinder<T> From(T instance);
        IBinder<T> From<THeir>() where THeir : T;
        IBinder<T> From(Func<T> creator);
        
        IBinder<T> With(ParametersContext parameters);
        IBinder<T> Preprocess(Action<T> processor);
        IBinder<T> Block(Func<bool> condition);
        IBinder<T> Override(int priority);
        
        void Now();
    }
}