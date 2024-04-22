using UnityEditor;
using UnityEngine;

namespace CI.Editor.Pipeline.Actions
{
    [CreateAssetMenu(order = 0, fileName = "Splash Settings", menuName = "CI/Action/Common/Splash Settings")]
    public class CI_LaunchSettings : CI_Action
    {
        [SerializeField] 
        private bool _showSplashScreen = true;
        
        [SerializeField]
        private bool _showUnityLogo = false;
        
        protected override bool Run()
        {
            PlayerSettings.SplashScreen.show = _showSplashScreen;
            PlayerSettings.SplashScreen.showUnityLogo = _showUnityLogo;
            return true;
        }
    }
}