using System;
using Purchase.Base.Server;
using UnityEngine.Purchasing;

namespace Purchase.Base
{
    public interface IPurchaseObserver
    {
        ePurchaseInitiator Initiator { get; }
        Product Product { get; }
        
        IDisposable ListenStart(Action<PurchaseStartResponse> callback);
        IDisposable ListenPending(Action<PurchasePendingResponse> callback);
        IDisposable ListenFail(Action<PurchaseFailResponse> callback);
        IDisposable ListenConfirm(Action<PurchaseConfirmResponse> callback);
    }
}