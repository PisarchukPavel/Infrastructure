namespace Purchase.Base
{
    public enum ePurchaseFailReason
    {
        PurchasingUnavailable,
        ExistingPurchasePending,
        ProductUnavailable,
        SignatureInvalid,
        UserCancelled,
        PaymentDeclined,
        DuplicateTransaction,
        Unknown,
        
        ServerReceiptReject,
        ServerUnavailable,
    }
}