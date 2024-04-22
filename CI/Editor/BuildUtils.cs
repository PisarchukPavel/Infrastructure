using System;
using System.Collections.Generic;
using System.Linq;
using CI.Editor.Pipeline;
using CI.Editor.Target;
using UnityEditor;
using UnityEditor.Build;
using UnityObject = UnityEngine.Object;

namespace CI.Editor
{
    public static partial class BuildUtils
    {
        private const string ASSEMBLY_KEY = "buildAssembly";
        private const string PATH_KEY = "buildPath";
        private const string PLATFORM_KEY = "buildPlatform";
        private const string ENVIRNOMENT_KEY = "buildEnvironment";
        
        private static string[] CommandLineArguments => CommandLine.Collect();

        public static string GetAssembly()
        {
            return GetValue(ASSEMBLY_KEY);
        }
        
        public static string GetPath()
        {
            return GetValue(PATH_KEY);
        }
        
        public static ePlatformType GetPlatform()
        {
            string value = GetValue(PLATFORM_KEY);
            if (!Enum.TryParse(value, true, out ePlatformType result))
            {
                result = 0;
            }

            return result;
        }
        
        public static eEnvironmentType GetEnvironment()
        {
            string value = GetValue(ENVIRNOMENT_KEY);
            if (!Enum.TryParse(value, true, out eEnvironmentType result))
            {
                result = eEnvironmentType.DEV;
            }

            return result;
        }
        
        public static string GetValue(string key)
        {
            return GetValueImpl(key); 
        }
        
        public static bool GetValue(string key, out string value)
        {
            value = GetValueImpl(key);
            return !string.IsNullOrEmpty(value);
        }
        
        public static bool HasValue(string key)
        {
            return CommandLineArguments.Any(x => x == key);
        }
        
        private static string GetValueImpl(string key)
        {
            key = $"-{key}";
            for (int i = 0; i < CommandLineArguments.Length - 1; i++)
            {
                if (CommandLineArguments[i] == key)
                {
                    if (i == CommandLineArguments.Length - 1 || CommandLineArguments[i + 1].StartsWith("-"))
                        return null;
                    
                    return CommandLineArguments[i + 1];
                }
            }
            
            return null;
        }
        
        public static T FindAssembly<T>(string id) where T : CI_Assembly
        {
            T result = null;
            List<T> results = FindAssets<T>(x => x.Id == id);

            if (results.Count > 1)
            {
                string assemblyNames = string.Empty;
                foreach (T ciAssembly in results)
                {
                    assemblyNames += $"{ciAssembly.name}\n";
                }
                
                throw new BuildFailedException($"Matched {typeof(T)} count > 1: {results.Count}\n{assemblyNames}");
            }

            if (results.Count == 1)
            {
                result = results[0];
            }

            return result;
        }

        public static T FindAsset<T>(Predicate<T> predicate = null) where T : UnityObject
        {
            return FindAssets(predicate).FirstOrDefault();
        }
        
        public static List<T> FindAssets<T>(Predicate<T> predicate = null) where T : UnityObject
        {
            List<T> result = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).FullName}");
           
            /*List<T> assets = AssetDatabase
                .FindAssets($"t:{typeof(T).FullName}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .ToList();*/

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T tempResult = AssetDatabase.LoadAssetAtPath<T>(path);
                    
                if (tempResult != null && (predicate == null || predicate(tempResult)))
                {
                    result.Add(tempResult);
                }
            }

            return result;
        }
        
        public static string[] CollectScenes()
        {
            List<string> editorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) 
                    continue;
                
                editorScenes.Add(scene.path);
            }

            return editorScenes.ToArray();
        }

        public static void SaveAsset(UnityObject asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}