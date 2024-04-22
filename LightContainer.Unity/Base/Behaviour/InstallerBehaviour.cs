using System;
using UnityEngine;
using LightContainer.Base;
using LightContainer.Base.Binding;

namespace LightContainer.Unity.Base.Behaviour
{
    public abstract class InstallerBehaviour : MonoBehaviour, IInstallerBehaviour
    {
        private bool _initialized = false;
        private bool _installed = false;
        private IDependencyContainer _container = null;

        void IInstallerBehaviour.Initialize(IDependencyContainer root)
        {
            if(_initialized)
                throw new InvalidOperationException($"Initialize fail, {GetType()} is already initialized");
            
            _initialized = true;
            _container = root;
        }

        void IInstallerBehaviour.Install()
        {
            if (_installed)
                throw new InvalidOperationException($"Install fail, {GetType()} is already installed");
            
            _installed = true;
            OnInstall();
        }
        
        protected abstract void OnInstall();

        protected IBinder<T> Bind<T>()
        {
            return _container.Bind<T>();
        }
        
        protected IBinder Bind()
        {
            return _container.Bind();
        }
    }
    
    public interface IInstallerBehaviour
    {
        void Initialize(IDependencyContainer root);
        void Install();
    }
}