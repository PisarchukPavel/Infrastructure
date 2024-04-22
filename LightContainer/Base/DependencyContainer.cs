using System;
using System.Collections.Generic;
using System.Data;
using LightContainer.Base.Binding;
using LightContainer.Reflection;
using LightContainer.Reflection.Direct;

namespace LightContainer.Base
{
    public class DependencyContainer : IDependencyContainer
    {
        private event Action<object> InjectEvent;
        event Action<object> IDependencyObserver.Inject
        {
            add => InjectEvent += value;
            remove => InjectEvent -= value;
        }

        private event Action<object> ResolveEvent;
        event Action<object> IDependencyObserver.Resolve
        {
            add => ResolveEvent += value;
            remove => ResolveEvent -= value;
        }

        private event Action DisposeEvent;
        event Action IDependencyObserver.Dispose
        {
            add => DisposeEvent += value;
            remove => DisposeEvent -= value;
        }
        
        // State
        private bool _enforced = false;
        private bool _enforcing = false;
        private bool _disposed = false;
        
        // Data
        private readonly List<IDisposable> _assigners = new List<IDisposable>();
        private readonly List<IDependencyStorage> _before = new List<IDependencyStorage>();
        private readonly List<IDependencyStorage> _after = new List<IDependencyStorage>();
        private readonly Dictionary<Type, object> _self = new Dictionary<Type, object>();
        
        // Bindings
        private readonly HashSet<object> _injected = new HashSet<object>();
        private readonly List<IBinderInternal> _related = new List<IBinderInternal>();
        private readonly List<IBinderInternal> _unrelated = new List<IBinderInternal>();
            
        // Include
        private static readonly List<Type> Included = new List<Type>()
        {
            typeof(IDependencyReader),
            typeof(IDependencyObserver),
            typeof(IDependencyStorage),
            typeof(IDependencyInjector),
            typeof(IDependencyResolver)
        };
        
        // Exclude
        private static readonly List<Type> Excluded = new List<Type>()
        {
            typeof(DependencyContainer),
            typeof(IDependencyContainer),
            typeof(IDependencyWriter),
            typeof(IDependencyBinder),
            typeof(IDependencyAssigner),
            typeof(IDependencyDisposer)
        };

        // Utility
        private static readonly object Locker = new object();
        private static readonly DirectReflection DirectReflection = new DirectReflection();
        private static readonly List<object[]> ParametersPool = new List<object[]>();

        static DependencyContainer()
        {
            string directReflectionTypeName = $"{nameof(LightContainer.Reflection.Direct)}.DirectReflectionGenerated";
            Type directReflectionType = Type.GetType(directReflectionTypeName);
            
            if (directReflectionType != null)
            {
                DirectReflection = (DirectReflection)Activator.CreateInstance(directReflectionType);
            }
        }
        
        public DependencyContainer()
        {
            Included.ForEach(x => _self.Add(x, this));
            ReceiveParametersContainer(10);
        }
        
        IBinder IDependencyBinder.Bind()
        {
            lock (Locker)
            {
                if(_disposed)
                    throw new InvalidOperationException($"Container is already disposed");
                
                if(_enforced)
                    throw new InvalidOperationException($"Container is already enforced");
               
                Binder binder = new Binder();
                ((IBinderInternal)binder).Now += OnBindNow;
                _unrelated.Add(binder);
                
                return binder;
            }
        }

        IDisposable IDependencyAssigner.Assign(IDependencyStorage container, eAssignPosition position)
        {
            lock (Locker)
            {
                if(_disposed)
                    throw new InvalidOperationException($"Container is already disposed");
                
                if (container == this)
                    throw new InvalidOperationException($"Assign fail. Trying assign itself");

                switch (position)
                {
                    case eAssignPosition.Before:
                        IDisposable beforeDisposer = new DisposeAction(() => _before.Remove(container));
                        _before.Insert(0, container);
                        _assigners.Add(beforeDisposer);
                        return beforeDisposer;
                    
                    case eAssignPosition.After:
                        IDisposable afterDisposer = new DisposeAction(() => _after.Remove(container));
                        _after.Add(container);
                        _assigners.Add(afterDisposer);
                        return afterDisposer;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(position), position, null);
                }
            }
        }
        
        void IDependencyDisposer.Dispose()
        {
            lock (Locker)
            {
                if(_disposed)
                    return;
         
                _disposed = true;
                DisposeEvent?.Invoke();
                
                InjectEvent = null;
                ResolveEvent = null;
                DisposeEvent = null;
                
                _assigners.ForEach(x => x?.Dispose());
                _assigners.Clear();
                
                _before.Clear();
                _after.Clear();
                _self.Clear();
                
                _related.Clear();
                _unrelated.Clear();
                _injected.Clear();
            }
        }
       
        object IDependencyStorage.Get(Type type)
        {
            ExtractValue(type, false, null, out object result);
            return result;
        }
        
        void IDependencyInjector.Inject(object target, ParametersContext context)
        {
            InjectImpl(target, context);
        }

        private void InjectImpl(object target, ParametersContext context)
        {
            lock (Locker)
            {
                Enforce();

                if(_disposed)
                    throw new InvalidOperationException($"Container is already disposed");
                
                if(target == null)
                    throw new NullReferenceException("Target object for injection is null");
                
                if(_injected.Contains(target))
                    return;
                
                TypeInformation typeInformation = TypeStorage.Get(target.GetType());
                if (typeInformation == null)
                    return;

                foreach (MethodInformation methodInformation in typeInformation.Methods)
                {
                    ParameterInformation[] parameters = methodInformation.Parameters;
                    object[] parametersData = ReceiveParametersContainer(parameters.Length);

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ParameterInformation parameter = parameters[i];
                        if (!ExtractValue(parameter.Type, parameter.HasDefault, context, out parametersData[i]))
                            throw new NullReferenceException($"Inject fail. Missing {parameter.Type.Name} parameter in {methodInformation.Method.Name} method " +
                                                             $"for {target.GetType().Name} object");
                    }

                    if (!DirectReflection.Inject(methodInformation.Id, target, parametersData))
                    {
                        methodInformation.Method.Invoke(target, parametersData);
                    }
                    
                    ReleaseParametersContainer(parametersData);
                }

                InjectEvent?.Invoke(target);
            }
        }
        
        object IDependencyResolver.Resolve(Type type, ParametersContext context)
        {
            return ResolveImpl(type, context, true);
        }

        private object ResolveImpl(Type type, ParametersContext context, bool inject)
        {
            lock (Locker)
            {
                if(_disposed)
                    throw new InvalidOperationException($"Container is already disposed");
                
                Enforce();
                
                TypeInformation typeInformation = TypeStorage.Get(type);
                if (typeInformation == null)
                    throw new NullReferenceException($"Resolve fail. Missing {type.Name} type information");

                foreach (ConstructorInformation constructorInformation in typeInformation.Constructors)
                {
                    bool matched = true;
                    ParameterInformation[] parameters = constructorInformation.Parameters;
                    object[] parametersData = ReceiveParametersContainer(parameters.Length);

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ParameterInformation parameter = parameters[i];
                        if (!ExtractValue(parameter.Type, parameter.HasDefault, context, out parametersData[i]))
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        if (!DirectReflection.Resolve(constructorInformation.Id, out object result, parametersData))
                        {
                            result = constructorInformation.Constructor.Invoke(parametersData);
                        }
                       
                        ResolveEvent?.Invoke(result);

                        if (inject)
                        {
                            InjectImpl(result, context);
                        }

                        ReleaseParametersContainer(parametersData);
                        
                        return result;
                    }
                    else
                    {
                        ReleaseParametersContainer(parametersData);
                    }
                }

                throw new NullReferenceException($"Resolve fail. Missing matched constructor for {type.Name} object");
            }
        }
        
        private bool Resolvable(Type type, ParametersContext context)
        {
            TypeInformation typeInformation = TypeStorage.Get(type);
            if (typeInformation == null)
                throw new NullReferenceException($"Resolvable fail. Missing {type.Name} type information");

            foreach (ConstructorInformation constructorInformation in typeInformation.Constructors)
            {
                bool matched = true;
                    
                ParameterInformation[] parameters = constructorInformation.Parameters;
                foreach (ParameterInformation parameter in parameters)
                {
                    if (!ExtractValue(parameter.Type, parameter.HasDefault, context,  out _))
                    {
                        matched = false;
                    }
                }

                if (matched)
                {
                    return true;
                }
            }

            return false;
        }

        private object[] ReceiveParametersContainer(int capacity)
        {
            if (ParametersPool.Count < capacity)
            {
                ParametersPool.Capacity = capacity;
                for (int i = ParametersPool.Count; i < capacity + 1; i++)
                {
                    ParametersPool.Add(new object[i]);
                }
            }

            return ParametersPool[capacity];
        }
        
        private void ReleaseParametersContainer(object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i] = null;
            }
        }
        
        private bool ExtractValue(Type type, bool hasDefault, ParametersContext context, out object value)
        {
            if (Included.Contains(type))
            {
                value = this;
                return true;
            }
            
            if(Excluded.Contains(type))
                throw new InvalidOperationException($"Get fail. Trying access to excluded {type.Name} type");
            
            value = Type.Missing;
            bool exist = false;

            // Check additional parameters
            if (context != null && context.Additional.TryGetValue(type, out value))
                return true;
            
            // Check before containers
            foreach (IDependencyStorage assignedContainer in _before)
            {
                value = assignedContainer.Get(type);
                exist = value != null;
                
                if (exist)
                    break;
            }
            
            // Check self container
            if (!exist)
            {
                exist = _self.TryGetValue(type, out value);
            }

            // Check after containers
            if (!exist)
            {
                foreach (IDependencyStorage assignedContainer in _after)
                {
                    value = assignedContainer.Get(type);
                    exist = value != null;
                    
                    if (exist)
                        break;
                }
            }
            
            return exist || hasDefault;
        }
        
        private void Enforce()
        {
            if(_enforced || _enforcing)
                return;
            
            if(_unrelated.Count == 0)
                return;

            _enforcing = true;
            
            PrepareBindings();
            ResolveBindings();
            InjectBindings();

            _enforced = true;
            _enforcing = false;
        }

        private void PrepareBindings()
        {
            for (int currentIndex = 0; currentIndex < _unrelated.Count; currentIndex++)
            {
                IBinderInternal current = _unrelated[currentIndex];
                if(current == null)
                    continue;

                int similarIndex = _unrelated.FindIndex(x => x != null && x != current && x.BindType == current.BindType);
                IBinderInternal similar = similarIndex == -1 ? null : _unrelated[similarIndex];
                if (similar != null)
                {
                    if (!current.Override)
                    {
                        _unrelated[currentIndex] = null;
                    }
                    else
                    {
                        int removeIndex = current.Priority > similar.Priority ? similarIndex : currentIndex;
                        _unrelated[removeIndex] = null;
                    }
                }
            }

            _related.Capacity = _related.Count + _unrelated.Count;
        }
        
        private void ResolveBindings()
        {
            bool changed = true;
            while (changed)
            {
                changed = false;

                foreach (IBinderInternal current in _unrelated)
                {
                    if(current == null)
                        continue;
                    
                    if (current.Condition != null && !current.Condition.Invoke())
                        continue;

                    if (!current.Filled)
                        throw new DataException($"Unfilled binder");

                    if (current.Instance != null)
                    {
                        changed = true;
                        current.Resolve(current.Instance);
                        CompleteBinding(current);
                    }
                    else
                    {
                        if (Resolvable(current.ResolveType, current.Parameters))
                        {
                            changed = true;
                            current.Resolve(ResolveImpl(current.ResolveType, current.Parameters, false));
                            CompleteBinding(current);
                        }
                    }
                }
            }
            
            _unrelated.RemoveAll(x => x == null);
            if (_unrelated.Count > 0)
            {
                string unrelatedInfo = string.Empty;
                foreach (IBinderInternal provider in _unrelated)
                {
                    unrelatedInfo += $"({provider.BindType.Name} from {provider.ResolveType.Name}) ";
                }
                
                throw new Exception($"Failed container resolve bindings for {unrelatedInfo}");
            }
        }

        private void InjectBindings()
        {
            foreach (IBinderInternal binder in _related)
            {
                if(_injected.Contains(binder.Instance))
                    continue;
                
                InjectImpl(binder.Instance, binder.Parameters);
                _injected.Add(binder.Instance);
            }

            foreach (IBinderInternal binder in _related)
            {
                binder.Dispose();
            }
        }
        
        private void OnBindNow(IBinderInternal binder)
        {
            if(!binder.Filled)
                throw new DataException($"Unfilled binder");
            
            if (binder.Instance != null)
            {
                binder.Resolve(binder.Instance);
                CompleteBinding(binder);
            }
            else
            {
                if (!Resolvable(binder.ResolveType, binder.Parameters))
                    throw new InvalidOperationException($"Can't bind now {binder.BindType.Name} because {binder.ResolveType.Name} not resolvable");

                binder.Resolve(ResolveImpl(binder.ResolveType, binder.Parameters, false));
                CompleteBinding(binder);
            }
        }

        private void CompleteBinding(IBinderInternal binder)
        {
            VerifyBinder(binder);
            
            if (_self.ContainsKey(binder.BindType))
            {
                if (binder.Override)
                {
                    IBinderInternal similar = _related.Find(x => x.BindType == binder.BindType);
                    if (!similar.Override || similar.Priority < binder.Priority)
                    {
                        _self[binder.BindType] = binder.Instance;
                        _related.Add(binder);
                        _related.Remove(similar);
                    }
                }
            }
            else
            {
                _self.Add(binder.BindType, binder.Instance);
                _related.Add(binder);
            }

            _unrelated[_unrelated.IndexOf(binder)] = null;
        }

        private void VerifyBinder(IBinderInternal binder)
        {
            if (Included.Contains(binder.BindType))
                throw new InvalidOperationException($"Bind fail. Attempting to register a included reserved {binder.BindType.Name} type");
                
            if (Excluded.Contains(binder.BindType))
                throw new InvalidOperationException($"Bind fail. Attempting to register a excluded {binder.BindType.Name} type");
        }

        private class DisposeAction : IDisposable
        {
            private Action _disposer = null;

            public DisposeAction(Action disposer)
            {
                _disposer = disposer;
            }
            
            void IDisposable.Dispose()
            {
                _disposer?.Invoke();
                _disposer = null;
            }
        }
    }
}