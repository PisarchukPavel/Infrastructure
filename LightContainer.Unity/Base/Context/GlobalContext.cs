using System;
using System.Collections.Generic;
using LightContainer.Base;
using LightContainer.Unity.Base.Behaviour;
using LightContainer.Unity.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace LightContainer.Unity.Base.Context
{
    [DefaultExecutionOrder(int.MinValue)]
    public class GlobalContext : MonoBehaviour
    {
        private IDependencyContainer Root { get; set; }
        private List<ContainerEntry> Entries { get; set; }
        
        private static GlobalContext Instance => LazyInstance();
        private static GlobalContext _instance = null;
        
        [SerializeField]
        private List<InstallerBehaviour> _installers = new List<InstallerBehaviour>();
        
        private static GlobalContext LazyInstance()
        {
            if (_instance != null)
                return _instance;
            
            SceneManager.sceneUnloaded += OnSceneUnload;
            
            GlobalContext prefab = Resources.Load<GlobalContext>($"{nameof(GlobalContext)}");
            _instance = (prefab == null) ? new GameObject(nameof(GlobalContext)).AddComponent<GlobalContext>() : Instantiate(prefab);
          
            DontDestroyOnLoad(_instance.gameObject);
            _instance.OnCreated();

            return _instance;
        }

        private static void OnSceneUnload(Scene scene)
        {
            if(_instance == null)
                return;
            
            Instance.Entries.RemoveAll(x => x.GameObject == null || x.GameObject.scene == scene || x.GameObject.scene.handle == scene.handle);
        }

        private void OnCreated()
        {
            ReflectionProvider.Load();
            Root = new DependencyContainer();
            Entries = new List<ContainerEntry> { new ContainerEntry(-1, gameObject, Root) };
            
            _installers.ForEach(x =>
            {
                IInstallerBehaviour installer = x;
                installer.Initialize(Root);
                installer.Install();
            });
            
            Root.Enforce();
            InjectSelf();
        }

        private void InjectSelf()
        {
            List<MonoBehaviour> targets = ContextUtils.FindInjectionTargets(gameObject);
            targets.ForEach(x => Root.Inject(x, null));
        }
        
        public static IDisposable Register(BaseContext context)
        {
            GameObject contextGameObject = context.gameObject;
            int id = (context is ObjectContext) ? contextGameObject.GetInstanceID() : contextGameObject.scene.handle;
            IDependencyContainer container = context;

            ContainerEntry entry = new ContainerEntry(id, contextGameObject, container);
            Instance.Entries.Add(entry);
            
            return new EntryDisposer(() =>
            {
                if(_instance == null)
                    return;
                
                Instance.Entries.Remove(entry);
            });
        }
       
        /// <param name="id">-1 for root container, Scene.handle for scene container, GameObject.GetInstanceID() for object container </param>
        public static IDependencyReader Get(int id)
        {
            ContainerEntry entry = Instance.Entries.Find(x => x.Id == id);
            return entry?.Container;
        }

        private class ContainerEntry
        {
            public int Id { get; }
            public GameObject GameObject { get; }
            public IDependencyContainer Container { get; }
            
            public ContainerEntry(int id, GameObject scene, IDependencyContainer container)
            {
                Id = id;
                GameObject = scene;
                Container = container;
            }
        }
        
        private class EntryDisposer : IDisposable
        {
            private Action _disposeAction = null;

            public EntryDisposer(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }
            
            void IDisposable.Dispose()
            {
                _disposeAction?.Invoke();
                _disposeAction = null;
            }
        }
    }
}