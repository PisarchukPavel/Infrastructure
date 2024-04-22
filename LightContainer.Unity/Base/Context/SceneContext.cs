using LightContainer.Base;
using UnityEngine;
using LightContainer.Unity.Base.Behaviour;

namespace LightContainer.Unity.Base.Context
{
    [DefaultExecutionOrder(int.MinValue)]
    public class SceneContext : BaseContext
    {
        [SerializeField] 
        private bool _autoLaunch = true;
  
        protected override void OnAwake()
        {
            IDependencyContainer self = this;
            IDependencyReader target = GlobalContext.Get(-1);

            self.Assign(target, eAssignPosition.After);
            
            if (_autoLaunch)
            {
                LaunchImpl();
            }
        }

        public new void Insert(IInstallerBehaviour installer)
        {
            base.Insert(installer);
        }

        public void Launch()
        {
            LaunchImpl();
        }

        private void LaunchImpl()
        {
            base.Launch(ContextUtils.FindInjectionTargets(gameObject.scene.GetRootGameObjects()));
        }
    }
}