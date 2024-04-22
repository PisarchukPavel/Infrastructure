using System;
using System.Collections.Generic;

namespace Bootstrap.Feature
{
    public class FeatureMediator : IFeatureMediator
    {
        IReadOnlyList<IFeature> IFeatureMediator.Features => _features;
        
        private readonly List<IFeature> _features = new List<IFeature>();

        IDisposable IFeatureMediator.Attach(IFeature feature)
        {
            _features.Add(feature);
            return new Disposer(() => _features.Remove(feature));
        }
        
        private class Disposer : IDisposable
        {
            private Action _disposeAction = null;
            
            public Disposer(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }
            
            void IDisposable.Dispose()
            {
                _disposeAction?.Invoke();
                _disposeAction = null;
            }
        }
    }
}