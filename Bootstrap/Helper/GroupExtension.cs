using System;
using System.Collections.Generic;
using Bootstrap.Base;
using Bootstrap.Base.Operations;

namespace Bootstrap.Helper
{
    public static class GroupExtension
    {
        public static OperationGroup AppendSilent(this OperationGroup group, IOperation operation)
        {
            group.AppendOperation(operation, -1, 0.0f);
            return group;
        }

        public static OperationGroup AppendSilent(this OperationGroup group, List<IOperation> operations)
        {
            group.Append(operations, 0.0f);
            return group;
        }
        
        public static OperationGroup Append(this OperationGroup group, List<IOperation> operations, float totalWeight = 1.0f)
        {
            float weight = totalWeight / operations.Count;
            foreach (IOperation operation in operations)
            {
                group.Append(operation, weight);
            }
            
            return group;
        }
        
        public static OperationGroup Append(this OperationGroup group, OperationGroup other)
        {
            group.ForkGroup(() => true, other);
            return group;
        }
        
        public static OperationGroup Append(this OperationGroup group, IOperation operation, float weight = 1.0f)
        {
            group.AppendOperation(operation, -1, weight);
            return group;
        }
        
        public static OperationGroup Append(this OperationGroup group, Action action, Func<bool> doneCondition = null)
        {
            group.Append(new ExternalOperation(action, doneCondition));
            return group;
        }
        
        public static OperationGroup AppendAsync(this OperationGroup group, params IOperation[] operations)
        {
            AsyncOperations asyncOperations = new AsyncOperations();
            foreach (IOperation operation in operations)
            {
                asyncOperations.Append(operation);
            }
            group.Append(asyncOperations);
            return group;
        }
        
        public static OperationGroup Delay(this OperationGroup group, float delay)
        {
            group.Append(new DelayOperation(delay));
            return group;
        }

        public static OperationGroup Interval(this OperationGroup group, float progress01)
        {
            group.IntervalGroup(progress01);
            return group;
        }

        public static OperationGroup Fork(this OperationGroup group, Func<bool> condition, OperationGroup trueGroup = null, OperationGroup falseGroup = null, bool normalizeInterval = true)
        {
            group.ForkGroup(condition, trueGroup, falseGroup, normalizeInterval);
            return group;
        }
        
        public static OperationGroup AppendConditional(this OperationGroup group, Func<bool> condition, IOperation operation)
        {
            group.ForkOperation(condition, operation, null);
            return group;
        }
        
        private static OperationGroup ForkOperation(this OperationGroup group, Func<bool> condition, IOperation trueOperation = null, IOperation falseOperation = null)
        {
            OperationGroup trueGroup = trueOperation == null ? null : new OperationGroup().Append(trueOperation);
            OperationGroup falseGroup = falseOperation == null ? null : new OperationGroup().Append(falseOperation);
            group.Fork(condition, trueGroup, falseGroup, false);
            return group;
        }
    }
}