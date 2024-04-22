using System;
using Bootstrap.Base;
using UnityEngine;
using UnityEngine.UI;
using Randomizer = UnityEngine.Random;

namespace Bootstrap.Helperqq
{
    public class GroupProgress : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve _acceleration = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
        
        [SerializeField] 
        private float _speed = 1.0f;

        [SerializeField]
        [Range(0.25f, 0.75f)]
        private float _noise = 0.5f;
        
        [SerializeField] 
        private Image _progressor = null;

        private OperationGroup _group = null;
        
        public void Assign(OperationGroup group)
        {
            if(_group == group)
                return;

            RebootImpl();
            
            _group = group;
            _group.Complete += OnComplete;
        }

        public void Reboot()
        {
            RebootImpl();
        }

        public void Adjust(float progress, bool instant)
        {
            Refresh(progress, instant);
        }

        private void RebootImpl()
        {
            if (_group != null)
            {
                _group.Complete -= OnComplete;
            }

            _group = null;
            
            Refresh(0.0f, true);
        }
        
        private void OnComplete()
        {
            _group.Complete -= OnComplete;
            _group = null;
        }

        private void Update()
        {
            if (_group != null)
            {
                Refresh(_group.Progress, false);
            }
        }

        private void Refresh(float progress, bool instant)
        {
            if (_progressor != null)
            {
                float noise = _acceleration.Evaluate(progress) * Randomizer.Range(1.0f - _noise, 1.0f + _noise);
                float calculateProgress = Mathf.MoveTowards(_progressor.fillAmount, progress, _speed * noise * Time.deltaTime);
                float resultProgress = Mathf.Clamp(instant ? progress : calculateProgress, 0.0f, 1.0f);
                _progressor.fillAmount = resultProgress;
            }
        }
    }
}