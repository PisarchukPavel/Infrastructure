using System;
using System.Collections.Generic;
using Bootstrap.Loading;
using UnityEngine;

namespace Bootstrap
{
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField]
        private List<LoaderBase> _baseLoaders = new List<LoaderBase>();

        private readonly List<object> _runtimeLoaders = new List<object>();
        
        private static BootstrapLoader _instance = null;

        public static IDisposable Insert<T>(ILoader<T> loader)
        {
            return LazyInstance().InsertImpl(loader);
        }
        
        public static ILoader<T> Find<T>(Predicate<ILoader<T>> filter = null)
        {
            return LazyInstance().FindImpl(filter);
        }
        
        public static void Load<T>(T loaderData)
        {
            LazyInstance().LoadImpl(loaderData);
        }
        
        private void LoadImpl<T>(T loaderData)
        {
            ILoader<T> loader = FindImpl<T>();
            
            if(loader == null)
                throw new ArgumentNullException($"Can't find loader with {loaderData} data.");
            
            loader.Load(loaderData);
        }

        private ILoader<T> FindImpl<T>(Predicate<ILoader<T>> filter = null)
        {
            foreach (object loader in _runtimeLoaders)
            {
                if (loader is ILoader<T> loaderGeneric)
                {
                    if (filter == null || filter(loaderGeneric))
                    {
                        return loaderGeneric;
                    }
                }
            }

            return null;
        }
        
        private IDisposable InsertImpl<T>(ILoader<T> loader)
        {
            _runtimeLoaders.Add(loader);
            return new LoaderDisposer(() => _runtimeLoaders.Remove(loader));
        }
        
        private static BootstrapLoader LazyInstance()
        {
            if (_instance == null)
            {
                BootstrapLoader prefab = Resources.Load<BootstrapLoader>($"{nameof(BootstrapLoader)}");
                _instance = (prefab == null) ? 
                    new GameObject(nameof(BootstrapLoader)).AddComponent<BootstrapLoader>() : 
                    Instantiate(prefab, Vector3.zero, Quaternion.identity);
                
                _instance._runtimeLoaders.AddRange(_instance._baseLoaders);

                DontDestroyOnLoad(_instance.gameObject);
            }
            
            return _instance;
        }
        
        private class LoaderDisposer : IDisposable
        {
            private Action _disposeAction = null;

            public LoaderDisposer(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }
            
            void IDisposable.Dispose()
            {
                _disposeAction?.Invoke();
                _disposeAction = null;
            }
        }
    }
}