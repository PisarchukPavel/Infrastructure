using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CI.Editor.Target;
using UnityEditor;
using UnityEngine;

namespace CI.Editor.Pipeline
{
    [Serializable]
    public class BuildContext
    {
        public bool ExternalCall { get; }
        public string AssemblyId { get; }
        public string BuildPath { get; }
        public ePlatformType PlatformType { get; }
        public eEnvironmentType EnvironmentType { get; }
        public BuildPlayerOptions BuildOptions { get; private set; }
        public RuntimeParameters RuntimeParameters { get; private set; }
        
        public BuildContext(bool externalCall, string assemblyId, string buildPath, ePlatformType platformType, eEnvironmentType environmentType)
        {
            ExternalCall = externalCall;
            AssemblyId = assemblyId;
            BuildPath = buildPath;
            PlatformType = platformType;
            EnvironmentType = environmentType;
            RuntimeParameters = new RuntimeParameters();
            
            BuildOptions = new BuildPlayerOptions()
            {
                scenes = BuildUtils.CollectScenes(),
                locationPathName = buildPath,
                target = PlatformHelper.ConvertToBuildTarget(platformType),
                //targetGroup = PlatformHelper.ConvertToBuildTargetGroup(buildTargetContext.PlatformType),
                options = UnityEditor.BuildOptions.None 
            };
        }

        public void ChangeScenes(IEnumerable<string> scenes)
        {
            Debug.Log($"[{nameof(BuildContext)}] Change linked scene from ({GetScenesStr(BuildOptions.scenes)}) to ({GetScenesStr(scenes)})");
           
            BuildOptions = new BuildPlayerOptions()
            {
                scenes = scenes.ToArray(),
                locationPathName = BuildOptions.locationPathName,
                target = BuildOptions.target,
                options = BuildOptions.options
            };

            string GetScenesStr(IEnumerable<string> list)
            {
                //StringBuilder sb = new StringBuilder();
                string result = string.Empty;
                foreach (string s in list)
                {
                    result += $"\"{s}\" ";
                }
                return result;
            }
        }
        
        public void ChangePath(string path)
        {
            Debug.Log($"[{nameof(BuildContext)}] Change build path from \"{BuildOptions.locationPathName}\" to \"{path}\"");

            BuildOptions = new BuildPlayerOptions()
            {
                scenes = BuildOptions.scenes,
                locationPathName = path,
                target = BuildOptions.target,
                options = BuildOptions.options
            };
        }
        
        public void ChangeOptions(BuildOptions options)
        {
            Debug.Log($"[{nameof(BuildContext)}] Change build options");

            BuildOptions = new BuildPlayerOptions()
            {
                scenes = BuildOptions.scenes,
                locationPathName = BuildOptions.locationPathName,
                target = BuildOptions.target,
                options = options
            };
        }
    }
}