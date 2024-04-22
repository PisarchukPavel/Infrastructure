using System;
using System.Collections.Generic;
using UnityEngine;

namespace Purchase.Stores.Microsoft
{
    public class WindowsStoreUtils : MonoBehaviour
    {
        private static volatile bool _callbacksPending = false;
        private static readonly List<Action> _callbacks = new List<Action>();

        public static void Create()
        {
            GameObject gameObject = new GameObject(nameof(WindowsStoreUtils));
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            gameObject.AddComponent<WindowsStoreUtils>();
        }
        
        public static void RunOnMainThread(Action runnable)
        {
            lock (_callbacks)
            {
                _callbacks.Add(runnable);
                _callbacksPending = true;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!_callbacksPending)
                return;
            
            Action[] copy = null;
            lock (_callbacks)
            {
                if (_callbacks.Count == 0)
                    return;

                copy = new Action[_callbacks.Count];
                _callbacks.CopyTo(copy);
                _callbacks.Clear();
                _callbacksPending = false;
            }

            foreach (Action action in copy)
            {
                action?.Invoke();
            }
        }
    }
}