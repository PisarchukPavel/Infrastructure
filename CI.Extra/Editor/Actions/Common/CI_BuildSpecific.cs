using System;
using System.Collections.Generic;
using System.IO;
using CI.Editor.Target;
using UnityEditor;
using UnityEngine;

namespace CI.Editor.Pipeline.Actions
{
    [CreateAssetMenu(order = 0, fileName = "Build Specific", menuName = "CI/Action/Common/Build Specific")]
    public class CI_BuildSpecific : CI_Action
    {
        [SerializeField] 
        private bool _internalRemove = false;
        
        [SerializeField] 
        private bool _recompileRequest = false;
     
        [SerializeField]
        private List<SpecificData<ePlatformType>> _platformSpecifics = new List<SpecificData<ePlatformType>>();

        [SerializeField]
        private List<SpecificData<eEnvironmentType>> _environmentSpecifics = new List<SpecificData<eEnvironmentType>>();

        protected override bool Run()
        {
            List<string> saveFolders = new List<string>();
            saveFolders.AddRange(_platformSpecifics.Find(x => x.Key == Context.PlatformType)?.Folders ?? Array.Empty<string>());
            saveFolders.AddRange(_environmentSpecifics.Find(x => x.Key == Context.EnvironmentType)?.Folders ?? Array.Empty<string>());
            
            foreach (SpecificData<ePlatformType> platformSpecific in _platformSpecifics)
            {
                if (platformSpecific.Key != Context.PlatformType)
                {
                    Remove(platformSpecific.Folders, saveFolders);
                }
            }
            
            foreach (SpecificData<eEnvironmentType> environmentSpecific in _environmentSpecifics)
            {
                if (environmentSpecific.Key != Context.EnvironmentType)
                {
                    Remove(environmentSpecific.Folders, saveFolders);
                }
            }
            
            if (_recompileRequest)
            {
                PlatformHelper.Refresh();
            }
            else
            {
                AssetDatabase.Refresh();
            }

            return true;
        }
        
        private void Remove(IEnumerable<string> folders, List<string> saveFolders)
        {
            bool remove = Context.ExternalCall || _internalRemove;
            string root = null; // TODO PP Not need now
            
            foreach (string removeFolder in folders)
            {
                if(saveFolders.Contains(removeFolder))
                    continue;
                
                string directory = string.IsNullOrEmpty(root) ? 
                    $"{removeFolder}".Replace("\\", "/") : 
                    $"{root}/{removeFolder}".Replace("\\", "/");
                
                if (Directory.Exists(directory))
                {
                    if (remove)
                    {
                        Directory.Delete(directory, true);
                        Debug.Log($"[{nameof(CI_BuildSpecific)}] Remove \"{directory}\" folder.");
                    }
                    else
                    {
                        Debug.Log($"[{nameof(CI_BuildSpecific)}] Remove \"{directory}\" folder (emulated, not really remove).");
                    }
                }
                else
                {
                    Debug.Log($"[{nameof(CI_BuildSpecific)}] Not remove \"{directory}\", folder not exist.");
                }
            }
        }

        [Serializable]
        private class SpecificData<T> where T : Enum
        {
            public T Key => _key;
            public IReadOnlyList<string> Folders => _folders;

            [SerializeField]
            private T _key = default;

            [SerializeField] 
            private List<string> _folders = new List<string>();
        }
    }
}