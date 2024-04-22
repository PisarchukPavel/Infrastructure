using UnityEngine.Purchasing;

namespace Purchase.Base.Server
{
    public class PurchaseConfirmRequest
    {
        public string Transaction { get; }
        public Product Product { get; }
        
        public PurchaseConfirmRequest(string transaction, Product product)
        {
            Transaction = transaction;
            Product = product;
        }
    }
}