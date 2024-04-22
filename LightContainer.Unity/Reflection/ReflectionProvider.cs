using System.IO;
using LightContainer.Reflection;
using LightContainer.Reflection.Direct;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace LightContainer.Unity.Reflection
{
    public static class ReflectionProvider
    {
        private const string ROOT_DIRECTORY = "Assets/LightContainer";
        private const string GENERATOR_DIRECTORY = ROOT_DIRECTORY + "/Generator";
        private const string REFLECTION_DIRECTORY = ROOT_DIRECTORY + "/Resources";
        private const string REFLECTION_NAME = "Reflection";
        
        public static void Save()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            
            if (!Directory.Exists(REFLECTION_DIRECTORY))
            {
                Directory.CreateDirectory(REFLECTION_DIRECTORY);
            }

            byte[] data = TypeStorage.Collect();
            File.WriteAllBytes($"{REFLECTION_DIRECTORY}/{REFLECTION_NAME}.txt", data);
    
            sp.Stop();
            Debug.Log($"[{nameof(ReflectionProvider)}] Serialize reflection at {sp.ElapsedMilliseconds / 1000.0f}s");
        }
        
        public static void Load()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();

            TextAsset reflection = Resources.Load(REFLECTION_NAME) as TextAsset;
            TypeStorage.Upload(reflection == null ? null : reflection.bytes);
            
            sp.Stop();
            Debug.Log($"[{nameof(ReflectionProvider)}] Deserialize reflection at {sp.ElapsedMilliseconds / 1000.0f}s");
        }
        
        public static void Generate()
        {
            DirectReflectionGenerator.Generate(GENERATOR_DIRECTORY);
        }
        
        public static void Clear()
        {
            if (Directory.Exists(ROOT_DIRECTORY))
            {
                Directory.Delete(ROOT_DIRECTORY, true);

                string meta = $"{ROOT_DIRECTORY}.meta";
                if (File.Exists(meta))
                {
                    File.Delete(meta);
                }
            }
        }
        
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Window/DI/Save", secondaryPriority = 1)]
        private static void SaveMenuItem()
        {
            Save();
            UnityEditor.AssetDatabase.Refresh();
        }

        [UnityEditor.MenuItem("Window/DI/Load", secondaryPriority = 2)]
        private static void LoadMenuItem()
        {
            Load();
        }
        
        [UnityEditor.MenuItem("Window/DI/Generate", secondaryPriority = 3)]
        private static void GenerateMenuItem()
        {
            Load();
            Generate();
            UnityEditor.AssetDatabase.Refresh();
        }
        
        [UnityEditor.MenuItem("Window/DI/Clear", secondaryPriority = 4)]
        private static void ClearMenuItem()
        {
            Clear();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
    }
}