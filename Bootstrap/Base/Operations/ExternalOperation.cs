using System;

namespace Bootstrap.Base.Operations
{
    public class ExternalOperation : IOperation
    {
        bool IOperationStatus.Done => _doneCondition == null || _doneCondition.Invoke();
        float IOperationStatus.Progress => _done ? 0.0f : 1.0f;

        private readonly bool _done = false;
        private readonly Action _action = null;
        private readonly Func<bool> _doneCondition = null;

        public ExternalOperation(Action action, Func<bool> doneCondition = null)
        {
            _action = action;
            _doneCondition = doneCondition;
        }

        void IOperationStarter.Start()
        {
            _action?.Invoke();
        }
    }
}