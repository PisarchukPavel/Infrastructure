namespace Purchase.Base.Server
{
    public interface IPurchaseServer
    {
        IPurchaseProcess<PurchaseStartResponse> Start(PurchaseStartRequest request);
        IPurchaseProcess<PurchasePendingResponse> Pending(PurchasePendingRequest request);
        IPurchaseProcess<PurchaseFailResponse> Fail(PurchaseFailRequest request);
        IPurchaseProcess<PurchaseConfirmResponse> Confirm(PurchaseConfirmRequest request);
    }
}