using System;
using System.Collections.Generic;
using LightContainer.Base;
using LightContainer.Base.Binding;
using LightContainer.Unity.Base.Behaviour;
using UnityEngine;

namespace LightContainer.Unity.Base.Context
{
    public abstract class BaseContext : MonoBehaviour, IDependencyContainer
    {
        private event Action ReadyImpl;
        public event Action Ready
        {
            add
            {
                if (_installed)
                {
                    value?.Invoke();
                    return;
                }

                ReadyImpl += value;
            }
            remove => ReadyImpl -= value;
        }

        [SerializeField]
        private List<InstallerBehaviour> _installers = new List<InstallerBehaviour>();
        
        private bool _installed = false;
        private IDependencyContainer _container = null;
        private IDisposable _deregister = null;
        
        private Queue<object> _injectionQueue = null;
        private Dictionary<object, ParametersContext> _injectionContexts = null;
        private List<IInstallerBehaviour> _installersRuntime = null;

        private void Awake()
        {
            _installed = false;
            _container = new DependencyContainer();
            _deregister = GlobalContext.Register(this);

            _injectionQueue = new Queue<object>();
            _injectionContexts = new Dictionary<object, ParametersContext>();
            _installersRuntime = new List<IInstallerBehaviour>();
            _installersRuntime.AddRange(_installers);
            _installersRuntime.ForEach(x => x.Initialize(_container));

            OnAwake();
        }

        protected abstract void OnAwake();
        
        protected void Insert(IInstallerBehaviour installer)
        {
            if(_installed)
                throw new InvalidOperationException($"Insert installer fail. {GetType().Name} {gameObject.name} is already installed");
            
            _installersRuntime.Add(installer);
        }
        
        protected void Launch(List<MonoBehaviour> injectionTargets)
        {
            if(_installed)
                throw new InvalidOperationException($"Install fail. {GetType().Name} {gameObject.name} is already installed");
            
            _installed = true;
            _installersRuntime.ForEach(x => x.Install());
            _container.Enforce();

            Debug.Log($"[{nameof(BaseContext)}] Number of injected in {GetType().Name} {gameObject.name} context is {injectionTargets.Count}");
           
            foreach (MonoBehaviour behaviour in injectionTargets)
            {
                if(behaviour == null)
                    continue;
                
                _injectionQueue.Enqueue(behaviour);
            }

            ResolveQueue();
            
            ReadyImpl?.Invoke();
            ReadyImpl = null;
        }

        private void ResolveQueue()
        {
            if (_installed && _injectionQueue.Count > 0)
            {
                while (_injectionQueue.Count != 0)
                {
                    object target = _injectionQueue.Dequeue();
                    _injectionContexts.TryGetValue(target, out ParametersContext context);
                    
                    InjectImpl(target, context);
                }
            }
        }
        
        IBinder IDependencyBinder.Bind()
        {
            return _container.Bind();
        }

        IDisposable IDependencyAssigner.Assign(IDependencyStorage container, eAssignPosition position)
        {
            return _container.Assign(container, position);
        }

        void IDependencyDisposer.Dispose()
        {
            _container.Dispose();
        }

        event Action<object> IDependencyObserver.Inject
        {
            add => ((IDependencyObserver) _container).Inject += value;
            remove => ((IDependencyObserver) _container).Inject -= value;
        }

        event Action<object> IDependencyObserver.Resolve
        {
            add => ((IDependencyObserver) _container).Resolve += value;
            remove => ((IDependencyObserver) _container).Resolve -= value;
        }
        
        event Action IDependencyObserver.Dispose
        {
            add => ((IDependencyObserver) _container).Dispose += value;
            remove => ((IDependencyObserver) _container).Dispose -= value;
        }
        
        object IDependencyStorage.Get(Type type)
        {
            return _container.Get(type);
        }
        
        void IDependencyInjector.Inject(object target, ParametersContext context)
        {           
            if (_installed)
            {
                InjectImpl(target, context);
            }
            else
            {
                _injectionContexts.Add(target, context);    
                _injectionQueue.Enqueue(target);
            }
        }
        
        private void InjectImpl(object target, ParametersContext context)
        {
            _container.Inject(target, context);
        }

        object IDependencyResolver.Resolve(Type type, ParametersContext context)
        {
            return _container.Resolve(type, context);
        }

        private void OnDestroy()
        {
            _container = null;
            
            _deregister?.Dispose();
            _deregister = null;
            
            _injectionQueue.Clear();
            _injectionQueue = null;
            
            _injectionContexts.Clear();
            _injectionContexts = null;
            
            _installersRuntime.Clear();
            _installersRuntime = null;
        }
    }
}