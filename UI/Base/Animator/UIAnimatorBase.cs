using System;
using UnityEngine;

namespace UI.Base.Animator
{
    public abstract class UIAnimatorBase : MonoBehaviour
    {
        public abstract void Show(bool instant, Action completeCallback);
        public abstract void Hide(bool instant, Action completeCallback);
    }
}