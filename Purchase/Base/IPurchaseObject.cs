using Purchase.Base.Server;
using UnityEngine.Purchasing;

namespace Purchase.Base
{
    public interface IPurchaseObject
    {
        string Id { get; }
        string Product { get; }
        ProductType Type { get; }

        void OnAssign(Product product);
        void OnStart(PurchaseStartResponse context);
        void OnFail(PurchaseFailResponse context);
        void OnConfirm(PurchaseConfirmResponse context);
    }
}