using Purchase.Base;
using UnityEngine.Purchasing;

namespace Purchase.Extension
{
    public class ExtensionProvider : IPurchaseExtension, IExtensionProvider
    {
        private readonly IExtensionProvider _provider = null;
        
        public ExtensionProvider(IExtensionProvider provider)
        {
            _provider = provider;
        }

        public T GetExtension<T>() where T : IStoreExtension
        {
            return _provider.GetExtension<T>();
        }
    }
}