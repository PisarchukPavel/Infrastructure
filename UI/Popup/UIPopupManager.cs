using System;
using System.Collections.Generic;
using UI.Base;
using UI.Base.Manager;
using UI.Popup.Observer;
using UnityEngine;

namespace UI.Popup
{
    public class UIPopupManager : UIManagerBase<UIPopupBase>, IPopupManager
    {
        IPopupObserver IPopupManager.Current => (_observersProcessing.Count == 0) ? null : _observersProcessing[^1];

        [SerializeField]
        private int _backgroundOrder = 100;
        
        [SerializeField] 
        private UIElementBase _background = null;

        private bool _instantShow = false;
        private bool _instantHide = false;
        
        private readonly List<PopupEntry> _entriesQueue = new List<PopupEntry>();
        private readonly List<PopupEntry> _entriesProcessing = new List<PopupEntry>();
        
        private readonly List<PopupEntry> _entriesOpening = new List<PopupEntry>();
        private readonly List<PopupEntry> _entriesHiding = new List<PopupEntry>();
        
        private readonly List<IPopupObserver> _observersProcessing = new List<IPopupObserver>();
        
        private readonly List<PopupEntry> _unvoicedEntries = new List<PopupEntry>();
        private readonly List<PopupListener> _popupListenersOriginal = new List<PopupListener>();
        private readonly List<PopupListener> _popupListenersIterator = new List<PopupListener>();
        
        protected override void OnInitialized()
        {
            ((IUIElement)_background).Initialize();
        }

        IDisposable IPopupManager.Listen(Action<IPopupObserver> callback, Predicate<IPopupObserver> predicate)
        {
            PopupListener listener = new PopupListener(callback, predicate);
            listener.Disposer(() => _popupListenersOriginal.Remove(listener));
            _popupListenersOriginal.Add(listener);
            
            return listener;
        }

        IPopupObserver<TElement> IPopupManager.Show<TElement>(bool instant, Predicate<TElement> predicate)
        {
            if (!Find(out TElement element, predicate))
            {
                Debug.Log($"[{nameof(UIPopupManager)}] {typeof(TElement)} popup not found. Can be overriden later.");    
            }
            
            PopupObserver<TElement> observer = new PopupObserver<TElement>(element, instant);

            PopupEntry entry = PopupEntry.Create(observer);
            _entriesQueue.Add(entry);
            _unvoicedEntries.Add(entry);
            
            return observer;
        }

        protected override void OnUpdate()
        {
            Process();
        }
        
        // TODO Refactoring
        protected override void OnExtract(UIPopupBase element)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                
                PopupEntry extractEntry =
                    _entriesOpening.Find(x => x.Controller.PopupBase == element) ??
                    _entriesHiding.Find(x => x.Controller.PopupBase == element) ??
                    _entriesProcessing.Find(x => x.Controller.PopupBase == element);

                if (extractEntry != null)
                {
                    _entriesOpening.Remove(extractEntry);
                    _entriesHiding.Remove(extractEntry);
                    _entriesProcessing.Remove(extractEntry);
                    _observersProcessing.Remove(extractEntry.Observer);

                    extractEntry.Release();
                    
                    changed = true;
                }

                extractEntry = _entriesQueue.Find(x => x.Controller.PopupBase == element);
                if (extractEntry != null)
                {
                    _entriesQueue.Remove(extractEntry);
                    extractEntry.Cancel();

                    changed = true;
                }
            }
        }
        
        private void Process()
        {
            RemoveCanceled();
            ReorderQueue();
            
            ProcessListeners();
            ProcessQueue();
            ProcessOpened();
            ProcessBackground();
        }
        
        private void RemoveCanceled()
        {
            _entriesQueue.RemoveAll(x =>
            {
                if (x.Controller.HideOrCancel || x.Controller.CancelCondition())
                {
                    x.Cancel();
                    return true;
                }

                return false;
            });
        }
        
        private void ReorderQueue()
        {
            _entriesQueue.Sort((entry1, entry2) =>
            {
                if (entry1.Controller.Priority < entry2.Controller.Priority)
                    return 1;
                
                if (entry1.Controller.Priority > entry2.Controller.Priority)
                    return -1;
                
                return 0;
            });
        }

        private void ProcessListeners()
        {
            _popupListenersIterator.Clear();
            _popupListenersOriginal.RemoveAll(x => x == null);
            _popupListenersIterator.AddRange(_popupListenersOriginal);
            
            foreach (PopupListener popupListener in _popupListenersIterator)
            {
                foreach (PopupEntry observer in _unvoicedEntries)
                {
                    if(observer.Controller == null)
                        continue;
                    
                    if(observer.Controller.HideOrCancel)
                        continue;
                    
                    popupListener?.Invoke(observer);
                }
            }
            
            _unvoicedEntries.Clear();
        }
        
        private void ProcessQueue()
        {
            if(AnimateNow())
                return;
            
            foreach (PopupEntry entry in _entriesQueue)
            {
                IPopupController controller = entry.Controller;

                if (controller.PopupBase == null)
                    continue;    
                
                if (_entriesProcessing.Count > 0)
                {
                    IPopupController topController = _entriesProcessing[^1].Controller;
                    if (!controller.Overlap || controller.Priority <= topController.Priority)
                        continue;
                }
                
                if (controller.ShowCondition())
                {
                    Show(entry);
                    break;
                }

                if (controller.BlockerShowCondition)
                    break;
            }
        }

        private void ProcessOpened()
        {
            if (_entriesProcessing.Count == 0 || AnimateNow())
                return;
            
            PopupEntry topEntry = _entriesProcessing[^1];
            IPopupController controller = topEntry.Controller;
            
            if (controller.PopupBase == null || controller.HideCondition() || controller.HideOrCancel)
            {
                Hide(topEntry);
            }
        }

        private void ProcessBackground()
        {
            bool empty = _entriesProcessing.Count == 0;
            Blur(!empty, empty ? _instantHide : _instantShow);
        }
        
        private void Show(PopupEntry entry)
        {
            _instantShow = entry.Controller.InstantShow;
            _entriesQueue.Remove(entry);
            
            IPopupController currentController = entry.Controller;
            IPopupController previousController = (_entriesProcessing.Count == 0) ? null : _entriesProcessing[^1].Controller;
            
            IDisposable backgroundOrder = (previousController == null) ? _background.IncreaseOrder(_backgroundOrder) : _background.OverlapOrder(previousController.PopupBase);
            IDisposable popupOrder = currentController.PopupBase.OverlapOrder(_background);
            
            _entriesProcessing.Add(entry);
            _entriesOpening.Add(entry);
            _observersProcessing.Add(entry.Observer);
            
            entry.SaveOrderDisposer(popupOrder, backgroundOrder);
            entry.Show(() =>
            {
                _entriesOpening.Remove(entry);
            });
        }
        
        private void Hide(PopupEntry entry)
        {
            _instantHide = entry.Controller.InstantHide;
            _entriesProcessing.Remove(entry);
            _observersProcessing.Remove(entry.Observer);
            
            _entriesHiding.Add(entry);
            entry.Hide(() =>
            {
                if (_entriesHiding.Remove(entry))
                {
                    entry.Release();
                }
            });
        }

        private void Blur(bool blurred, bool instant)
        {
            IUIElement background = _background;
            eElementState backgroundState = background.State;
            
            if (blurred)
            {
                if (backgroundState == eElementState.Hided || backgroundState == eElementState.Hiding)
                {
                    background.Show(instant, null);
                }
            }
            else
            {
                if (backgroundState == eElementState.Showed || backgroundState == eElementState.Showing)
                {
                    background.Hide(instant, null);
                }
            }
        }

        private bool AnimateNow()
        {
            return _entriesOpening.Count > 0 || _entriesHiding.Count > 0;
        }
        
        private class PopupEntry
        {
            public IPopupController Controller { get; private set; }
            public IPopupObserver Observer { get; private set; }

            private bool _showed = false;
            private List<IDisposable> _disposables = new List<IDisposable>();
            
            public static PopupEntry Create<TElement>(PopupObserver<TElement> entry) where TElement : UIPopupBase
            {
                return new PopupEntry(entry, entry);
            }
            
            private PopupEntry(IPopupController controller, IPopupObserver observer)
            {
                Controller = controller;
                Observer = observer;
            }

            public void SaveOrderDisposer(params IDisposable[] disposables)
            {
                _disposables.AddRange(disposables);
            }
            
            public void Show(Action completeCallback)
            {
                _showed = true;
                Controller.PopupBase.HideRequest += OnHideRequest;
                Controller.Show(Controller.InstantShow, completeCallback);
            }

            public void Hide(Action completeCallback)
            {
                if (Controller.PopupBase == null)
                {
                    completeCallback?.Invoke();
                }
                else
                {
                    Controller.Hide(Controller.InstantHide, completeCallback);
                }
            }
            
            private void OnHideRequest(bool instant)
            {
                Controller.PopupBase.HideRequest -= OnHideRequest;
                Observer.Hide(instant);
            }

            public void Cancel()
            {
                if(_showed)
                    throw new InvalidOperationException("Internal error. Cancel unavailable after showing, use Release()");
                
                _disposables.ForEach(x => x.Dispose());
                _disposables.Clear();
                _disposables = null;
                
                Controller.Cancel();
                Controller = null;
                Observer = null;
            }

            public void Release()
            {
                if(!_showed)
                    throw new InvalidOperationException("Internal error. Release unavailable before showing, use Cancel()");
                
                if (Controller.PopupBase != null)
                {
                    Controller.PopupBase.HideRequest -= OnHideRequest;
                }
                
                _disposables.ForEach(x => x.Dispose());
                _disposables.Clear();
                _disposables = null;

                Controller.Release();
                Controller = null;
                Observer = null;
            }
        }
        
        private class PopupListener : IDisposable
        {
            private Action _disposer = null;
            private Action<IPopupObserver> _callback = null;
            private Predicate<IPopupObserver> _predicate = null;

            public PopupListener(Action<IPopupObserver> callback, Predicate<IPopupObserver> predicate = null)
            {
                _callback = callback;
                _predicate = predicate;
            }

            public void Disposer(Action disposer)
            {
                _disposer = disposer;
            }
            
            public bool Invoke(PopupEntry entry)
            {
                if (_predicate == null || _predicate.Invoke(entry.Observer))
                {
                    _callback?.Invoke(entry.Observer);
                }

                return false;
            }

            void IDisposable.Dispose()
            {
                _disposer?.Invoke();
                _disposer = null;
                _callback = null;
                _predicate = null;
            }
        }
    }
}