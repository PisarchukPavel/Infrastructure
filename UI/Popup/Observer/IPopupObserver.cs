using System;
using UI.Base;

namespace UI.Popup.Observer
{
    public interface IPopupObserver<T> where T : UIPopupBase
    {
        T Popup { get; }
        eElementState State { get; }

        void Make(Action<T> action);
        void Priority(int priority);
        void Single();
        void Overlap();
        void Override(T popup);
        void ShowCondition(Func<T, bool> condition, bool blocker);
        void HideCondition(Func<T, bool> condition, bool instant);
        void CancelCondition(Func<T, bool> condition, bool instant);
        void Hide(bool instant);
        void Cancel();
        
        void ListenCancel(Action<T> callback);
        void ListenShowStart(Action<T> callback);
        void ListenShowComplete(Action<T> callback);
        void ListenHideStart(Action<T> callback);
        void ListenHideComplete(Action<T> callback);
    }

    public interface IPopupObserver
    {
        UIPopupBase Popup { get; }
        eElementState State { get; }
        
        void Hide(bool instant);

        void ListenCancel(Action<UIPopupBase> callback);
        void ListenShowStart(Action<UIPopupBase> callback);
        void ListenShowComplete(Action<UIPopupBase> callback);
        void ListenHideStart(Action<UIPopupBase> callback);
        void ListenHideComplete(Action<UIPopupBase> callback);
    }
}