using System;
using UI.Base;

namespace UI.Popup.Observer
{
    public class PopupObserver<T> : IPopupObserver, IPopupObserver<T>, IPopupController where T : UIPopupBase
    {
        private event Action<T> ShowStart;
        private event Action<T> ShowComplete;
        private event Action<T> HideStart;
        private event Action<T> HideComplete;
        private event Action<T> CancelComplete;
        private event Action<T> ReleaseControl;

        public UIPopupBase PopupBase => Popup;
        public int Priority { get; private set; }
        public bool Overlap { get; private set; }
        public bool Single { get; private set; }
        public bool InstantShow { get;  private set; }
        public bool InstantHide { get;  private set; }
        public bool HideOrCancel { get; private set; }
        public bool BlockerShowCondition { get;  private set; }
        public Func<bool> ShowCondition { get;  private set; }
        public Func<bool> HideCondition { get;  private set; }
        public Func<bool> CancelCondition { get;  private set; }

        public T Popup { get; private set; }
        eElementState IPopupObserver<T>.State => Element.State;

        UIPopupBase IPopupObserver.Popup => Popup;
        eElementState IPopupObserver.State => Element.State;
        
        private IUIElement Element => Popup;
        private bool _canOverride = true;

        public PopupObserver(T element, bool instant)
        {
            Priority = 0;
            Single = false;
            InstantShow = instant;
            HideOrCancel = false;
            BlockerShowCondition = false;
            ShowCondition = () => true;
            HideCondition = () => false;
            CancelCondition = () => false;
            Popup = element;
        }

        void IPopupObserver<T>.Make(Action<T> action)
        {
            action?.Invoke(Popup);
        }

        void IPopupObserver<T>.Priority(int priority)
        {
            Priority = priority;
        }

        void IPopupObserver<T>.Single()
        {
            Single = true;
            throw new NotImplementedException();
        }

        void IPopupObserver<T>.Overlap()
        {
            Overlap = true;
        }

        void IPopupObserver<T>.ShowCondition(Func<T, bool> condition, bool blocker)
        {
            ShowCondition = () => condition(Popup);
            BlockerShowCondition = blocker;
        }
        
        void IPopupObserver<T>.HideCondition(Func<T, bool> condition, bool instant)
        {
            InstantHide = instant;
            HideCondition = () => condition(Popup);
        }
        void IPopupObserver<T>.CancelCondition(Func<T, bool> condition, bool instant)
        {
            InstantHide = instant;
            CancelCondition = () => condition(Popup);
        }

        void IPopupObserver<T>.Override(T popup)
        {
            if (_canOverride)
            {
                Popup = popup;
                
                if (Element.State == eElementState.NotInitialized)
                {
                    Element.Initialize();
                }
            }
        }
        
        void IPopupObserver<T>.Hide(bool instant)
        {
            HideOrCancel = true;
            InstantHide = instant;
        }

        void IPopupObserver<T>.Cancel()
        {
            HideOrCancel = true;
        }

        void IPopupObserver<T>.ListenCancel(Action<T> callback)
        {
            CancelComplete += callback;
        }

        void IPopupObserver<T>.ListenShowStart(Action<T> callback)
        {
            ShowStart += callback;
        }

        void IPopupObserver<T>.ListenShowComplete(Action<T> callback)
        {
            ShowComplete += callback;
        }
        
        void IPopupObserver<T>.ListenHideStart(Action<T> callback)
        {
            HideStart += callback;
        }
        
        void IPopupObserver<T>.ListenHideComplete(Action<T> callback)
        {
            HideComplete += callback;
        }
        
        
        void IPopupObserver.Hide(bool instant)
        {
            HideOrCancel = true;
            InstantHide = instant;
        }

        void IPopupObserver.ListenCancel(Action<UIPopupBase> callback)
        {
            CancelComplete += popup => callback?.Invoke(popup);
        }
        
        void IPopupObserver.ListenShowStart(Action<UIPopupBase> callback)
        {
            ShowStart += popup => callback?.Invoke(popup);
        }

        void IPopupObserver.ListenShowComplete(Action<UIPopupBase> callback)
        {
            ShowComplete += popup => callback?.Invoke(popup);
        }

        void IPopupObserver.ListenHideStart(Action<UIPopupBase> callback)
        {
            HideStart += popup => callback?.Invoke(popup);
        }

        void IPopupObserver.ListenHideComplete(Action<UIPopupBase> callback)
        {
            HideComplete += popup => callback?.Invoke(popup);
        }
        
        
        void IPopupController.Show(bool instant, Action callback)
        {
            _canOverride = false;
            ShowStart?.Invoke(Popup);
            Element.Show(instant, () =>
            {
                ShowComplete?.Invoke(Popup);
                callback?.Invoke();
            });
        }

        void IPopupController.Hide(bool instant,  Action callback)
        {
            HideStart?.Invoke(Popup);
            Element.Hide(instant, () =>
            {
                HideComplete?.Invoke(Popup);
                callback?.Invoke();
            });
        }
        
        void IPopupController.Release()
        {
            ReleaseControl?.Invoke(Popup);
            Reset();
        }

        void IPopupController.Cancel()
        {
            CancelComplete?.Invoke(Popup);
            Reset();
        }

        private void Reset()
        {
            ShowStart = null;
            ShowComplete = null;
            HideStart = null;
            HideComplete = null;
            CancelComplete = null;
            ReleaseControl = null;
            
            ShowCondition = null;
            HideCondition = null;
            CancelCondition = null;
            Popup = null;
        }
    }
}