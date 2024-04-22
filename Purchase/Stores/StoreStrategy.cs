using System.Collections.Generic;
using Purchase.Base;
using Purchase.Extension;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Purchase.Stores
{
    public class StoreStrategy : IPurchasingModule
    {
        public IPurchasingModule PurchasingModule { get; }
        
        public StoreStrategy()
        {
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
            StandardPurchasingModule standardPurchasingModule = StandardPurchasingModule.Instance();
            PurchasingModule = standardPurchasingModule;
            standardPurchasingModule.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#else
            PurchasingModule = this;
#endif
        }
        
        void IPurchasingModule.Configure(IPurchasingBinder binder)
        {
            binder.RegisterStore(GetStoreName(), InstantiateStore());
        }

        public void OnConfigBuild(ConfigurationBuilder configurationBuilder)
        {
#if UNITY_EDITOR
            configurationBuilder.Configure<IMicrosoftConfiguration>().useMockBillingSystem = true;
#endif
        }
        
        public IReadOnlyList<IPurchaseExtension> CreateExtensions(IStoreController controller, IExtensionProvider extension)
        {
            List<IPurchaseExtension> result = new List<IPurchaseExtension>
            {
                new ProductExtension(controller),
                new ExtensionProvider(extension)
            };

            return result;
        }
        
        private IStore InstantiateStore() 
        {
#if UNITY_WEBGL
            return new Purchase.Stores.Web.WebGLStore();
#endif
            
#if UNITY_WSA
            return new Purchase.Stores.Microsoft.MicrosoftStoreCustom();
#endif
            return null;
        }

        private string GetStoreName()
        {
            string result = nameof(AppStore.fake);

#if UNITY_WEBGL
            result = "WebGL";
#endif
            
#if UNITY_WSA
            result = nameof(AppStore.WinRT);
#endif
            
            return result;
        }
    }
}