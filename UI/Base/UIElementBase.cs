using System;
using System.Collections.Generic;
using System.Linq;
using UI.Base.Animator;
using UI.Base.Order;
using UnityEngine;
using UnityCanvas = UnityEngine.Canvas;

namespace UI.Base
{
    public class UIElementBase : MonoBehaviour, IUIElement
    {
        private event Action<bool> ShowStart;
        private event  Action<bool> ShowComplete;
        private event  Action<bool> HideStart;
        private event  Action<bool> HideComplete;
        private event Action<eElementState> StateChange;

        public string Identifier => _identifier;
        
        string IUIElement.Id => _identifier;
        eElementState IUIElement.State => _state;
        protected RectTransform RectTransform => _runtimeRect;
        
        [SerializeField] 
        private bool _showStart = false;

        [SerializeField] 
        private string _identifier = string.Empty;
        
        [SerializeField]
        private UIAnimatorBase _animator = null;

        [SerializeField] 
        private RectTransform _overridedRoot = null;

        [SerializeField] 
        private RectTransform _cornerRoot = null;

        [SerializeField] 
        private UnityCanvas _canvas = null;

        [SerializeField] 
        private List<UIElementBase> _subElements = new List<UIElementBase>();
        
        private bool _initialized = false;
        private eElementState _state = eElementState.NotInitialized;
        private RectTransform _runtimeRect = null;
        private RectTransform _cornerRuntimeRect = null;
        private UIOrderHelper _orderHelper = null;
        
        private static readonly Dictionary<eElementState, List<eElementState>> AVAILABLE_TRANSITIONS = null;

        static UIElementBase()
        {
            AVAILABLE_TRANSITIONS = new Dictionary<eElementState, List<eElementState>>
            {
                { eElementState.NotInitialized, new List<eElementState>() { eElementState.Hided, eElementState.Hiding, eElementState.Showing, eElementState.Showed } },
                { eElementState.Hided, new List<eElementState>() { eElementState.Showing } },
                { eElementState.Hiding, new List<eElementState>() { eElementState.Hided, eElementState.Showing } },
                { eElementState.Showed, new List<eElementState>() { eElementState.Hiding } },
                { eElementState.Showing, new List<eElementState>() { eElementState.Showed, eElementState.Hiding } }
            };
        }
        
#if UNITY_EDITOR
        [ContextMenu("Show")]
        private void ShowItem()
        {
            ShowImpl(false);
        }
        
        [ContextMenu("Hide")]
        private void HideItem()
        {
            HideImpl(false);
        }
#endif
        
        public void Initialize()
        {
            LazyInitialize();
        }
        
        void IUIElement.Initialize()
        {
            LazyInitialize();
        }

        void IUIElement.Show(bool instant, Action completeCallback)
        {
            ShowImpl(instant, completeCallback);
        }
        
        void IUIElement.Hide(bool instant, Action completeCallback)
        {
            HideImpl(instant, completeCallback);
        }

        protected void ShowImpl(bool instant, Action completeCallback = null)
        {
            LazyInitialize();
            ChangeStateImpl(eElementState.Showing);
            
            ShowStart?.Invoke(instant);
            _animator.Show(instant, () =>
            {
                ChangeStateImpl(eElementState.Showed);
                ShowComplete?.Invoke(instant);
                completeCallback?.Invoke();
            });
            
            _subElements.ForEach(x => x.ShowImpl(instant));
        }
        
        protected void HideImpl(bool instant, Action completeCallback = null)
        {
            LazyInitialize();
            ChangeStateImpl(eElementState.Hiding);
            
            HideStart?.Invoke(instant);
            _animator.Hide(instant, () =>
            {
                ChangeStateImpl(eElementState.Hided);
                HideComplete?.Invoke(instant);
                completeCallback?.Invoke();
            });
            
            _subElements.ForEach(x => x.HideImpl(instant));
        }

        private void LazyInitialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                InitializeImpl();
            } 
        }
        
        private void InitializeImpl()
        {
            _initialized = true;
            //_id = Guid.NewGuid().ToString("N");
            _animator = (_animator != null) ? _animator : gameObject.GetComponent<UIAnimatorBase>();
            _animator = (_animator != null) ? _animator : gameObject.AddComponent<UIInstantAnimator>();
            _runtimeRect = _overridedRoot != null ? _overridedRoot : GetComponent<RectTransform>();
            _cornerRuntimeRect = _cornerRoot != null ? _cornerRoot : _runtimeRect;
            _orderHelper = new UIOrderHelper(_runtimeRect.gameObject, _runtimeRect, _canvas);
            
            if (_showStart)
            {
                _animator.Show(true, () => {});
                ChangeStateImpl(eElementState.Showed);
            }
            else
            {
                _animator.Hide(true, () => {});
                ChangeStateImpl(eElementState.Hided);
            }
            
            ShowStart += OnShowStart;
            ShowComplete += OnShowComplete;
            HideStart += OnHideStart;
            HideComplete += OnHideComplete;
            StateChange += OnStateChanged;
            
            _subElements.ForEach(x => x.LazyInitialize());
            
            OnInitialized();
        }
        
        private void ChangeStateImpl(eElementState nextState)
        {
            //if (!CanTransitions(_state, nextState))
                //Debug.LogError($"[{nameof(UIElementBase)}] Unavailable transition from {_state} to {nextState}", gameObject);   
            
            _state = nextState;
            StateChange?.Invoke(_state);
        }
        
        private bool CanTransitions(eElementState from, eElementState to)
        {
            return AVAILABLE_TRANSITIONS[from].Contains(to);
        }

        protected virtual void OnInitialized() { }
        protected virtual void OnShowStart(bool instant) { }
        protected virtual void OnShowComplete(bool instant) { }
        protected virtual void OnHideStart(bool instant) { }
        protected virtual void OnHideComplete(bool instant) { }
        protected virtual void OnStateChanged(eElementState state) { }
        
        public IDisposable OverlapOrder(UIElementBase overlappedElement, int overlapBy = 1)
        {
            LazyInitialize();
            
            overlapBy = Mathf.Clamp(overlapBy, 0, int.MaxValue);
            return IncreaseOrder(overlappedElement._orderHelper.GetOrder(true) - _orderHelper.GetOrder(false) + overlapBy);
        }
        
        public IDisposable IncreaseOrder(int increaseValue)
        {
            LazyInitialize();

            increaseValue = Mathf.Clamp(increaseValue, 0, int.MaxValue);
            return _orderHelper.IncreaseOrder(increaseValue);
        }
        
        /// <summary>
        /// Segments is [0,1,2,3] started from Left-Bot, moved clockwise.
        /// </summary>
        public Vector2 GetContourPosition(int segment, float path, bool consideringTransformation)
        {
            Vector3[] corners = GetCorners(consideringTransformation);

            int indexFrom = segment;
            int indexTo = segment == 3 ? 0 : segment + 1;

            Vector3 startPos = corners[indexFrom];
            Vector3 finishPos = corners[indexTo];

            float x = Mathf.Lerp(startPos.x, finishPos.x, path);
            float y = Mathf.Lerp(startPos.y, finishPos.y, path);
            
            return new Vector2(x, y);
        }
        
        public Vector2 GetAreaPosition(float xPath, float yPath, bool consideringTransformation)
        {
            Vector3[] corners = GetCorners(consideringTransformation);

            float x = Mathf.Lerp(corners[0].x, corners[2].x, xPath);
            float y = Mathf.Lerp(corners[3].y, corners[1].y, yPath);
            
            return new Vector2(x, y);
        }

        public Vector2 GetPosition()
        {
            return GetAreaPosition(0.5f, 0.5f, true);
        }
        
        public Vector2 GetSize()
        {
            return _cornerRuntimeRect.sizeDelta;
        }
        
        public void ParentTo(UIElementBase target)
        {
            RectTransform.SetParent(target.RectTransform, true);
        }

        private Vector3[] GetCorners(bool consideringTransformation)
        {
            Vector3[] corners = new Vector3[4];
            _cornerRuntimeRect.GetWorldCorners(corners);

            if (consideringTransformation)
            {
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;
                
                foreach (Vector3 corner in corners)
                {
                    if (corner.x < minX)
                        minX = corner.x;

                    if (corner.y < minY)
                        minY = corner.y;
                    
                    if (corner.x > maxX)
                        maxX = corner.x;

                    if (corner.y > maxY)
                        maxY = corner.y;
                }

                Vector3 minPoint = new Vector2(minX, minY);
                Vector3 maxPoint = new Vector2(maxX, maxY);

                float minDist1 = float.MaxValue;
                float minDist2 = float.MaxValue;
                
                Vector3 firstPoint = Vector2.zero;
                Vector3 thirdPoint = Vector2.zero;
                
                foreach (Vector3 corner in corners)
                {
                    float distanceBetweenMinPoint = Vector2.Distance(minPoint, corner);
                    float distanceBetweenMaxPoint = Vector2.Distance(maxPoint, corner);
                    
                    if (distanceBetweenMinPoint < minDist1)
                    {
                        minDist1 = distanceBetweenMinPoint;
                        firstPoint = corner;
                    }

                    if (distanceBetweenMaxPoint < minDist2)
                    {
                        minDist2 = distanceBetweenMaxPoint;
                        thirdPoint = corner;
                    }
                }

                // TODO Remove linq, change to for/if logic
                List<Vector3> otherCorners = corners.Where(x => x != firstPoint && x != thirdPoint).OrderBy(x => x.y).ToList();

                corners[0] = firstPoint;
                corners[1] = otherCorners[1];
                corners[2] = thirdPoint;
                corners[3] = otherCorners[0];
            }

            return corners;
        }
    }
}