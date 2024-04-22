using System.Collections.Generic;
using LightContainer.Reflection;
using LightContainer.Unity.Base.Behaviour;
using UnityEngine;

namespace LightContainer.Unity.Base.Context
{
    public static class ContextUtils
    {
        public static List<MonoBehaviour> FindInjectionTargets(GameObject root)
        {
            return FindInjectionTargets(new[] { root });
        }
        
        public static List<MonoBehaviour> FindInjectionTargets(GameObject[] roots)
        {
            List<MonoBehaviour> result = new List<MonoBehaviour>();
            
            foreach (GameObject root in roots)
            {
                MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
                result.Capacity += behaviours.Length;
                
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if(behaviour == null)
                        continue;
                    
                    if(!TypeStorage.Available(behaviour.GetType()))
                        continue;
                    
                    if(behaviour.GetComponentInParent<InjectBehaviour>(true) != null)
                        continue;

                    BaseContext baseContext = behaviour.GetComponentInParent<BaseContext>(true);
                    if(baseContext != null && baseContext.gameObject != root)
                        continue;
                    
                    result.Add(behaviour);
                }
            }

            return result;
        }
    }
}