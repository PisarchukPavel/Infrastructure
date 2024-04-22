using System;
using UI.Widget.Observer;

namespace UI.Widget
{
    public interface IWidgetManager
    {
        IWidgetObserver<TElement> Get<TElement>(Predicate<TElement> predicate = null) where TElement : UIWidgetBase;
    }
}