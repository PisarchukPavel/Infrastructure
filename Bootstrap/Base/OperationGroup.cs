using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bootstrap.Base.Operations;
using Bootstrap.Helper;
using UnityEngine;

namespace Bootstrap.Base
{
    public class OperationGroup
    {
        public event Action Complete;
        public event Action<IOperationStatus> Next;

        public bool Done => _done;
        public float Progress => _progress;
        
        private bool _started = false;
        private bool _done = false;
        private float _progress  = 0.0f;

        private int _currentNodeIndex = 0;
        private int _leftIntervalIndex = 0;
        private int _rightIntervalIndex = 0;
        private Vector2 _currentInterval = Vector2.zero;

        private float _currentIntervalSum = 0.0f;
        private float _totalIntervalSum = 0.0f;
        
        private List<LifecycleNode> _nodes = new List<LifecycleNode>();
        private List<IOperation> _tempOperations = new List<IOperation>();

        private IDisposable _canceler = null;
        
        public void AppendOperation(IOperation lifecycleOperation, int index, float weight = 1.0f)
        {
            VerifyStatus();
            index = (index == -1) ? _nodes.Count : index;
            _nodes.Insert(index, new LifecycleNode(eNodeType.Operation, lifecycleOperation, weight));
        }

        public void IntervalGroup(float progress01)
        {
            VerifyStatus();
            _nodes.Add(new LifecycleNode(eNodeType.Interval, progress01));
        }

        public void ForkGroup(Func<bool> condition, OperationGroup trueGroup = null, OperationGroup falseGroup = null, bool normalizeInterval = true)
        {
            VerifyStatus();
            _nodes.Add(new LifecycleNode(eNodeType.Fork, condition, trueGroup, falseGroup, normalizeInterval));
        }

        public void Run()
        {
            VerifyStatus();
            StartImpl();
        }

        public void Abort()
        {
            Dispose();
        }
        
        private void VerifyStatus()
        {
            if (_started)
                throw new InvalidOperationException($"[{nameof(OperationGroup)}] Can't modify {nameof(OperationGroup)} after launch");
        }
        
        private void StartImpl()
        {
            _started = true;
            AddBasicInterval();
            _canceler = GroupWorker.Process(RunCoroutine());
        }

        private void AddBasicInterval()
        {
            _nodes.Insert(0, new LifecycleNode(eNodeType.Interval, 0.0f));
            _nodes.Add(new LifecycleNode(eNodeType.Interval, 1.0f));
        }
        
        private IEnumerator RunCoroutine()
        {
            Debug.Log($"[{nameof(OperationGroup)}] Start load group");
            System.Diagnostics.Stopwatch sp = new System.Diagnostics.Stopwatch();
            sp.Start();

            for (int i = 0; i < _nodes.Count; i++)
            {
                _currentNodeIndex = i;
                
                switch (_nodes[_currentNodeIndex].Type)
                {
                    case eNodeType.Operation:
                        yield return ProcessOperationNode();
                        break;
                    case eNodeType.Interval:
                        RefreshInterval();
                        break;
                    case eNodeType.Fork:
                        Fork();
                        RefreshInterval();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                yield return null;
            }

            yield return null;
            
            _done = true;
            Complete?.Invoke();
            
            float seconds = sp.ElapsedMilliseconds / 1000.0f;
            Debug.Log($"[{nameof(OperationGroup)}] Complete load group in {seconds}s");
            
            Dispose();
        }
        
        private void Dispose()
        {
            Next = null;
            Complete = null;

            _nodes.Clear();
            _nodes = null;
            
            _tempOperations.Clear();
            _tempOperations = null;
            
            _canceler?.Dispose();
            _canceler = null;
        }
        
        private IEnumerator ProcessOperationNode()
        {
            LifecycleNode node = _nodes[_currentNodeIndex];
            
            IOperation operation = (IOperation)node.Parameters[0];
            float weight = (float) node.Parameters[1];

            return ProcessOperation(operation, weight);
        }
        
        private IEnumerator ProcessOperation(IOperation operation, float weight)
        {
            string operationName = GetOperationName(operation);
            
            System.Diagnostics.Stopwatch sp = new System.Diagnostics.Stopwatch();
            sp.Start();
            
            _tempOperations.Clear();
            
            if (operation is AsyncOperations asyncOperations)
            {
                _tempOperations.AddRange(asyncOperations.Operations);
                foreach (IOperation nestedOperation in asyncOperations.Operations)
                {
                    Next?.Invoke(nestedOperation);
                }
            }
            else
            {
                _tempOperations.Add(operation);
                Next?.Invoke(operation);
            }
            
            operation.Start();
            
            while (!operation.Done)
            {
                CalculateProgress();
                yield return null;
            }

            CalculateProgress();

            float seconds = sp.ElapsedMilliseconds / 1000.0f;
            Debug.Log($"[{nameof(OperationGroup)}] Operation {operationName} completed in {seconds}s");

            _currentIntervalSum += weight;
        }

        private string GetOperationName(IOperation operation)
        {
            string operationName = operation.GetType().Name;
            
            if (operation is AsyncOperations asyncOperations)
            {
                operationName = $"Async (";
                
                foreach (IOperation asyncOperation in asyncOperations.Operations)
                {
                    operationName += $"{GetOperationName(asyncOperation)}";
                    operationName += ", ";
                }
                
                operationName = operationName.Remove(operationName.Length - 2, 2);
                operationName += $")";
            }

            if (operation.GetType().IsGenericType)
            {
                operationName = operationName.Remove(operationName.Length - 2, 2);
                operationName += $"<";
                
                Type[] genericArguments = operation.GetType().GetGenericArguments();
                foreach (Type genericArgument in genericArguments)
                {
                    operationName += $"{genericArgument.Name}, ";
                }

                operationName = operationName.Remove(operationName.Length - 2, 2);
                operationName += ">";
            }

            return operationName;
        }
        
        private void CalculateProgress()
        {
            if (_totalIntervalSum <= 0.0f)
            {
                _progress = _currentInterval.x;
            }
            else
            {
                LifecycleNode node = _nodes[_currentNodeIndex];
                IOperation operation = (IOperation)node.Parameters[0];

                float operationSum = (float) node.Parameters[1] * operation.Progress;
                float currentSum = _currentIntervalSum + operationSum;
                float progress = currentSum / _totalIntervalSum;
            
                float resultProgress = Mathf.Lerp(_currentInterval.x, _currentInterval.y, progress);
                _progress = Mathf.Max(_progress, resultProgress);
            }
        }
        
        private void RefreshInterval()
        {
            float intervalSum = 0.0f;
            int left = NearInterval(_currentNodeIndex);
            int right = NearInterval(left + 1);

            for (int i = left; i < right; i++)
            {
                LifecycleNode node = _nodes[i];
                if (node.Type == eNodeType.Operation)
                {
                    intervalSum += (float)node.Parameters[1];
                }
            }
            
            _leftIntervalIndex = left;
            _rightIntervalIndex = right;
            _currentInterval = new Vector2((float)_nodes[_leftIntervalIndex].Parameters[0], (float)_nodes[_rightIntervalIndex].Parameters[0]);
            _currentIntervalSum = 0.0f;
            _totalIntervalSum = intervalSum;
            
            int NearInterval(int startIndex)
            {
                for (int i = startIndex; i < _nodes.Count; i++)
                {
                    LifecycleNode node = _nodes[i];
                    if (node.Type == eNodeType.Interval)
                    {
                        return i;
                    }
                }

                return _nodes.Count - 1;
            }
        }
        
        private void Fork()
        {
            LifecycleNode node = _nodes[_currentNodeIndex];
            Func<bool> condition = (Func<bool>)node.Parameters[0];
            OperationGroup nextGroup = condition() ? (OperationGroup) node.Parameters[1] : (OperationGroup) node.Parameters[2];

            if(nextGroup == null)
                return;
            
            bool normalize = (bool)node.Parameters[3];
            if (normalize)
            {
                nextGroup.AddBasicInterval();
                foreach (LifecycleNode nextGroupNode in nextGroup._nodes)
                {
                    if (nextGroupNode.Type == eNodeType.Interval)
                    {
                        nextGroupNode.Parameters[0] = Mathf.Lerp(_currentInterval.x, _currentInterval.y, (float)nextGroupNode.Parameters[0]);
                    }
                }
            }
            else
            {
                nextGroup._nodes.RemoveAll(x => x.Type == eNodeType.Interval);
            }

            _nodes.InsertRange(_currentNodeIndex + 1, nextGroup._nodes);
        }
        
        private class LifecycleNode
        {
            public eNodeType Type { get; }
            public List<object> Parameters { get; }
        
            public LifecycleNode(eNodeType type, params object[] parameters)
            {
                Type = type;
                Parameters = parameters.ToList();
            }
        }
        
        private enum eNodeType
        {
            Operation = 1,
            Interval = 2,
            Fork = 3
        }
    }
}