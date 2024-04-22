using System;
using UI.Base;

namespace UI.Popup
{
    public class UIPopupBase : UIElementBase
    {
        public event Action<bool> HideRequest;
            
        protected void HideSelf(bool instant)
        {
            HideRequest?.Invoke(instant);
        }
    }
}