using UnityEngine.Purchasing;

namespace Purchase.Base.Server
{
    public class PurchaseFailRequest
    {
        public string Id { get; }
        public string Transaction { get; }
        public Product Product { get; }
        public PurchaseFailureReason Reason { get; }
        
        public PurchaseFailRequest(string id, string transaction, Product product, PurchaseFailureReason reason)
        {
            Id = id;
            Transaction = transaction;
            Product = product;
            Reason = reason;
        }
    }
}