using System;

namespace Purchase.Base.Server
{
    public interface IPurchaseProcess<out T>
    {
        event Action<T> Complete;
        
        bool Done { get; }
        T Result { get; }
    }
}