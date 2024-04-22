using System;
using System.Collections;
using UnityEngine;

namespace Purchase.Extension
{
    public class PurchaseWorker : MonoBehaviour
    {
        private static PurchaseWorker _instance = null;
        
        static PurchaseWorker()
        {
            LazyInstance();
        }

        public static IDisposable Process(IEnumerator operation)
        {
            Coroutine coroutine = LazyInstance().StartCoroutine(operation);
            IDisposable disposer = new Disposer(() =>
            {
                if(coroutine == null)
                    return;
                
                LazyInstance().StopCoroutine(coroutine);
            });

            return disposer;
        }

        private static PurchaseWorker LazyInstance()
        {
            if (_instance != null)
                return _instance;
            
            GameObject go = new GameObject(nameof(PurchaseWorker));
            DontDestroyOnLoad(go);
            
            _instance = go.AddComponent<PurchaseWorker>();
            _instance.transform.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            return _instance;
        }
        
        private class Disposer : IDisposable
        {
            private Action _action = null;
            
            public Disposer(Action action)
            {
                _action = action;
            }
            
            void IDisposable.Dispose()
            {
                _action?.Invoke();
                _action = null;
            }
        }
    }
}