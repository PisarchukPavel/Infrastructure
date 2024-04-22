using System;
using UI.Base;

namespace UI.Widget.Observer
{
    public class WidgetObserver<T> : IWidgetObserver<T> where T : UIWidgetBase
    {
        public event Action<T> Release;
        
        private event Action<T> ShowStart;
        private event Action<T> ShowComplete;
        private event Action<T> HideStart;
        private event Action<T> HideComplete;
        private event Action<T> ReleaseControl;

        T IWidgetObserver<T>.Widget => Widget;
        private IUIElement Element => Widget;
        private T Widget { get; set; }

        public WidgetObserver(T widget)
        {
            Widget = widget;
            ReleaseControl += Release;
        }

        void IWidgetObserver<T>.Show(bool instant)
        {
            ShowStart?.Invoke(Widget);
            ShowStart = null;
            
            Element.Show(instant, () =>
            {
                ShowComplete?.Invoke(Widget);
                ShowComplete = null;
            });
        }

        void IWidgetObserver<T>.Hide(bool instant)
        {
            HideStart?.Invoke(Widget);
            HideStart = null;
            
            Element.Hide(instant, () =>
            {
                HideComplete?.Invoke(Widget);
                HideComplete = null;
            });
        }

        void IWidgetObserver<T>.ListenShowStart(Action<T> callback)
        {
            ShowStart += callback;
        }

        void IWidgetObserver<T>.ListenShowComplete(Action<T> callback)
        {
            ShowComplete += callback;
        }

        void IWidgetObserver<T>.ListenHideStart(Action<T> callback)
        {
            HideStart += callback;
        }

        void IWidgetObserver<T>.ListenHideComplete(Action<T> callback)
        {
            HideComplete += callback;
        }
        
        void IWidgetObserver<T>.Release()
        {
            ReleaseControl?.Invoke(Widget);

            Widget = null;
            ReleaseControl = null;
            ShowStart = null;
            ShowComplete = null;
            HideStart = null;
            HideComplete = null;
        }
    }
}