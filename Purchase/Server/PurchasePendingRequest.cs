using UnityEngine.Purchasing;

namespace Purchase.Base.Server
{
    public class PurchasePendingRequest
    {
        public Product Product { get; }

        public PurchasePendingRequest(Product product)
        {
            Product = product;
        }
    }
}