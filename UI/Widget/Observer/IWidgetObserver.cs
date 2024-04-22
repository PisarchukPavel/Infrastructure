using System;

namespace UI.Widget.Observer
{
    public interface IWidgetObserver<out T> where T : UIWidgetBase
    {
        T Widget { get; }

        void Show(bool instant);
        void Hide(bool instant);
        void Release();
        
        void ListenShowStart(Action<T> callback);
        void ListenShowComplete(Action<T> callback);
        void ListenHideStart(Action<T> callback);
        void ListenHideComplete(Action<T> callback);
    }
}