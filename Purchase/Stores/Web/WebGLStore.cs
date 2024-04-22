using System;
using System.Collections.ObjectModel;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Purchase.Stores.Web
{
    public class WebGLStore : IStore
    {
        private IStoreCallback _storeCallback = null;
        
        void IStore.Initialize(IStoreCallback callback)
        {
            _storeCallback = callback;
        }

        void IStore.RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
        {
            throw new NotImplementedException();
        }

        void IStore.Purchase(ProductDefinition product, string developerPayload)
        {
            throw new NotImplementedException();
        }

        void IStore.FinishTransaction(ProductDefinition product, string transactionId)
        {
            throw new NotImplementedException();
        }
    }
}