using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace CI.Editor.Target
{
    public static class PlatformHelper
    {
        public static void SwitchPlatform(ePlatformType platformType)
        {
            BuildTarget newBuildTarget = ConvertToBuildTarget(platformType);
            BuildTargetGroup newTargetGroup = ConvertToBuildTargetGroup(platformType);
            EditorUserBuildSettings.SwitchActiveBuildTarget(newTargetGroup, newBuildTarget);

            Debug.Log($"[{nameof(PlatformHelper)}] Switch to {platformType} platform");

            SaveChanges(); 
        }
        
        public static void ReplaceDefines(IEnumerable<string> customDefines)
        {
            List<ePlatformType> platforms = GetPlatforms();
            List<string> defines = customDefines.ToList();
            
            ClearDefines(platforms);
            InsertDefines(defines, platforms);
            SaveChanges();
        }

        public static void Refresh()
        {
            SaveChanges();
        }

        private static List<ePlatformType> GetPlatforms()
        {
            return ((ePlatformType[]) Enum.GetValues(typeof(ePlatformType))).ToList();
        }
        
        private static void ClearDefines(List<ePlatformType> platforms)
        {
            foreach (ePlatformType platformType in platforms)
            {
                BuildTargetGroup buildTargetGroup = ConvertToBuildTargetGroup(platformType);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Empty);
            }
        }
        
        private static void InsertDefines(List<string> customDefines, List<ePlatformType> platforms)
        {
            if(customDefines.Count == 0)
                return;
            
            foreach (ePlatformType platformType in platforms)
            {
                BuildTargetGroup buildTargetGroup = ConvertToBuildTargetGroup(platformType);

                string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                List<string> allDefines = definesString.Split(';' ).ToList();
                
                foreach (string customDefine in customDefines)
                {
                    allDefines.Add(customDefine);
                }

                string newDefines = string.Join(";", allDefines.ToArray());
                if (newDefines.StartsWith(";"))
                {
                    newDefines = newDefines.Remove(0, 1);
                }
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
                
                Debug.Log($"[{nameof(PlatformHelper)}] Set \"{newDefines}\" defines for {platformType} platform.");
            }
        }
    
        public static BuildTarget ConvertToBuildTarget(ePlatformType platformType)
        {
            switch (platformType)
            {
                case ePlatformType.Android:
                    return BuildTarget.Android;
                case ePlatformType.iOS:
                    return BuildTarget.iOS;
                case ePlatformType.UWP:
                    return BuildTarget.WSAPlayer;
                case ePlatformType.WebGL:
                    return BuildTarget.WebGL;       
                case ePlatformType.Windows:
                    return BuildTarget.StandaloneWindows64;
                case ePlatformType.MacOS:
                    return BuildTarget.StandaloneOSX;
                case ePlatformType.Linux:
                    return BuildTarget.StandaloneLinux64;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platformType), platformType, null);
            }   
        }
        
        public static BuildTargetGroup ConvertToBuildTargetGroup(ePlatformType platformType)
        {
            switch (platformType)
            {
                case ePlatformType.Android:
                    return BuildTargetGroup.Android;
                case ePlatformType.iOS:
                    return BuildTargetGroup.iOS;
                case ePlatformType.UWP:
                    return BuildTargetGroup.WSA;
                case ePlatformType.WebGL:
                    return BuildTargetGroup.WebGL;    
                case ePlatformType.Windows:
                    return BuildTargetGroup.Standalone;
                case ePlatformType.MacOS:
                    return BuildTargetGroup.Standalone;
                case ePlatformType.Linux:
                    return BuildTargetGroup.Standalone;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platformType), platformType, null);
            }   
        }
        
        private static void SaveChanges()
        {
            UnityObject projectSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0];
            EditorUtility.SetDirty(projectSettings);
            AssetDatabase.SaveAssetIfDirty(projectSettings);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.RefreshSettings();
            AssetDatabase.Refresh();
            
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);

            AssetDatabase.SaveAssets();
            AssetDatabase.RefreshSettings();
            AssetDatabase.Refresh();
        }
    }
}