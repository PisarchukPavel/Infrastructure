using System;
using UI.Widget.Observer;

namespace UI.Widget
{
    public static class WidgetExtension
    {
        // Old API
        public static TElement Find<TElement>(this IWidgetManager widgetManager, Predicate<TElement> predicate = null) where TElement : UIWidgetBase
        {
            IWidgetObserver<TElement> observer = widgetManager.Get<TElement>(predicate);
            TElement widget = observer.Widget;
            observer.Release();
            return widget;
        }
        
        public static TElement Show<TElement>(this IWidgetManager widgetManager, bool instant = false, Predicate<TElement> predicate = null) where TElement : UIWidgetBase
        {
            IWidgetObserver<TElement> observer = widgetManager.Get<TElement>(predicate);
            TElement widget = observer.Widget;
            observer.Show(instant);
            observer.Release();
            return widget;
        }
        
        public static TElement Hide<TElement>(this IWidgetManager widgetManager, bool instant = false, Predicate<TElement> predicate = null) where TElement : UIWidgetBase
        {
            IWidgetObserver<TElement> observer = widgetManager.Get<TElement>(predicate);
            TElement widget = observer.Widget;
            observer.Hide(instant);
            observer.Release();
            return widget;
        }
        
        public static void Show<TElement>(this IWidgetManager widgetManager, bool instant, TElement element) where TElement : UIWidgetBase
        {
            widgetManager.Show<TElement>(instant, x => x == element);
        }

        public static void Hide<TElement>(this IWidgetManager widgetManager, bool instant, TElement element) where TElement : UIWidgetBase
        {
            widgetManager.Hide<TElement>(instant, x => x == element);
        }
    }
}