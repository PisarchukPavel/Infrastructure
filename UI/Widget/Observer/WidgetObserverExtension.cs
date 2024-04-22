using System;

namespace UI.Widget.Observer
{
    public static class WidgetObserverExtension
    {
        public static IWidgetObserver<T> OnShowStart<T>(this IWidgetObserver<T> observer, Action<T> callback) where T : UIWidgetBase
        {
            observer.ListenShowStart(callback);
            return observer;
        }

        public static IWidgetObserver<T> OnShowComplete<T>(this IWidgetObserver<T> observer, Action<T> callback) where T : UIWidgetBase
        {
            observer.ListenShowComplete(callback);
            return observer;
        }

        public static IWidgetObserver<T> OnHideStart<T>(this IWidgetObserver<T> observer, Action<T> callback) where T : UIWidgetBase
        {
            observer.ListenHideStart(callback);
            return observer;
        }

        public static IWidgetObserver<T> OnHideComplete<T>(this IWidgetObserver<T> observer, Action<T> callback) where T : UIWidgetBase
        {
            observer.ListenHideComplete(callback);
            return observer;
        }
    }
}