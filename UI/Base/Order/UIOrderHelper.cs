using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityCanvas = UnityEngine.Canvas;
using UnityObject = UnityEngine.Object;

namespace UI.Base.Order
{
    [Serializable]
    public class UIOrderHelper
    {
        private bool _runtimeCreatedCanvas = false;
        private bool _startOverrideSorting = false;
        private int _startSortingOrder = 0;
        private int _increasesCount = 0;
        private int _sortingOrderValue = 0;
                
        private UnityCanvas _runtimeCanvas = null;
        private GraphicRaycaster _runtimeGraphicRaycaster = null;
        private RectTransform _runtimeRectTransform = null;
        private GameObject _gameObject = null;
        
        public UIOrderHelper(GameObject go, RectTransform rectTransform, UnityCanvas canvas = null)
        {
            _gameObject = go;
            _runtimeRectTransform = rectTransform;

            if (canvas == null)
            {
                canvas = rectTransform.GetComponent<UnityCanvas>();
            }
            
            if (canvas != null)
            {
                _runtimeCanvas = canvas;
                _startOverrideSorting = _runtimeCanvas.overrideSorting;
                _startSortingOrder = _runtimeCanvas.sortingOrder;
                _runtimeCreatedCanvas = false;
            }
            else
            {
                _runtimeCanvas = null;
                _startOverrideSorting = false;
                _startSortingOrder = 0;
                _runtimeCreatedCanvas = true;
            }
        }

        public int GetOrder(bool overlapped)
        {
            string overlapInfoLog = overlapped ? $"Overlapped" : $"Overlap";
            
            if (_runtimeCanvas != null)
            {
                Debug.Log($"[{nameof(UIOrderHelper)}] {overlapInfoLog} {_gameObject.name} return {_sortingOrderValue} order.", _gameObject);

                return _sortingOrderValue;
            }

            if (overlapped)
            {
                UnityCanvas[] canvases = _runtimeRectTransform.GetComponentsInParent<UnityCanvas>(false);

                foreach (UnityCanvas canvas in canvases)
                {
                    if (canvas.overrideSorting || canvas.isRootCanvas)
                    {
                        Debug.Log($"[{nameof(UIOrderHelper)}] {overlapInfoLog} {_gameObject.name} return parent {canvas.sortingOrder} order.", _gameObject);

                        return canvas.sortingOrder;
                    }
                }
            }

            Debug.Log($"[{nameof(UIOrderHelper)}] {overlapInfoLog} {_gameObject.name} return default (zero) order.", _gameObject);

            return 0;
        }
        
        public IDisposable IncreaseOrder(int value)
        {
            if (_runtimeCanvas == null)
            {
                UnityCanvas canvas = _runtimeRectTransform.gameObject.GetComponent<UnityCanvas>();
                if (canvas == null)
                {
                    canvas = _runtimeRectTransform.gameObject.AddComponent<UnityCanvas>();
                }

                GraphicRaycaster graphicRaycaster = _runtimeRectTransform.gameObject.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster == null)
                {
                    graphicRaycaster = _runtimeRectTransform.gameObject.AddComponent<GraphicRaycaster>();
                }
                
                _runtimeCanvas = canvas;
                _runtimeGraphicRaycaster = graphicRaycaster;
            }

            _increasesCount++;
            _sortingOrderValue += value;
            
            OrderEntry entry = new OrderEntry(_runtimeCanvas, value, () =>
            {
                _increasesCount--;
                _sortingOrderValue -= value;

                if (_increasesCount == 0)
                {
                    ResetCanvas();
                }
            });
            
            UIWorker.Process(EntryProcessor(entry));
            
            return entry;
        }
        
        private void ResetCanvas()
        {
            if(_runtimeCanvas == null)
                return;
            
            if (_runtimeCreatedCanvas)
            {
                UnityObject.Destroy(_runtimeGraphicRaycaster);
                UnityObject.Destroy(_runtimeCanvas);

                _runtimeCanvas = null;
                _runtimeGraphicRaycaster = null;
            }
            else
            {
                _runtimeCanvas.overrideSorting = _startOverrideSorting;
                _runtimeCanvas.sortingOrder = _startSortingOrder;
            }
        }

        private IEnumerator EntryProcessor(OrderEntry entry)
        {
            while (true)
            {
                if (entry.Destroyed)
                {
                    entry.Disable();
                    yield break;
                }

                if (entry.Ready)
                {
                    entry.Enable();
                    entry.Increment();
                    break;
                }
                
                yield return null;                   
            }

            while (true)
            {
                if (entry.Destroyed)
                {
                    entry.Disable();
                    yield break;
                }
                
                if (entry.Canceled)
                {
                    entry.Decrement();
                    entry.Disable();
                    yield break;
                }
                
                yield return null;
            }
        }
        
        private class OrderEntry : IDisposable
        {
            public bool Ready => CanEnableOverriderSorting();
            public bool Canceled => _canceled;
            public bool Destroyed => _canvas == null;
            
            private int _value = 0;
            private bool _canceled = false;
            private UnityCanvas _canvas = null;
            private Action _completeCallback = null;
            
            public OrderEntry(UnityCanvas canvas, int value, Action completeCallback)
            {
                _value = value;
                _canvas = canvas;
                _completeCallback = completeCallback;
            }

            public void Enable()
            {
                if(_canvas == null)
                    return;
                
                _canvas.overrideSorting = true;
            }

            public void Increment()
            {
                if(_canvas == null)
                    return;
                
                _canvas.sortingOrder += _value;
            }
            
            public void Decrement ()
            {
                if(_canvas == null)
                    return;
                
                _canvas.sortingOrder -= _value;
            }

            public void Disable()
            {
                _completeCallback?.Invoke();
                _completeCallback = null;
                _canvas = null;
            }
            
            private bool CanEnableOverriderSorting()
            {
                return (!_canvas.isRootCanvas && _canvas.gameObject.activeInHierarchy);
            }

            void IDisposable.Dispose()
            {
                _canceled = true;
            }
        }
    }
}