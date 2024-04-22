using System;
using System.Collections.Generic;

namespace LightContainer.Base.Binding
{
    public class Binder : IBinder, IBinderInternal
    {
        private event Action<IBinderInternal> NowEvent;
        event Action<IBinderInternal> IBinderInternal.Now
        {
            add
            {
                Verify();
                NowEvent += value;
            }
            remove
            {
                if(_bound)
                    return;

                NowEvent -= value;
            }
        }

        bool IBinderInternal.Filled => _instance != null || _creator != null || _resolveType != null;
        Type IBinderInternal.BindType => _bindType;
        Type IBinderInternal.ResolveType => _resolveType;
        object IBinderInternal.Instance => Instance();
        
        bool IBinderInternal.Override => _override;
        int IBinderInternal.Priority => _priority;
        ParametersContext IBinderInternal.Parameters => _parameters;
        Func<bool> IBinderInternal.Condition => _condition;

        private bool _bound = false;
        private Type _bindType = null;
        private Type _resolveType = null;
        private object _instance = default;

        private bool _override = false;
        private int _priority = int.MinValue;
        private Func<object> _creator = null; 
        private ParametersContext _parameters = null;
        private List<Action<object>> _processors = null;
        private Func<bool> _condition = null;

        IBinder IBinder.As(Type type)
        {
            Verify();

            _bindType = type;
            
            return this;
        }
        
        private object Instance()
        {
            if (_instance == null && _creator != null)
            {
                Verify();

                _instance = _creator.Invoke();
                _resolveType = _instance.GetType();
            }

            return _instance;
        }
        
        void IBinderInternal.Resolve(object instance)
        {
            Verify();

            _bound = true;
            _instance = instance;
            _processors?.ForEach(x => x?.Invoke(_instance));
            _processors?.Clear();
        }
        
        IBinder IBinder.From(object instance)
        {
            Verify();

            if(!_bindType.IsInstanceOfType(instance))
                throw new InvalidCastException($"Can't bind {instance.GetType().Name} instance to {_bindType.Name} type");

            Set(instance, instance.GetType(), null);
            
            return this;
        }

        IBinder IBinder.From(Type type)
        {
            Verify();

            if(!_bindType.IsAssignableFrom(type))
                throw new InvalidCastException($"Can't bind {type.Name} type to {_bindType.Name} type");

            Set(default, type, null);
            
            return this;
        }

        IBinder IBinder.From(Func<object> creator)
        {
            Verify();
            
            Set(default, null, creator);
            
            return this;
        }

        private void Set(object instance, Type type, Func<object> creator)
        {
            _instance = instance;
            _resolveType = type;
            _creator = creator;
        }
        
        IBinder IBinder.Preprocess(Action<object> processor)
        {
            Verify();

            _processors ??= new List<Action<object>>();
            _processors.Add(processor);
            
            return this;
        }

        IBinder IBinder.With(ParametersContext parameters)
        {
            Verify();
            
            _parameters = parameters;
            
            return this;
        }

        IBinder IBinder.Block(Func<bool> condition)
        {
            Verify();
            
            _condition = condition;
            
            return this;
        }

        IBinder IBinder.Override(int priority)
        {
            Verify();

            _override = true;
            _priority = priority;
            
            return this;
        }

        void IBinder.Now()
        {
            Verify();
            NowEvent?.Invoke(this);
        }
        
        void IDisposable.Dispose()
        {
            _bindType = null;
            _resolveType = null;
            _instance = default;
            _creator = null;
            _parameters = null;
            _processors?.Clear();
            _processors = null;
            _condition = null;
            
            NowEvent = null;
        }
        
        private void Verify()
        {
            if(_bound)
                throw new InvalidOperationException($"Binder for {_bindType.Name} from {_resolveType.Name} is already resolved");
        }
    }
}