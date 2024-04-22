using UnityEngine;

namespace Bootstrap.Base.Operations
{
    public class DelayOperation : IOperation
    {
        bool IOperationStatus.Done => Time.time > _finish;
        float IOperationStatus.Progress => CalculateProgress();
        
        private float _start = 0.0f;
        private float _finish = 0.0f;
        private float _delay = 0.0f;

        public DelayOperation(float delay)
        {
            _delay = delay;
        }

        private float CalculateProgress()
        {
            float difference = Mathf.Clamp((_finish - Time.time), 0.0f, 1.0f);
            return 1 - (difference / _delay);
        }
        
        void IOperationStarter.Start()
        {
            _start = Time.time;
            _finish = _start + _delay;
        }
    }
}