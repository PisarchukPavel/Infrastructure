using System;
using System.Collections.Generic;
using Bootstrap.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;
using UnityObject = UnityEngine.Object;

namespace Bootstrap.Helper
{
    public class SceneUtils : MonoBehaviour
    {
        private static SceneUtils _instance = null;

        private const int DONT_DESTROY_HANDLE = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnApplicationStarted()
        {
            LazyInstance();
        }

        private static SceneUtils LazyInstance()
        {
            if (_instance != null)
                return _instance;

            GameObject go = new GameObject(nameof(SceneUtils));
            DontDestroyOnLoad(go);

            _instance = go.AddComponent<SceneUtils>();
            _instance.transform.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            return _instance;
        }

        public static void Enter<T>(T data)
        {
            Enter<T>(data, ActiveScene().handle);
        }
        
        public static void Enter<T>(T data, int handle)
        {
            FindAll<IEntry<T>>(handle).ForEach(x => x.Enter(data));
        }
        
        /// <summary>
        /// Find in active scene
        /// </summary>
        public static T Find<T>() where T : class
        {
            return Find<T>(ActiveScene().handle);
        }

        /// <summary>
        /// Find in concrete scene, for DontDestroyOnLoad scene send handle -1
        /// </summary>        
        public static T Find<T>(int handle) where T : class
        {
            GameObject[] rootObjects = GetScene(handle).GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects)
            {
                T result = rootObject.GetComponentInChildren<T>(true);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Find in active scene
        /// </summary>
        public static List<T> FindAll<T>() where T : class
        {
            return FindAll<T>(ActiveScene().handle);
        }

        /// <summary>
        /// Find in concrete scene, for DontDestroyOnLoad scene send handle -1
        /// </summary>  
        public static List<T> FindAll<T>(int handle) where T : class
        {
            List<T> result = new List<T>();
            GameObject[] rootObjects = GetScene(handle).GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects)
            {
                T[] elements = rootObject.GetComponentsInChildren<T>(true);
                result.AddRange(elements);
            }

            return result;
        }

        /// <summary>
        /// Find all loaded (instantiated) scenes by name
        /// </summary>  
        public static List<UnityScene> FindScenes(string name)
        {
            List<UnityScene> result = new List<UnityScene>();

            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                UnityScene scene = SceneManager.GetSceneAt(i);
                if (scene.name == name)
                {
                    result.Add(scene);
                }
            }

            return result;
        }

        public static string GetName(int handle)
        {
            return GetScene(handle).name;
        }
        
        /// <summary>
        /// For DontDestroyOnLoad scene send handle -1
        /// </summary>  
        public static UnityScene GetScene(int handle)
        {
            if (handle == DONT_DESTROY_HANDLE)
                return LazyInstance().gameObject.scene;

            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                UnityScene scene = SceneManager.GetSceneAt(i);
                if (scene.handle == handle)
                {
                    return scene;
                }
            }

            throw new NullReferenceException($"Scene with {handle} handle is not exist");
        }

        public static UnityScene ActiveScene()
        {
            return SceneManager.GetActiveScene();
        }
    }
}