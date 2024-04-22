using UnityEngine.Purchasing;

namespace Purchase.Base.Server
{
    public class PurchaseStartRequest
    {
        public string Id { get; }
        public Product Product { get; }
        
        public PurchaseStartRequest(string id, Product product)
        {
            Id = id;
            Product = product;
        }
    }
}