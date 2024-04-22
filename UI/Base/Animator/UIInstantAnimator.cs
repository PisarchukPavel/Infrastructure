using System;
using UnityEngine;

namespace UI.Base.Animator
{
    public class UIInstantAnimator : UIAnimatorBase
    {
        [SerializeField] 
        private GameObject _rootObject = null;

        [SerializeField]
        private float _openDelay = 0.0f;
        
        [SerializeField]
        private float _closeDelay = 0.0f;

        private bool _destroyed = false;
        private GameObject Root => _rootObject != null ? _rootObject : gameObject;
        
        #if UNITY_EDITOR
        [ContextMenu("EmulateShow")]
        private void EmulateShow()
        {
            Root.SetActive(true);
        }
        
        [ContextMenu("EmulateHide")]
        private void EmulateHide()
        {
            Root.SetActive(false);
        }
        #endif
        
        public override void Show(bool instant, Action completeCallback)
        {
            if (instant)
            {
                Root.SetActive(true);
                completeCallback?.Invoke();
            }
            else
            {
                UITimer.Create(_openDelay, () =>
                {
                    Root.SetActive(true);
                    completeCallback?.Invoke();
                }, () => _destroyed);
            }
        }

        public override void Hide(bool instant, Action completeCallback)
        {
            if (instant)
            {
                Root.SetActive(false);
                completeCallback?.Invoke();
            }
            else
            {
                UITimer.Create(_closeDelay, () =>
                {
                    Root.SetActive(false);
                    completeCallback?.Invoke();
                }, () => _destroyed);
            }
        }

        private void OnDestroy()
        {
            _destroyed = true;
        }
    }
}