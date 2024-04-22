using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CI.Editor.Target;
using UnityEditor;

namespace CI.Editor
{
    public static partial class BuildUtils
    {
        private const string VERSION_KEY = "buildVersion";
        private const string NUMBER_KEY = "buildNumber";
        private const string STAGE_KEY = "buildStage";
        private const string DEFINES_KEY = "buildDefines";
        private const string BRANCH_KEY = "buildBranch";
        
        private const string DISTRIBUTION_KEY = "buildDistribution";
        private const string KEYSTORE_KEY = "buildKeystore";

        [MenuItem("Window/CI/Branch")]
        private static void ApplyBranch()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "git",
                Arguments = "rev-parse --abbrev-ref HEAD",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo);
            using StreamReader reader = process?.StandardOutput;
            string result = reader?.ReadToEnd();
            string branch = result?.Trim();
            
            if (string.IsNullOrEmpty(branch))
                throw new NullReferenceException($"Can't parse current branch");

            string formatBranch = FormatBranch(branch);
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
           
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            List<string> allDefines = definesString.Split(';' ).Where(x => !x.StartsWith("BRANCH_")).ToList();
            allDefines.Add($"BRANCH_{formatBranch}");
            
            PlatformHelper.ReplaceDefines(allDefines);
        }
        
        public static string GetFullVersion()
        {
            return $"{GetVersion()}.{GetBuildNumber()}";
        }
        
        public static string GetShortVersion()
        {
            return GetVersion();
        }

        private static string GetVersion()
        {
            string variant = GetValue(VERSION_KEY);
            if (variant == null)
                throw new NullReferenceException($"Missing {VERSION_KEY} command line argument");
            
            return variant.Split("-")[0].Replace(" ", string.Empty);
        }
        
        public static string GetBuildNumber()
        {
            string buildNumber = GetValue(NUMBER_KEY);
            if (buildNumber == null)
                throw new NullReferenceException($"Missing {NUMBER_KEY} command line argument");
            
            return buildNumber;
        }
        
        public static string GetBranch()
        {
            string branchName = GetValue(BRANCH_KEY) ?? string.Empty;
            return FormatBranch(branchName);
        }
        
        private static string FormatBranch(string branchName)
        {
            branchName = branchName
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("-", "_")
                .Replace("/", "_")
                .Replace(" ", "_");

            return branchName; 
        }

        public static string GetStage()
        {
            return GetValue(STAGE_KEY);
        }

        public static string GetDefines()
        {
            return GetValue(DEFINES_KEY);
        }

        public static class Android
        {
            public static string GetDistribution()
            {
                return GetValue(DISTRIBUTION_KEY);
            }
            
            public static string GetKeystore()
            {
                return GetValue(KEYSTORE_KEY);
            }
        }
        
        public static class iOS
        {
            public static string GetDistribution()
            {
                return GetValue(DISTRIBUTION_KEY);
            }
        }
    }
}