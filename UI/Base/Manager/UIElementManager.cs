using System;

namespace UI.Base.Manager
{
    public class UIElementManager : UIManagerBase<UIElementBase>, IElementManager
    {
        public event Action<UIElementBase> Inserted;
        public event Action<UIElementBase> Extracted;
        
        protected override void OnInsert(UIElementBase element)
        {
            ((IUIElement)element).Initialize();
            Inserted?.Invoke(element);
        }

        protected override void OnExtract(UIElementBase element)
        {
            Extracted?.Invoke(element);
        }

        bool IElementManager.TryGet<TElement>(out TElement result, Predicate<TElement> predicate)
        {
            return Find(out result, predicate);
        }
    }
}