using CI.Editor.Target;
using UnityEngine;

namespace CI.Editor.Pipeline.Actions
{
    [CreateAssetMenu(order = 0, fileName = "Switch Platform", menuName = "CI/Action/Common/Switch Platform")]
    public class CI_SwitchPlatform : CI_Action
    {
        protected override bool Run()
        {
            PlatformHelper.SwitchPlatform(Context.PlatformType);
            return true;
        }
    }
}