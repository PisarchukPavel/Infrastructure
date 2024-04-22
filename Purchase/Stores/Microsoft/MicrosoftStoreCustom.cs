using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

#if ENABLE_WINMD_SUPPORT
using System.IO;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Storage;

#if !PROD
using MicrosoftApp = Windows.ApplicationModel.Store.CurrentAppSimulator;
#else
using MicrosoftApp = Windows.ApplicationModel.Store.CurrentApp;
#endif
#endif

namespace Purchase.Stores.Microsoft
{
    public class MicrosoftStoreCustom : IStore
    {
	    private IStoreCallback _storeCallback = null;
	    private string[] _storeProxyPath = new[] { Application.streamingAssetsPath, "UWP", "Config", "WindowsStoreProxy.xml" };
	    
        void IStore.Initialize(IStoreCallback callback)
        {
            _storeCallback = callback;
            WindowsStoreUtils.Create();
            Log("Initialize custom store");
        }
        
        void IStore.RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
        {
	        Log("RetrieveProducts start");

	        RetrieveProducts(products.Select(x => x.id).ToList(), productDescriptions =>
            {
	            foreach (ProductDescription description in productDescriptions)
	            {
		            Log($"RetrievedProduct: {description.storeSpecificId}");
	            }
	            _storeCallback.OnProductsRetrieved(productDescriptions);
            });
        }

        private void RetrieveProducts(IEnumerable<string> ids, Action<List<ProductDescription>> complete)
        {
	        RunOnOtherThread(() =>
	        {
		        RetrieveProductsAsync(ids, list =>
		        {
					RunOnMainThread(() =>
					{
						complete?.Invoke(list);
					});
		        });
	        });
        }
        
        private async void RetrieveProductsAsync(IEnumerable<string> ids, Action<List<ProductDescription>> complete)
        {
            List<ProductDescription> productDescriptions = new List<ProductDescription>();
            
#if ENABLE_WINMD_SUPPORT
#if !PROD
			try
			{
				string path = Path.Combine(_storeProxyPath);
				path = path.Replace("/", "\\");
				Log($"Retrieve products config path: {path}; Exist: {File.Exists(path)}");

				StorageFile file = await StorageFile.GetFileFromPathAsync(path);
				Log($"Retrieve products StorageFile ready");

				await MicrosoftApp.ReloadSimulatorAsync(file);
			}
			catch(Exception e)
			{
				Log($"Retrieve products fail because: {e.Message}");
				Log($"Retrieve products fail because: {e.StackTrace }");
			}

			foreach(string id in ids)
			{
				ProductDescription fakeDescription = CreateProductDescription(id, "$0.01", "Fake Title", "Fake Description", "USD", 0);
				productDescriptions.Add(fakeDescription);
			}
#else
			try
			{
				ListingInformation listingInformation = await MicrosoftApp.LoadListingInformationByProductIdsAsync(ids);
				foreach(ProductListing productListing in listingInformation.ProductListings.Values)
				{
					ProductDescription productDescription = CreateProductDescription(productListing.ProductId, productListing.FormattedPrice, productListing.GameplaySelectors, productListing.Description, productListing.CurrencyCode, ParsePrice(productListing.FormattedPrice));
					productDescriptions.Add(productDescription);
				}
			}
			catch(Exception e)
			{
				Log($"Retrieve products fail because: {e.Message}");
			}
#endif
#endif
	        
	        complete?.Invoke(productDescriptions);
        }
        
        void IStore.Purchase(ProductDefinition product, string developerPayload)
        {
	        Log($"Product purchase start: {product.storeSpecificId}");

	        PurchaseProduct(product, (receipt, transaction) =>
            {
	            Log($"Product purchase finish: {product.storeSpecificId}");
                _storeCallback.OnPurchaseSucceeded(product.storeSpecificId, receipt, transaction);
            }, failMessage =>
            {
	            Log($"Product purchase fail: {product.storeSpecificId} {failMessage}");
	            PurchaseFailureDescription description = new PurchaseFailureDescription(product.storeSpecificId, ParseError(failMessage), failMessage);
                _storeCallback.OnPurchaseFailed(description);
            });
        }

        private PurchaseFailureReason ParseError(string message)
        {
	        PurchaseFailureReason reason = PurchaseFailureReason.Unknown;
	        
	        switch (message)
	        {
		        case "NotPurchased":
			        reason = PurchaseFailureReason.UserCancelled;
			        break;
		        case "AlreadyPurchased":
			        reason = PurchaseFailureReason.ExistingPurchasePending;
			        break;
	        }

	        return reason;
        }
        
        private void PurchaseProduct(ProductDefinition product, Action<string, string> successCallback, Action<string> failCallback)
        {
	        RunOnOtherThread(() =>
	        {
		        PurchaseProductAsync(product, (receipt, transaction) =>
		        {
					RunOnMainThread(() =>
					{
						successCallback?.Invoke(receipt, transaction);
					});
		        }, failMessage =>
		        {
			        RunOnMainThread(() =>
			        {
				        failCallback?.Invoke(failMessage);
			        });
		        });
	        });
        }
        
        private async void PurchaseProductAsync(ProductDefinition product, Action<string, string> successCallback, Action<string> failCallback)
        {					
#if ENABLE_WINMD_SUPPORT
			try
			{
				PurchaseResults purchase = await MicrosoftApp.RequestProductPurchaseAsync(product.storeSpecificId);
				switch (purchase.Status)
				{
					case ProductPurchaseStatus.Succeeded:
						successCallback?.Invoke(purchase.ReceiptXml, purchase.TransactionId.ToString());
						break;
					case ProductPurchaseStatus.NotFulfilled:
					case ProductPurchaseStatus.AlreadyPurchased:
					case ProductPurchaseStatus.NotPurchased:
						failCallback?.Invoke(purchase.Status.ToString());
						break;
					}
			}
			catch(Exception e)
			{
				failCallback?.Invoke(e.Message);
			}
#else
	        successCallback?.Invoke("Fake XML Receipt", "Fake transaction");
#endif
        }

        void IStore.FinishTransaction(ProductDefinition product, string transactionId)
        {
	        FinishPurchase(product, transactionId);
        }

        private void FinishPurchase(ProductDefinition product, string transactionId)
        {
	        RunOnOtherThread(() =>
	        {
		        FinishPurchaseAsync(product, transactionId);
	        });
        }
        
        private async void FinishPurchaseAsync(ProductDefinition product, string transactionId)
        {
#if ENABLE_WINMD_SUPPORT
			try
            {
                var result = await MicrosoftApp.ReportConsumableFulfillmentAsync(product.storeSpecificId, Guid.Parse(transactionId));
            }
            catch (Exception e)
            {
				Log($"Finish purchase fail because: {e.Message}");
            }
#endif
        }

        private void Log(string message)
        {
	        UnityEngine.Debug.Log($"[{nameof(MicrosoftStoreCustom)}] {message}");
        }

        
        private void RunOnMainThread(Action action)
        {
	        WindowsStoreUtils.RunOnMainThread(action);
        }
        
        private void RunOnOtherThread(Action action)
        {
#if ENABLE_WINMD_SUPPORT
	        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
	        {
		        action?.Invoke();
	        });
#else
	        action?.Invoke();
#endif
        }
        
        private ProductDescription CreateProductDescription(string id, string priceString, string title, string description, string currencyCode, decimal localizedPrice)
        {
            ProductMetadata metadata = new ProductMetadata(priceString, title, description, currencyCode, localizedPrice);
            ProductDescription productDescription = new ProductDescription(id, metadata);
            return productDescription;
        }
        
        private decimal ParsePrice(string formattedPrice)
        {
            decimal price = 0;
            decimal.TryParse(formattedPrice, NumberStyles.Currency, CultureInfo.CurrentCulture, out price);
            return price;
        }
    }
}