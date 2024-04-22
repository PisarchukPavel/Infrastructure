//#define PROGRAMMING
//#define PLAY_SERVICES_RESOLVER
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_IOS || PROGRAMMING
using UnityEditor.iOS.Xcode;
#endif

namespace CI.Editor.Pipeline.Actions.iOS
{
    // Depended from CI_iOSSettings (ExportOptions file generation)
    [CreateAssetMenu(order = 0, fileName = "Pbx Settings", menuName = "CI/Action/iOS/PBX")]
    public class CI_PbxSettings : CI_Action
    {
        [SerializeField] 
        private List<string> _frameworks = new List<string>()
        {
            "Security.framework",
            "SystemConfiguration.framework",
            "AdSupport.framework",
            "WebKit.framework",
            "StoreKit.framework",
            "libsqlite3.0.tbd",
            "libz.tbd"
        };
        
        [SerializeField] 
        private List<KeyValue> _descriptions = new List<KeyValue>()
        {
            new ("NSPrivacyAccessedAPICategorySystemBootTime", "35F9.1"),
            new ("NSMicrophoneUsageDescription", "Access to the microphone is required to enable voice chat functionality within the game."),
            new ("NSBluetoothPeripheralUsageDescription", "Bluetooth access is needed to connect and use Bluetooth microphones for voice chat."),
            new ("NSUserTrackingUsageDescription", "Our application uses your location and activity data for analytics and to improve your experience." +
                                                   " We do not share these data with third parties. Please allow us to track this data to enhance our service.")
        };
        
        protected override bool Run()
        {
#if PLAY_SERVICES_RESOLVER && (UNITY_IOS || PROGRAMMING)
            Google.IOSResolver.InstallCocoapods(false, Context.BuildPath, false);
#endif
            
#if UNITY_IOS || PROGRAMMING
            string pbxPath = $"{Context.BuildPath}/Unity-iPhone.xcworkspace/project.pbxproj".Replace("\\", "/");
            if (!File.Exists(pbxPath))
            {
                pbxPath = pbxPath.Replace("Unity-iPhone.xcworkspace", "Unity-iPhone.xcodeproj");
                Debug.Log($"[{nameof(CI_PbxSettings)}] Pbx path in {Context.BuildPath} not exits, use .xcodeproj");
            }
            else
            {
                Debug.Log($"[{nameof(CI_PbxSettings)}] Pbx path in {Context.BuildPath} exits, use .xcworkspace");
            }
            
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxPath);

            PrepareProject();
            ModifyInfo(Context.BuildPath);
            ModifySettings(pbxPath, pbxProject);
            ModifyCapability(pbxPath, pbxProject);
#endif
            
            return true;
        }

#if UNITY_IOS || PROGRAMMING
        private void PrepareProject()
        {
            string targetPath = $"{Context.BuildPath}/ExportOptions.plist";
            File.Copy(GenerateExportOptionsPath(), targetPath);
        }
        
        private void ModifyInfo(string buildPath)
        {
            string plistPath = buildPath + "/Info.plist";
            PlistDocument plist = ReadPlist(plistPath);
            
            plist.root.CreateArray("UIBackgroundModes").AddString("location");
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            foreach (KeyValue description in _descriptions)
            {
                plist.root.SetString(description.Key, description.Value);
            }
            
            plist.root.SetString("CFBundleVersion", BuildUtils.GetFullVersion());
            plist.root.SetString("CFBundleShortVersionString", BuildUtils.GetShortVersion());
            
            File.WriteAllText(plistPath, plist.WriteToString());
        }

        private void ModifySettings(string pbxPath, PBXProject pbxProject)
        {
            string unityTargetGuid = pbxProject.GetUnityMainTargetGuid();
            string testTargetGuid = pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName()); //pbxProject.GetUnityMainTestTargetGuid();
            string frameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();

            AddBuildProperty("OTHER_LDFLAGS", "-ObjC");
            SetBuildProperty("ENABLE_BITCODE", "NO");
            SetBuildProperty("ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

            if (ReadPlist(GenerateExportOptionsPath()).root.values.TryGetValue("signingCertificate", out PlistElement certificateElement))
            {
                string certificate = certificateElement.AsString();
                SetBuildProperty("CODE_SIGN_IDENTITY", certificate);
                SetBuildProperty("CODE_SIGN_IDENTITY[sdk=iphoneos*]", certificate);
            }

            foreach (string framework in _frameworks)
            {
                AddFrameworkToProject(framework);
            }
            
            pbxProject.WriteToFile(pbxPath);
            
            // Local methods
            void AddBuildProperty(string key, string value)
            {
                pbxProject.AddBuildProperty(unityTargetGuid, key, value);
                pbxProject.AddBuildProperty(testTargetGuid, key, value);
                pbxProject.AddBuildProperty(frameworkTargetGuid, key, value);
            }
            
            void SetBuildProperty(string key, string value)
            {
                pbxProject.SetBuildProperty(unityTargetGuid, key, value);
                pbxProject.SetBuildProperty(testTargetGuid, key, value);
                pbxProject.SetBuildProperty(frameworkTargetGuid, key, value);
            }
            
            void AddFrameworkToProject(string framework)
            {
                pbxProject.AddFrameworkToProject(unityTargetGuid, framework, false);
                pbxProject.AddFrameworkToProject(frameworkTargetGuid, framework, false);
            }
            
            void AddFileToBuild(string lib)
            {
                string path = $"usr/lib/{lib}";
                string projectPath = $"Frameworks/{lib}";

                List<PBXSourceTree> targets = new List<PBXSourceTree>()
                {
                    PBXSourceTree.Absolute, PBXSourceTree.Source, PBXSourceTree.Build, PBXSourceTree.Developer, PBXSourceTree.Sdk    
                };
                
                foreach (PBXSourceTree source in targets)
                {
                    pbxProject.AddFileToBuild(unityTargetGuid, pbxProject.AddFile(path, projectPath, source));
                }
            }
        }
        
        private void ModifyCapability(string pbxPath, PBXProject pbxProject)
        {
            string[] idArray = Application.identifier.Split('.');
            string entitlementsPath = $"Unity-iPhone/{idArray[^1]}.entitlements";        
            
            ProjectCapabilityManager projCapability = new ProjectCapabilityManager(pbxPath, entitlementsPath, null, pbxProject.GetUnityMainTargetGuid());
            //projCapability.AddBackgroundModes((BackgroundModesOptions) int.MaxValue);
            projCapability.AddPushNotifications(false);
            projCapability.AddInAppPurchase();
            //projCapability.AddSignInWithApple();
            
            projCapability.WriteToFile();
        }
        
        private PlistDocument ReadPlist(string path)
        {
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(path);

            return plist;
        }
        
        private string GenerateExportOptionsPath()
        {
            string selfPath = AssetDatabase.GetAssetPath(this);
            string selfName = Path.GetFileName(selfPath);
            selfPath = selfPath.Replace(selfName, "ExportOptions.plist");

            return selfPath;
        }
#endif
        
        [Serializable]
        private class KeyValue
        {
            public string Key => _key;
            public string Value => _value;
            
            [SerializeField] 
            private string _key = string.Empty;
            
            [SerializeField] 
            private string _value = string.Empty;

            public KeyValue(string key, string value)
            {
                _key = key;
                _value = value;
            }
        }
    }
}