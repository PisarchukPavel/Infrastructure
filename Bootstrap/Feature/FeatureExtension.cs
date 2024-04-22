using System;
using System.Collections.Generic;
using System.Linq;
using Bootstrap.Base;

namespace Bootstrap.Feature
{
    public static class FeatureExtension
    {
        public static bool Enabled<T>(this IFeatureMediator mediator, Predicate<IFeature> predicate = null) where T : class, IFeature
        {
            T feature = mediator.Get<T>(predicate);
            return feature != null && feature.Enabled;
        }
        
        public static bool Available<T>(this IFeatureMediator mediator, Func<T, bool> condition, Predicate<IFeature> predicate = null) where T : class, IFeature
        {
            T feature = mediator.Get<T>(predicate);
            return feature != null && feature.Enabled && (condition == null || condition.Invoke(feature));
        }
        
        public static T Get<T>(this IFeatureMediator mediator, Predicate<IFeature> predicate = null) where T : class, IFeature
        {
            mediator.Get<T>(out T result, predicate);
            
            if(result == null)
                throw new NullReferenceException($"{typeof(T)} is not represent in {mediator.GetType().Name}");

            return result;
        }
        
        public static bool Get<T>(this IFeatureMediator mediator, out T result, Predicate<IFeature> predicate = null) where T : class, IFeature
        {
            result = mediator.Features.FirstOrDefault(feature => feature is T featureGeneric && (predicate == null || predicate(featureGeneric))) as T;
            return result != null;
        }

        public static List<IOperation> Launchers<T>(this IFeatureMediator mediator, int handle, T data)
        {
            List<IFeatureLauncher<T>> launchers = new List<IFeatureLauncher<T>>();
            foreach (IFeature feature in mediator.Features)
            {
                if (feature is IFeatureLauncher<T> featureStarter)
                {
                    launchers.Add(featureStarter);
                }
            }
            
            launchers.Sort((left, right) => left.Order.CompareTo(right.Order));
            
            List<IOperation> operations = new List<IOperation>(launchers.Count);
            foreach (IFeatureLauncher<T> launcher in launchers)
            {
                IOperation operation = launcher.Launch(handle, data);
                if(operation == null)
                    continue;
                
                operations.Add(operation);
            }
            
            return operations;
        }
    }
}