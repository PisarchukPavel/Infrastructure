//#define PROGRAMMING
#define PLAY_SERVICES_RESOLVER
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_IOS || PROGRAMMING
using UnityEditor.iOS.Xcode;
#endif

namespace CI.Editor.Pipeline.Actions.iOS
{
    [CreateAssetMenu(order = 0, fileName = "iOS Settings", menuName = "CI/Action/iOS/Settings")]
    public class CI_iOSSettings : CI_Action
    {
        [SerializeField]
        private string _teamId = string.Empty;
        
        [SerializeField]
        private string _provisionId = string.Empty;
        
        [SerializeField]
        private string _provisionName = string.Empty;

        [SerializeField] 
        private string _certificateName = string.Empty;
        
        protected override bool Run()
        {
#if UNITY_IOS || PROGRAMMING
            PlayerSettings.bundleVersion = BuildUtils.GetShortVersion();
            PlayerSettings.applicationIdentifier = PlayerSettings.applicationIdentifier.ToLower();
            PlayerSettings.iOS.buildNumber = BuildUtils.GetBuildNumber();
            PlayerSettings.iOS.appleDeveloperTeamID = _teamId;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.iOSManualProvisioningProfileID = _provisionId;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;

            bool externalDistribution = BuildUtils.iOS.GetDistribution() == "Ad-Hoc";
            if (externalDistribution)
            {
                PlayerSettings.iOS.appleEnableAutomaticSigning = true;
                GenerateAdHocExportFile();
            }
            else
            {
                PlayerSettings.iOS.appleEnableAutomaticSigning = false;
                GenerateAppStoreExportFile();
            }
#endif
            return true;
        }

#if UNITY_IOS || PROGRAMMING
        private void GenerateAdHocExportFile()
        {
            PlistDocument plist = new PlistDocument();
            plist.Create();
            
            plist.root.SetBoolean("compileBitcode", false);
            plist.root.SetString("method", "ad-hoc");
            plist.root.SetString("destination", "export");
            plist.root.SetBoolean("stripSwiftSymbols", true);
            plist.root.SetString("teamID", _teamId);
            plist.root.SetString("signingStyle", "automatic");
            //plist.root.SetString("signingCertificate", _signingCertificate);
            //plist.root.CreateDict("provisioningProfiles").SetString(applicationIdentifier, _provisionName);
            plist.root.SetString("thinning", "<none>");
            
            File.WriteAllText(GenerateExportOptionsPath(), plist.WriteToString());
        }

        private void GenerateAppStoreExportFile()
        {
            PlistDocument plist = new PlistDocument();
            plist.Create();
            
            string applicationIdentifier = PlayerSettings.applicationIdentifier.ToLower();
            
            plist.root.SetBoolean("compileBitcode", false);
            plist.root.SetString("method", "app-store-connect");
            plist.root.SetString("destination", "export");
            plist.root.SetBoolean("stripSwiftSymbols", true);
            plist.root.SetString("teamID", _teamId);
            plist.root.SetString("signingStyle", "manual");
            plist.root.SetString("signingCertificate", _certificateName);
            plist.root.CreateDict("provisioningProfiles").SetString(applicationIdentifier, _provisionName);
            plist.root.SetString("thinning", "<none>");
            
            File.WriteAllText(GenerateExportOptionsPath(), plist.WriteToString());
        }
        
        private string GenerateExportOptionsPath()
        {
            string selfPath = AssetDatabase.GetAssetPath(this);
            string selfName = Path.GetFileName(selfPath);
            selfPath = selfPath.Replace(selfName, "ExportOptions.plist");

            return selfPath;
        }
#endif
    }
}