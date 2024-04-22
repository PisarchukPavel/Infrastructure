using System;
using System.Collections.Generic;

namespace Bootstrap.Feature
{
    public interface IFeatureMediator
    {
        IReadOnlyList<IFeature> Features { get; }
        
        IDisposable Attach(IFeature service);
    }
}