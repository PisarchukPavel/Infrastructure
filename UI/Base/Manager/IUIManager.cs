using System;

namespace UI.Base.Manager
{
    public interface IUIManager<in T> where T : IUIElement
    {
        IDisposable Insert(T element);
    }
} 