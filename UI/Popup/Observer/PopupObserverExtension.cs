using System;

namespace UI.Popup.Observer
{ 
    public static class PopupObserverExtension
    {
        // Generic API
        public static IPopupObserver<T> Prepare<T>(this IPopupObserver<T> observer, Action<T> prepare) where T : UIPopupBase
        {
            observer.Make(prepare);
            return observer;
        }

        public static IPopupObserver<T> SetPriority<T>(this IPopupObserver<T> observer, int priority) where T : UIPopupBase
        {
            observer.Priority(priority);
            return observer;
        }
        
        public static IPopupObserver<T> SetSingle<T>(this IPopupObserver<T> observer) where T : UIPopupBase
        {
            observer.Single();
            return observer;
        }
        
        public static IPopupObserver<T> SetOverlap<T>(this IPopupObserver<T> observer) where T : UIPopupBase
        {
            observer.Overlap();
            return observer;
        }

        
        public static IPopupObserver<T> SetShowCondition<T>(this IPopupObserver<T> observer, Func<T, bool> condition, bool blocker) where T : UIPopupBase
        {
            observer.ShowCondition(condition, blocker);
            return observer;
        }
        
        public static IPopupObserver<T> SetHideCondition<T>(this IPopupObserver<T> observer, Func<T, bool> condition, bool instant) where T : UIPopupBase
        {
            observer.HideCondition(condition, instant);
            return observer;
        }
        
        public static IPopupObserver<T> SetCancelCondition<T>(this IPopupObserver<T> observer, Func<T, bool> condition, bool instant) where T : UIPopupBase
        {
            observer.CancelCondition(condition, instant);
            return observer;
        }
        
        public static IPopupObserver<T> OverridePopup<T>(this IPopupObserver<T> observer, T popup) where T : UIPopupBase
        {
            observer.Override(popup);
            return observer;
        }

        public static IPopupObserver<T> Hide<T>(this IPopupObserver<T> observer, bool instant) where T : UIPopupBase
        {
            observer.Hide(instant);
            return observer;
        }

        public static IPopupObserver<T> Cancel<T>(this IPopupObserver<T> observer) where T : UIPopupBase
        {
            observer.Cancel();
            return observer;
        }
        
        public static IPopupObserver<T> OnCancel<T>(this IPopupObserver<T> observer, Action<T> callback) where T : UIPopupBase
        {
            observer.ListenCancel(callback);
            return observer;
        }
        
        public static IPopupObserver<T> OnShowStart<T>(this IPopupObserver<T> observer, Action<T> callback) where T : UIPopupBase
        {
            observer.ListenShowStart(callback);
            return observer;
        }

        public static IPopupObserver<T> OnShowComplete<T>(this IPopupObserver<T> observer, Action<T> callback) where T : UIPopupBase
        {
            observer.ListenShowComplete(callback);
            return observer;
        }

        public static IPopupObserver<T> OnHideStart<T>(this IPopupObserver<T> observer, Action<T> callback) where T : UIPopupBase
        {
            observer.ListenHideStart(callback);
            return observer;
        }

        public static IPopupObserver<T> OnHideComplete<T>(this IPopupObserver<T> observer, Action<T> callback) where T : UIPopupBase
        {
            observer.ListenHideComplete(callback);
            return observer;
        }
        
        
        // Non generic API
        public static IPopupObserver Hide(this IPopupObserver observer, bool instant)
        {
            observer.Hide(instant);
            return observer;
        }

        public static IPopupObserver OnCancel(this IPopupObserver observer, Action<UIPopupBase> callback)
        {
            observer.ListenCancel(callback);
            return observer;
        }
        
        public static IPopupObserver OnShowStart(this IPopupObserver observer, Action<UIPopupBase> callback)
        {
            observer.ListenShowStart(callback);
            return observer;
        }
        
        public static IPopupObserver OnShowComplete(this IPopupObserver observer, Action<UIPopupBase> callback)
        {
            observer.ListenShowComplete(callback);
            return observer;
        }
        
        public static IPopupObserver OnHideStart(this IPopupObserver observer, Action<UIPopupBase> callback)
        {
            observer.ListenHideStart(callback);
            return observer;
        }
        
        public static IPopupObserver OnHideComplete(this IPopupObserver observer, Action<UIPopupBase> callback)
        {
            observer.ListenHideComplete(callback);
            return observer;
        }
    }
}