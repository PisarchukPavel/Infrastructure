using System;

namespace LightContainer.Base.Binding
{
    public class Binder<T> : Binder, IBinder<T>
    {
        private readonly IBinder _binder = null;
        
        public Binder(IBinder binder)
        {
            _binder = binder;
        }

        IBinder<T> IBinder<T>.From(T instance)
        {
            _binder.From(instance);
            return this;
        }

        IBinder<T> IBinder<T>.From<THeir>()
        {
            _binder.From(typeof(T));
            return this;
        }

        IBinder<T> IBinder<T>.From(Func<T> creator)
        {
            _binder.From(() => creator.Invoke());
            return this;
        }

        IBinder<T> IBinder<T>.With(ParametersContext parameters)
        {
            _binder.With(parameters);
            return this;
        }

        IBinder<T> IBinder<T>.Preprocess(Action<T> processor)
        {
            _binder.Preprocess(obj => processor?.Invoke((T)obj));
            return this;
        }

        IBinder<T> IBinder<T>.Block(Func<bool> condition)
        {
            _binder.Block(condition);
            return this;
        }

        IBinder<T> IBinder<T>.Override(int priority)
        {
            _binder.Override(priority);
            return this;
        }

        void IBinder<T>.Now()
        {
            _binder.Now();
        }
    }
}