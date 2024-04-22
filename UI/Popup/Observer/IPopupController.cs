using System;

namespace UI.Popup.Observer
{
    // Internal usage only
    public interface IPopupController
    {
        UIPopupBase PopupBase { get; }
        int Priority { get; }
        bool Single { get; }
        bool Overlap { get; }
        bool InstantShow { get; }
        bool InstantHide { get; }
        bool HideOrCancel { get; }
        bool BlockerShowCondition { get; }
        Func<bool> ShowCondition { get; }
        Func<bool> HideCondition { get; }
        Func<bool> CancelCondition { get; }

        void Show(bool instant, Action callback);
        void Hide(bool instant, Action callback);
        void Release();
        void Cancel();
    }
}