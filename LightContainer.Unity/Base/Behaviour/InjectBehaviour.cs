using System;
using System.Linq;
using UnityEngine;
using LightContainer.Base;
using LightContainer.Base.Binding;
using LightContainer.Reflection;
using LightContainer.Unity.Base.Context;

namespace LightContainer.Unity.Base.Behaviour
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(int.MinValue + 1)]
    public class InjectBehaviour : MonoBehaviour
    {
        [SerializeField]
        private eInjectTarget _target = eInjectTarget.Parent;
        
        [SerializeField]
        private eInjectContext _context = eInjectContext.Scene;
        
        [SerializeField]
        private ParametersBehaviour _parameters = null;

        private bool _skip = false;
        
        private void Awake()
        {
            if(_skip)
                return;
            
            Inject();
        }

        private void Inject()
        {
            IDependencyInjector injector = GetInjector();
            MonoBehaviour[] targets = GetTargets();

            ParametersContext parameters = (_parameters == null) ? null : _parameters.Additional();
            foreach (MonoBehaviour behaviour in targets)
            {
                if(behaviour == null)
                    continue;
                
                if(!TypeStorage.Available(behaviour.GetType()))
                    continue;
                
                if (behaviour is InjectBehaviour injectBehaviour && behaviour != this)
                {
                    injectBehaviour._skip = true;
                    Debug.Log($"[{nameof(InjectBehaviour)}] Find nested {nameof(InjectBehaviour)} in {name}, use InjectTarget as Parent", gameObject);
                    continue;
                }
                
                injector.Inject(behaviour, parameters);
            }
        }

        private MonoBehaviour[] GetTargets()
        {
            switch (_target)
            {
                case eInjectTarget.All:
                    return gameObject.GetComponentsInChildren<MonoBehaviour>(true);
                case eInjectTarget.Parent:
                    return gameObject.GetComponents<MonoBehaviour>();
                case eInjectTarget.Child:
                    MonoBehaviour[] parents = gameObject.GetComponents<MonoBehaviour>();
                    return gameObject.GetComponentsInChildren<MonoBehaviour>(true).Where(x => !parents.Contains(x)).ToArray();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private IDependencyReader GetInjector()
        {
            int handle = gameObject.scene.handle;

            switch (_context)
            {
                case eInjectContext.Auto:
                    ObjectContext objectContext = GetComponentInParent<ObjectContext>(true);
                    if (objectContext != null)
                        return GlobalContext.Get(objectContext.gameObject.GetInstanceID());
                    IDependencyReader sceneContainer = GlobalContext.Get(handle);
                    return sceneContainer ?? GlobalContext.Get(-1);
                
                case eInjectContext.Global:
                    return GlobalContext.Get(-1);
             
                case eInjectContext.Scene:
                    IDependencyReader sceneReader = GlobalContext.Get(handle);
                    if(sceneReader == null)
                        throw new NullReferenceException($"Injection failed because scene context not found");
                    return sceneReader;
                
                case eInjectContext.Object:
                    GameObject objectContextGameObject = GetComponentInParent<ObjectContext>(true)?.gameObject;
                    if(objectContextGameObject == null)
                        throw new NullReferenceException($"Injection failed because object context not found");
                    IDependencyReader objectReader = GlobalContext.Get(objectContextGameObject.GetInstanceID());
                    return objectReader;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private enum eInjectTarget
        {
            All = 0,
            Parent = 1,
            Child = 2
        }
        
        private enum eInjectContext
        {
            Auto = 0,
            Global = 1,
            Scene = 2,
            Object = 3
        }
    }
}