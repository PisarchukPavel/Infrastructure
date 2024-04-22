using System.Collections.Generic;
using System.Linq;

namespace Bootstrap.Base.Operations
{
    public class AsyncOperations : IOperation
    {
        bool IOperationStatus.Done => _operationWeights.Keys.All(x => x.Done);
        float IOperationStatus.Progress => CalculateProgress();
        
        public IReadOnlyList<IOperation> Operations => _operationWeights.Keys.ToList();
        
        private readonly Dictionary<IOperation, float> _operationWeights = new Dictionary<IOperation, float>();
        
        public AsyncOperations Append(IOperation operation, float weight = 1.0f)
        {
            _operationWeights.Add(operation, weight);
            return this;
        }

        void IOperationStarter.Start()
        {
            foreach (IOperation lifecycleOperation in _operationWeights.Keys)
            {
                lifecycleOperation.Start();
            }
        }

        private float CalculateProgress()
        {
            float currentWeight = 0.0f;
            float totalWeight = 0.0f;
            
            foreach (KeyValuePair<IOperation,float> kv in _operationWeights)
            {
                currentWeight += kv.Key.Progress * kv.Value;
                totalWeight += kv.Value;
            }
            
            return currentWeight / totalWeight;
        }
    }
}