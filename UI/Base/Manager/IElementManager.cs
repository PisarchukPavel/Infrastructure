using System;

namespace UI.Base.Manager
{
    public interface IElementManager
    {
        event Action<UIElementBase> Inserted;
        event Action<UIElementBase> Extracted;
        
        bool TryGet<TElement>(out TElement result, Predicate<TElement> predicate = null) where TElement : UIElementBase;
    }
}