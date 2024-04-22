using System.Globalization;
using System.Linq;
using Purchase.Base;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Purchase.Extension
{
    public class ProductExtension : IPurchaseExtension
    {
        private readonly string _productsLog = string.Empty;
        private readonly IStoreController _storeController = null;

        public ProductExtension(IStoreController controller)
        {
            _storeController = controller;
            foreach (Product product in _storeController.products.all)
            {
                _productsLog += $"{product.definition.id}, ";
            }

            if (_storeController.products.all.Length > 0)
            {
                _productsLog = _productsLog.Remove(_productsLog.Length - 2, 2);
            }
        }
        
        public string GetPriceString(string productId)
        {
            Product product = _storeController.products.WithID(productId) ?? _storeController.products.WithStoreSpecificID(_productsLog);
            if (product != null)
            {
                //Debug.Log($"[{nameof(ProductExtension)}] Find price string for {productId} in {_productsLog}");
                return product.metadata.localizedPriceString;
            }
            else
            {
                Debug.Log($"[{nameof(ProductExtension)}] Not find price string for {productId} in {_productsLog}");
                return productId;
            }
        }
        
        private string GetCurrencySymbol(string isoCurrencyCode)
        {
            string symbol = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture => 
                {
                    try
                    {
                        return new RegionInfo(culture.Name);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(ri => ri!=null && ri.ISOCurrencySymbol == isoCurrencyCode)
                .Select(ri => ri.CurrencySymbol)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(symbol))
            {
                symbol = isoCurrencyCode;
            }
            
            return symbol;
        }
    }
}