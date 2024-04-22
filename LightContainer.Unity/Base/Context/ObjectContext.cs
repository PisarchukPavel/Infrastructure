using System;
using System.Collections.Generic;
using LightContainer.Base;
using UnityEngine;

namespace LightContainer.Unity.Base.Context
{
    [DefaultExecutionOrder(int.MinValue)]
    public class ObjectContext : BaseContext
    {
        [SerializeField] 
        private eAssignTarget _assignTarget = eAssignTarget.Nearest;

        protected override void OnAwake()
        {
            IDependencyContainer self = this;
            IDependencyReader target = null;
            
            switch (_assignTarget)
            {
                case eAssignTarget.Nearest:
                    BaseContext baseContext = NearestContext();
                    if (baseContext == null)
                    {
                        target = GlobalContext.Get(-1);
                    }
                    else
                    {
                        if (baseContext is SceneContext)
                        {
                            target = GlobalContext.Get(gameObject.scene.handle);
                        }
                        else
                        {
                            target = GlobalContext.Get(baseContext.gameObject.GetInstanceID());
                        }
                    }
                    break;
                
                case eAssignTarget.Object:
                    BaseContext objectContext = NearestContext() as ObjectContext;
                    target = (objectContext != null) ? GlobalContext.Get(objectContext.gameObject.GetInstanceID()) : null;
                    break;
                
                case eAssignTarget.Scene:
                    target = GlobalContext.Get(gameObject.scene.handle);
                    break;
                
                case eAssignTarget.Global:
                    target = GlobalContext.Get(-1);
                    break;
                
                case eAssignTarget.None:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_assignTarget != eAssignTarget.None)
            {
                if (target == null)
                    throw new NullReferenceException($"Failed to assign {nameof(ObjectContext)} with {_assignTarget} context");

                self.Assign(target, eAssignPosition.After);
            }

            if (target is SceneContext sceneContext)
            {
                sceneContext.Ready += Launch;
            }
            else
            {
                Launch();
            }
        }

        private BaseContext NearestContext()
        {
            return GetComponentInParent<BaseContext>();
        }
        
        private void Launch()
        {
            List<MonoBehaviour> targets = ContextUtils.FindInjectionTargets(gameObject);
            base.Launch(targets);
        }
        
        private enum eAssignTarget
        {
            Nearest,
            Object,
            Scene,
            Global,
            None
        }
    }
}