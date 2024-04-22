using System;
using UI.Popup.Observer;

namespace UI.Popup
{
    public interface IPopupManager
    {
        IPopupObserver Current { get; }
        IDisposable Listen(Action<IPopupObserver> callback, Predicate<IPopupObserver> predicate = null);
        IPopupObserver<TElement> Show<TElement>(bool instant = false, Predicate<TElement> predicate = null) where TElement : UIPopupBase;
    }
}