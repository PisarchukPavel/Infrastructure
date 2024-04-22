using System;
using System.Collections.Generic;
using Purchase.Base.Server;

namespace Purchase.Base
{
    public interface IPurchaseServices
    {
        event Action<IPurchaseObserver> Purchasing;
        eServicesStatus Status { get; }

        IPurchaseProcess<bool> Initialize(IPurchaseServer server, IEnumerable<IPurchaseObject> purchases, IEnumerable<IPurchaseExtension> extensions);
        IPurchaseObserver Purchase(string id);
        bool Extension<T>(out T result) where T : IPurchaseExtension;
        IPurchaseProcess<bool> Additional(IEnumerable<IPurchaseObject> purchases);
    }
}