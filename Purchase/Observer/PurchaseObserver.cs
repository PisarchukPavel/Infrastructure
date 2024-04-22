using System;
using System.Collections.Generic;
using Purchase.Base;
using Purchase.Base.Server;
using UnityEngine.Purchasing;

namespace Purchase
{
    public class PurchaseObserver : IPurchaseObserver
    {
        private event Action<PurchaseStartResponse> Start;
        private event Action<PurchasePendingResponse> Pending;
        private event Action<PurchaseFailResponse> Fail;
        private event Action<PurchaseConfirmResponse> Confirm;
        
        public string Id { get; } = null;
        public string ServerTransaction { get; private set; } = null;
        public ePurchaseInitiator Initiator { get; } = 0;
        public Product Product { get; } = null;

        private bool _released = false;
        private IPurchaseObject _target = null;
        private List<IDisposable> _disposers = null;
        
        public PurchaseObserver(IPurchaseObject target, Product product)
        {
            Id = target.Id;
            Product = product;
            Initiator = ePurchaseInitiator.User;
            
            _target = target;
            _disposers = new List<IDisposable>(4);
        }
        
        public PurchaseObserver(Product product)
        {
            Id = string.Empty;
            Product = product;
            Initiator = ePurchaseInitiator.Store;
            
            _target = null;
            _disposers = new List<IDisposable>(4);
        }
        
        IDisposable IPurchaseObserver.ListenStart(Action<PurchaseStartResponse> callback)
        {
            Verify();
            Start += callback;
            
            IDisposable disposer = new Disposer(() => Start -= callback);
            _disposers.Add(disposer);
            
            return disposer;
        }

        IDisposable IPurchaseObserver.ListenPending(Action<PurchasePendingResponse> callback)
        {
            Verify();
            Pending += callback;
            
            IDisposable disposer = new Disposer(() => Pending -= callback);
            _disposers.Add(disposer);
            
            return disposer;
        }

        IDisposable IPurchaseObserver.ListenFail(Action<PurchaseFailResponse> callback)
        {
            Verify();
            Fail += callback;
            
            IDisposable disposer = new Disposer(() => Fail -= callback);
            _disposers.Add(disposer);
            
            return disposer;
        }

        IDisposable IPurchaseObserver.ListenConfirm(Action<PurchaseConfirmResponse> callback)
        {
            Verify();
            Confirm += callback;
            
            IDisposable disposer = new Disposer(() => Confirm -= callback);
            _disposers.Add(disposer);
            
            return disposer;
        }

        public void SetServerTransaction(string id)
        {
            ServerTransaction = id;
        }
        
        public void PurchaseStart(PurchaseStartResponse context)
        {
            _target?.OnStart(context);
            Start?.Invoke(context);
        }

        public void PurchasePending(PurchasePendingResponse context)
        {
            Pending?.Invoke(context);
        }
        
        public void PurchaseFail(PurchaseFailResponse context)
        {
            _target?.OnFail(context);
            Fail?.Invoke(context);
        }
        
        public void PurchaseConfirm(PurchaseConfirmResponse context)
        {
            _target?.OnConfirm(context);
            Confirm?.Invoke(context);
        }

        private void Verify()
        {
            if(_released)
                throw new InvalidOperationException("Observer is already released");
        }
        
        public void Release()
        {
            _released = true;
            _target = null;
            
            _disposers.ForEach(x => x?.Dispose());
            _disposers.Clear();
            _disposers = null;
            
            Start = null;
            Pending = null;
            Fail = null;
            Confirm = null;
        }
        
        private class Disposer : IDisposable
        {
            private Action _action = null;

            public Disposer(Action action)
            {
                _action = action;
            }
            
            void IDisposable.Dispose()
            {
                _action?.Invoke();
                _action = null;
            }
        }
    }
}