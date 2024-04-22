using System;
using System.Collections;
using UnityEngine;

namespace UI.Base.Animator
{
    public class UIMono : MonoBehaviour { }
    
    public class UITimer 
    {
        private float Delay { get; }
        private Action Callback { get; }

        private static MonoBehaviour _mono = null;
        
        static UITimer()
        {
            GameObject go = new GameObject();
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            _mono = go.AddComponent<UIMono>();
            GameObject.DontDestroyOnLoad(_mono);
        }

        public static void Create(float delay, Action callback, Func<bool> cancel = null)
        {
            UITimer timer = new UITimer(delay, callback, cancel);
        }
        
        private UITimer(float delay, Action callback, Func<bool> cancel)
        {
            Delay = delay;
            Callback = callback;

            _mono.StartCoroutine(Timer(cancel));
        }

        private IEnumerator Timer(Func<bool> cancel)
        {
            float time = Delay;

            while (time >= 0)
            {
                time -= Time.deltaTime;
                yield return null;

                if (cancel != null && cancel())
                {
                    yield break;
                }
            }
            
            if (cancel != null && cancel())
            {
                yield break;
            }
            
            Callback?.Invoke();
        }
    }
}