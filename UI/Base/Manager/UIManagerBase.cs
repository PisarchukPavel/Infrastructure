using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Base.Manager
{
    public abstract class UIManagerBase<T> : MonoBehaviour, IUIManager<T> where T : UIElementBase
    {
        private bool _initialized = false;
        private readonly List<T> _elements = new List<T>();

        private void Awake()
        {
            LazyInitialize();
        }

        private void Update()
        {
            OnUpdate();
        }

        protected virtual void OnInitialized() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnInsert(T element) { }
        protected virtual void OnExtract(T element) { }
        
        protected bool Find<TElement>(out TElement result, Predicate<TElement> predicate = null) where TElement : T
        {
            LazyInitialize();

            predicate ??= element => true; 
            result = (TElement)_elements.Find(x => x is TElement element && predicate.Invoke(element));
            return result != null;
        }

        IDisposable IUIManager<T>.Insert(T element)
        {
            LazyInitialize();

            if(_elements.Contains(element))
                return Disposer.Stub;
            
            _elements.Insert(0, element);
            OnInsert(element);
            
            return new Disposer(() =>
            {
                _elements.Remove(element);
                OnExtract(element);
            });
        }

        private void LazyInitialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                OnInitialized();
            }
        }

        private class Disposer : IDisposable
        {
            public static IDisposable Stub { get; } = new Disposer(null);
            
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