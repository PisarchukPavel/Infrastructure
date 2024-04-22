using System;
using System.Collections;
using Purchase.Base.Server;
using Purchase.Extension;
using UnityEngine;

namespace Purchase
{
    // Mock for IPurchaseServer with delay emulation
    public class PurchaseServer : IPurchaseServer
    {
        private readonly ResponseResult _responseSuccessResult = new ResponseResult(true, 200, "Mock success");
        private readonly ResponseResult _responseFailResult = new ResponseResult(false, 0, "Mock fail");
        
        private const float DELAY = 0.15f;
        
        IPurchaseProcess<PurchaseStartResponse> IPurchaseServer.Start(PurchaseStartRequest request)
        {
            PurchaseProcess<PurchaseStartResponse> result = new PurchaseProcess<PurchaseStartResponse>();
            
            DelayResult(() =>
            {
                result.Resolve(new PurchaseStartResponse(request, _responseSuccessResult, GenerateTransaction()));
            });
            
            return result;
        }

        IPurchaseProcess<PurchasePendingResponse> IPurchaseServer.Pending(PurchasePendingRequest request)
        {
            PurchaseProcess<PurchasePendingResponse> result = new PurchaseProcess<PurchasePendingResponse>();
            
            DelayResult(() =>
            {
                result.Resolve(new PurchasePendingResponse(request, _responseSuccessResult, GenerateTransaction()));
            });
            
            return result;
        }

        IPurchaseProcess<PurchaseFailResponse> IPurchaseServer.Fail(PurchaseFailRequest request)
        {
            PurchaseProcess<PurchaseFailResponse> result = new PurchaseProcess<PurchaseFailResponse>();
            
            DelayResult(() =>
            {
                result.Resolve(new PurchaseFailResponse(request, _responseFailResult));
            });
            
            return result;
        }

        IPurchaseProcess<PurchaseConfirmResponse> IPurchaseServer.Confirm(PurchaseConfirmRequest request)
        {
            PurchaseProcess<PurchaseConfirmResponse> result = new PurchaseProcess<PurchaseConfirmResponse>();
            
            DelayResult(() =>
            {
                result.Resolve(new PurchaseConfirmResponse(request, _responseSuccessResult));
            });
            
            return result;
        }

        private string GenerateTransaction()
        {
            return Guid.NewGuid().ToString().GetHashCode().ToString();
        }

        private void DelayResult(Action action)
        {
            PurchaseWorker.Process(DelayActionCoroutine(action));
        }
        
        private IEnumerator DelayActionCoroutine(Action action)
        {
            yield return new WaitForSeconds(DELAY);
            action?.Invoke();
        }
    }
}