using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Purchase.Base;
using Purchase.Base.Server;
using Purchase.Extension;
using Purchase.Stores;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Purchase
{
    public class PurchaseServices : IPurchaseServices, IDetailedStoreListener
    {
        private event Action<IPurchaseObserver> PurchasingEvent;
        event Action<IPurchaseObserver> IPurchaseServices.Purchasing
        {
            add => PurchasingEvent += value;
            remove => PurchasingEvent -= value;
        }

        eServicesStatus IPurchaseServices.Status => _status;

        private IStoreController _storeController = null;
        private IExtensionProvider _extensionProvider = null;
      
        private eServicesStatus _status = eServicesStatus.WaitInitialize;
        private PurchaseProcess<bool> _initializeProcess = null;
        
        private StoreStrategy _store = null;
        private ConfigurationBuilder _builder = null;
        private IPurchaseServer _server = null;
        private List<IPurchaseObject> _purchases = null;
        private List<IPurchaseExtension> _extensionsStart = null;
        private List<IPurchaseExtension> _extensionsRuntime = null;

        private PurchaseObserver _current = null;

        IPurchaseProcess<bool> IPurchaseServices.Initialize(IPurchaseServer server, IEnumerable<IPurchaseObject> purchases, IEnumerable<IPurchaseExtension> extensions)
        {
            bool initializeAvailable = _status == eServicesStatus.WaitInitialize || _status == eServicesStatus.FailedInitialize;
            if(!initializeAvailable)
                throw new InvalidOperationException($"Can't initialize purchase services in {_status} status");
            
            _initializeProcess = new PurchaseProcess<bool>();
            _store = new StoreStrategy();
            _builder = ConfigurationBuilder.Instance(_store.PurchasingModule);
            _server = server;
            _purchases = purchases.ToList();
            _extensionsStart = extensions.ToList();
            _extensionsRuntime = new List<IPurchaseExtension>();

            string purchasesLog = string.Empty;
            List<ProductDefinition> productDefinitions = new List<ProductDefinition>();
            foreach (IPurchaseObject purchase in _purchases)
            {
                purchasesLog += $"[{purchase.Id} : {purchase.Product} {purchase.Type}] ";

                if(productDefinitions.Exists(x => x.id == purchase.Product))
                    continue;
                
                productDefinitions.Add(new ProductDefinition(purchase.Product, purchase.Type));
            }

            _builder.AddProducts(productDefinitions);
            _store.OnConfigBuild(_builder);
            
            Log($"Initialize purchases {purchasesLog}");
            
            ChangeStatus(eServicesStatus.ProcessingInitialize);
            UnityPurchasing.Initialize(this, _builder);

            return _initializeProcess;
        }

        IPurchaseObserver IPurchaseServices.Purchase(string id)
        {
            if (_status != eServicesStatus.Ready)
                throw new InvalidOperationException($"Can't purchase in {_status} status");

            IPurchaseObject purchaseObject = _purchases.Find(x => x.Id == id);
            if(purchaseObject == null)
                throw new InvalidOperationException($"Can't find {id} purchase object");
            
            Product product = _storeController.products.WithID(purchaseObject.Product);
            if(product == null)
                throw new InvalidOperationException($"Can't find {purchaseObject.Product} product");
            
            _current = CreateObserver(purchaseObject);
            
            ChangeStatus(eServicesStatus.Purchase);
            PurchasingEvent?.Invoke(_current);

            string productInfo = $"[{id} : {product.definition.id}]";
            Log($"Start {productInfo} purchase request to server");
            
            Delay(() =>
            {
                _server.Start(new PurchaseStartRequest(id, product)).Complete += response =>
                {
                    if (response.Result.Success)
                    {
                        Log($"Start {productInfo} purchase approved by server (server transaction is {response.Transaction}");

                        _current.SetServerTransaction(response.Transaction);
                        _current.PurchaseStart(response);
                        _storeController.InitiatePurchase(purchaseObject.Product);
                    }
                    else
                    {
                        Log($"Start {productInfo} purchase canceled by server because {response.Result.Message}");

                        _current.SetServerTransaction(response.Transaction);
                        _current.PurchaseStart(response);
                        _current.Release();
                        _current = null;
                        
                        ChangeStatus(eServicesStatus.Ready);
                    }
                };
            });
            
            return _current;
        }

        bool IPurchaseServices.Extension<T>(out T result)
        {
            result = (T)_extensionsRuntime.Find(x => x is T);
            return result != null;
        }

        IPurchaseProcess<bool> IPurchaseServices.Additional(IEnumerable<IPurchaseObject> purchases)
        {
            PurchaseProcess<bool> fetchProcess = new PurchaseProcess<bool>();
            List<IPurchaseObject> additionalPurchases = purchases.ToList();

            if (_status != eServicesStatus.Ready)
            {
                Log($"Can't purchase in {_status} status");
                fetchProcess.Resolve(false);
                return fetchProcess;
            }

            ChangeStatus(eServicesStatus.Additional);
            
            string purchasesLog = string.Empty;
            List<ProductDefinition> productDefinitions = new List<ProductDefinition>();
            foreach (IPurchaseObject purchase in additionalPurchases)
            {
                purchasesLog += $"[{purchase.Id} : {purchase.Product} {purchase.Type}] ";

                if(productDefinitions.Exists(x => x.id == purchase.Product) ||
                   _purchases.Exists(x => x.Product == purchase.Product))
                    continue;
                
                productDefinitions.Add(new ProductDefinition(purchase.Product, purchase.Type));
            }

            if (productDefinitions.Count > 0)
            {
#if UNITY_EDITOR
                _storeController.FetchAdditionalProducts(productDefinitions.ToHashSet(), 
                    () => Delay(SuccessFetch, 60), 
                    (reason, message) => Delay(() => FailFetch(reason, message), 60));
#else
                _storeController.FetchAdditionalProducts(productDefinitions.ToHashSet(), SuccessFetch, FailFetch);
#endif
            }
            else
            {
               SuccessFetch();
            }

            void SuccessFetch()
            {
                foreach (IPurchaseObject purchase in additionalPurchases)
                {
                    _purchases.Remove(_purchases.Find(x => x.Id == purchase.Id));
                    _purchases.Add(purchase);
                }
                
                PrepareExtensions();
                ResolvePurchases(additionalPurchases);
                
                ChangeStatus(eServicesStatus.Ready);
                fetchProcess.Resolve(true);
                
                Log($"Success fetch additional {purchasesLog} purchases");
            }

            void FailFetch(InitializationFailureReason reason, string message)
            {
                ChangeStatus(eServicesStatus.Ready);
                fetchProcess.Resolve(false);
                
                Log($"Fail fetch additional {purchasesLog} purchases because {reason} {message}");
            }
            
            return fetchProcess;
        }

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
            
            PrepareExtensions();
            ResolvePurchases(_purchases);
            
            ChangeStatus(eServicesStatus.Ready);
            _initializeProcess.Resolve(true);
            
            Log($"Initialize purchases success");
        }

        private void PrepareExtensions()
        {
            _extensionsRuntime = _store.CreateExtensions(_storeController, _extensionProvider).ToList();
            _extensionsRuntime.AddRange(_extensionsStart);
        }
        
        private void ResolvePurchases(IEnumerable<IPurchaseObject> purchases)
        {
            string log = string.Empty;
            foreach (IPurchaseObject purchaseObject in purchases)
            {
                Product product = _storeController.products.WithID(purchaseObject.Product);
                purchaseObject.OnAssign(product);

                log += $"({purchaseObject.Id} as {product.definition.id}) ";
            }
            
            Log($"Resolve purchases {log}");
        }
        
        void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message)
        {
            ChangeStatus(eServicesStatus.FailedInitialize);
            _initializeProcess.Resolve(false);
            
            Log($"Initialize purchases fail because {error} ({message})");
        }
        
        void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
        {
            ChangeStatus(eServicesStatus.FailedInitialize);
            _initializeProcess.Resolve(false);
            
            Log($"Initialize purchases fail because {error}");
        }
        
        PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            Delay(() => ProcessPurchaseImpl(purchaseEvent));
            return PurchaseProcessingResult.Pending;
        }

        private void ProcessPurchaseImpl(PurchaseEventArgs purchaseEvent)
        {
            Product product = purchaseEvent.purchasedProduct;
            
            bool notPending = _current != null && Product.Equals(_current.Product, product);
            string pendingPrefix = notPending ? string.Empty : "pending ";
            string transactionInfo = (notPending ? $"(server transaction is {_current.ServerTransaction}) " : string.Empty) + $"(store transaction is {product.transactionID})";
            string productInfo = "[" + (notPending ? $"{_current.Id} : " : string.Empty) + $"{product.definition.id}]";
            
            Log($"Store announced completion of the {productInfo} {pendingPrefix}purchase {transactionInfo}");

            if (notPending)
            {
                Log($"Waiting for {productInfo} purchase confirmation from the server {transactionInfo}");

                _server.Confirm(new PurchaseConfirmRequest(_current.ServerTransaction, product)).Complete += confirmResponse =>
                {
                    if (confirmResponse.Result.Success)
                    {
                        _storeController.ConfirmPendingPurchase(product);
                        
                        Log($"Complete {productInfo} purchase {transactionInfo}");
                    }
                    else
                    {
                        Log($"Fail {productInfo} purchase by server because {confirmResponse.Result.Message} {transactionInfo}");
                    }
                    
                    _current.PurchaseConfirm(confirmResponse);
                    _current.Release();
                    _current = null;
                    
                    ChangeStatus(eServicesStatus.Ready);
                };
            }
            else
            {
                Log($"Trying resolve {productInfo} pending purchase on the server {transactionInfo}");

                PurchaseObserver observer = CreateObserver(product);
                PurchasingEvent?.Invoke(observer);
                
                _server.Pending(new PurchasePendingRequest(product)).Complete += pendingResponse =>
                {
                    observer.SetServerTransaction(pendingResponse.Transaction);
                    
                    transactionInfo = $"(server transaction is {observer.ServerTransaction}) " + transactionInfo;
                    
                    if (pendingResponse.Result.Success)
                    {
                        _storeController.ConfirmPendingPurchase(product);

                        Log($"Complete {productInfo} pending purchase {transactionInfo}");
                    }
                    else
                    {
                        Log($"Fail {productInfo} pending purchase by server because {pendingResponse.Result.Message} {transactionInfo}");
                    }
                    
                    observer.PurchasePending(pendingResponse);
                    observer.Release();
                };
            }
        }
        
        void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            OnPurchaseFailedImpl(product, failureDescription.reason);
        }
        
        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailedImpl(product, failureReason);
        }

        private void OnPurchaseFailedImpl(Product product, PurchaseFailureReason reason)
        {
            bool notPending = Product.Equals(_current.Product, product);
            string transactionInfo = (notPending ? $"(server transaction is {_current.ServerTransaction}) " : string.Empty) + $"(store transaction is {product.transactionID})";
            string productInfo = "[" + (notPending ? $"{_current.Id} : " : string.Empty) + $"{product.definition.id}]";
            
            if (notPending)
            {
                Log($"Purchase {productInfo} failed by store because {reason} {transactionInfo}");

                _server.Fail(new PurchaseFailRequest(_current.Id, _current.ServerTransaction, product, reason)).Complete += response =>
                {
                    _current.PurchaseFail(response);
                    _current.Release();
                    _current = null;
                    
                    ChangeStatus(eServicesStatus.Ready);
                };
            }
            else
            {
                Log($"Pending {productInfo} purchase failed by store because {reason} {transactionInfo}");
            }
        }
        
        private PurchaseObserver CreateObserver(Product product)
        {
            return new PurchaseObserver(product);
        }
        
        private PurchaseObserver CreateObserver(IPurchaseObject purchaseObject)
        {
            Product product = _storeController.products.WithID(purchaseObject.Product);
            return new PurchaseObserver(purchaseObject, product);
        }
        
        private void ChangeStatus(eServicesStatus status)
        {
            _status = status;
        }
        
        private void Delay(Action continueCallback, int frames = 5)
        {
            PurchaseWorker.Process(DelayProcess(continueCallback, frames));
        }

        private IEnumerator DelayProcess(Action continueCallback, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            continueCallback?.Invoke();
        }

        private void Log(string message)
        {
            Debug.Log($"[{nameof(PurchaseServices)}] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[{nameof(PurchaseServices)}] {message}");
        }
    }
}