using System;
using System.Collections;
using UnityEngine;

namespace Bootstrap.Helper
{
    public class GroupWorker : MonoBehaviour
    {
        private static GroupWorker _instance = null;
        
        static GroupWorker()
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

        private static GroupWorker LazyInstance()
        {
            if (_instance != null)
                return _instance;
            
            GameObject go = new GameObject(nameof(GroupWorker));
            DontDestroyOnLoad(go);
            
            _instance = go.AddComponent<GroupWorker>();
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