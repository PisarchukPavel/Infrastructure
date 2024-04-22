using System;
using System.Collections;
using UnityEngine;

namespace UI.Base.Order
{
    public class UIWorker : MonoBehaviour
    {
        private static UIWorker _instance = null;
        
        static UIWorker()
        {
            GetOrCreateInstance();
        }

        public static IDisposable Process(IEnumerator operation)
        {
            Coroutine coroutine = GetOrCreateInstance().StartCoroutine(operation);
            IDisposable disposer = new CoroutineDisposer(() =>
            {
                GetOrCreateInstance().StopCoroutine(coroutine);
            });

            return disposer;
        }

        private static UIWorker GetOrCreateInstance()
        {
            if (_instance != null)
                return _instance;

            GameObject go = new GameObject(nameof(UIWorker));
            DontDestroyOnLoad(go);

            _instance = go.AddComponent<UIWorker>();
            _instance.transform.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            
            return _instance;
        }
        
        private class CoroutineDisposer : IDisposable
        {
            private Action _disposeCallback = null;
            
            public CoroutineDisposer(Action disposeCallback)
            {
                _disposeCallback = disposeCallback;
            }

            void IDisposable.Dispose()
            {
                _disposeCallback?.Invoke();
                _disposeCallback = null;
            }
        }
    }
}