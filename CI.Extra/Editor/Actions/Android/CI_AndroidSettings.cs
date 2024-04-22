//#define PLAY_SERVICES_RESOLVER
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor.Android;

namespace CI.Editor.Pipeline.Actions.Android
{
    [CreateAssetMenu(order = 0, fileName = "Android Settings", menuName = "CI/Action/Android/Settings")]
    public class CI_AndroidSettings : CI_Action
    {
        [SerializeField] 
        private KeyStoreInfo _keyStoreInfo = null;
        
        protected override bool Run()
        {
            PrepareUnitySettings();
            PrepareEnvironmentSettings();
            PrepareDependencyResolve();
            PrepareGradleProperties();
            
            return true;
        }

        private void PrepareUnitySettings()
        {
            Debug.Log($"[{nameof(CI_AndroidSettings)}] Keystore path is {BuildUtils.Android.GetKeystore()}");

            PlayerSettings.bundleVersion = BuildUtils.GetShortVersion();
            PlayerSettings.Android.bundleVersionCode = int.Parse(BuildUtils.GetBuildNumber());
            
            if (BuildUtils.Android.GetDistribution() == "Play-Market")
            {
                EditorUserBuildSettings.buildAppBundle = true;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = _keyStoreInfo.KeyStorePath;
                PlayerSettings.Android.keystorePass = _keyStoreInfo.KeyStorePass;
                PlayerSettings.Android.keyaliasName = _keyStoreInfo.KeyStoreAliasName;
                PlayerSettings.Android.keyaliasPass = _keyStoreInfo.KeyStoreAliasPass;
                
                Context.ChangePath($"{Context.BuildOptions.locationPathName}/{GetFileName()}.aab");
            }
            else
            {
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

                PlayerSettings.Android.useCustomKeystore = false;
                PlayerSettings.Android.keystoreName = string.Empty;
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasName = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                                
                Context.ChangePath($"{Context.BuildOptions.locationPathName}/{GetFileName()}.apk");
            }
        }

        private void PrepareEnvironmentSettings()
        { 
            string jdkPath = AndroidExternalToolsSettings.jdkRootPath;
            Environment.SetEnvironmentVariable("JAVA_HOME", jdkPath);
            
            if (Context.ExternalCall)
            {
                string gradleCachePath = $"{Application.temporaryCachePath}"; //"C:\\gradle-cache";
                if (!Directory.Exists(gradleCachePath))
                {
                    Directory.CreateDirectory(gradleCachePath);
                }
                
                Environment.SetEnvironmentVariable("GRADLE_USER_HOME", gradleCachePath);
            }            
        }
        
        private void PrepareDependencyResolve()
        {
            string configPath = Path.Combine("ProjectSettings", "AndroidResolverDependencies.xml");
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
            
#if PLAY_SERVICES_RESOLVER
            GooglePlayServices.PlayServicesResolver.DeleteResolvedLibrariesSync();
            GooglePlayServices.PlayServicesResolver.ResolveSync(false);
#endif
        }

        private void PrepareGradleProperties()
        {
            string gradlePath = AndroidExternalToolsSettings.gradlePath;
            if (!gradlePath.EndsWith(".properties"))
            {
                gradlePath += ".properties";
            }
            
            StreamWriter writer = File.AppendText(gradlePath);
            writer.WriteLine("");
            writer.WriteLine("android.useAndroidX=true");
            writer.WriteLine("android.enableJetifier=true");
            writer.Flush();
            writer.Close();
        }
        
        private string GetFileName()
        {
            string product = PlayerSettings.productName.Replace(" ", string.Empty);
            string number = BuildUtils.GetBuildNumber();
            string branch = BuildUtils.GetBranch();
            string environment = $"{BuildUtils.GetEnvironment()}";
            string stage = BuildUtils.GetStage();

            return $"{product}_{number}_{branch}_{environment}_{stage}";
        }
        
        [Serializable]
        private class KeyStoreInfo
        {
            public string KeyStorePath => _keyStoreFile == null ? BuildUtils.Android.GetKeystore() : AssetDatabase.GetAssetPath(_keyStoreFile);
            public string KeyStorePass => _keyStorePass;
            public string KeyStoreAliasName => _keyStoreAliasName;
            public string KeyStoreAliasPass => _keyStoreAliasPass;

            [SerializeField] 
            private DefaultAsset _keyStoreFile = null;

            [SerializeField] 
            private string _keyStorePass = string.Empty;
       
            [SerializeField] 
            private string _keyStoreAliasName = string.Empty;
       
            [SerializeField] 
            private string _keyStoreAliasPass = string.Empty;
        }
    }
}