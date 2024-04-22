using System;

namespace UI.Base
{
    public interface IUIElement
    {
        string Id { get; }
        eElementState State { get; }

        void Initialize();
        void Show(bool instant, Action completeCallback);
        void Hide(bool instant, Action completeCallback);
    }
}