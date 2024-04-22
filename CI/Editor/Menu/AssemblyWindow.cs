using System;
using System.Collections.Generic;
using System.Linq;
using CI.Editor.Pipeline;
using CI.Editor.Target;
using UnityEditor;
using UnityEngine;

namespace CI.Editor.Menu
{
    public class AssemblyWindow : EditorWindow
    {
        private string _assemblyId = string.Empty;
        private string _buildPath = $"C:";
        private ePlatformType _platformType = ePlatformType.Windows;
        private eEnvironmentType _environmentType = eEnvironmentType.DEV;
        private string _additionalArguments = string.Empty;

        private int _selected = 0;
        private string[] _assemblyIds = Array.Empty<string>();
        private List<CI_Assembly> _assemblies = new List<CI_Assembly>();

        [MenuItem("Window/CI/Assembly")]
        private static void ShowWindow()
        {
            AssemblyWindow assemblyWindow = (AssemblyWindow)EditorWindow.GetWindow(typeof(AssemblyWindow));
            assemblyWindow.CacheAssemblies();
            assemblyWindow.Show();
        }
    
        private void CacheAssemblies()
        {
            _assemblies = AssetDatabase
                .FindAssets($"t:{nameof(CI_Assembly)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<CI_Assembly>)
                .ToList();
            
            _assemblyIds = new string[_assemblies.Count];
            for (var i = 0; i < _assemblyIds.Length; i++)
            {
                _assemblyIds[i] = $"{i + 1}: {_assemblies[i].Id}";
            }
        }
        
        private void OnEnable()
        {
            Load();
        }
        
        private void OnGUI()
        {
            if (_assemblies.Count > 0)
            {
                _selected = EditorGUILayout.Popup("Assembly", _selected, _assemblyIds);
                _assemblyId = _assemblies[_selected].Id;
            }
            else
            {
                _selected = EditorGUILayout.Popup("Assembly", 0, new []{"NONE"});
            }
            
            _buildPath = EditorGUILayout.TextField ("Path", _buildPath);
            _platformType = (ePlatformType)EditorGUILayout.EnumPopup("Platform", _platformType);
            _environmentType = (eEnvironmentType)EditorGUILayout.EnumPopup("Environment", _environmentType);
            _additionalArguments = EditorGUILayout.TextField("Arguments", _additionalArguments);

            if (GUILayout.Button("Run"))
            {
                Save();

                CommandLine.Reset();
                CommandLine.Append(_additionalArguments);
                
                BuildContext buildContext = new BuildContext(false, _assemblyId, _buildPath, _platformType, _environmentType);
                Builder.Build(buildContext);
            }
        } 
        
        private void Save()
        {
            Save(nameof(_selected), _selected);
            Save(nameof(_buildPath), _buildPath);
            Save(nameof(_platformType), _platformType);
            Save(nameof(_environmentType), _environmentType);
            Save(nameof(_additionalArguments), _additionalArguments);
        }

        private void Load()
        {
            int.TryParse(Load(nameof(_selected)), out _selected);
            _buildPath = Load(nameof(_buildPath));
            Enum.TryParse(Load(nameof(_platformType)), out _platformType);
            Enum.TryParse(Load(nameof(_environmentType)), out _environmentType);
            _additionalArguments = Load(nameof(_additionalArguments));
        }
        
        private void Save(string key, object value)
        {
            EditorPrefs.SetString(key, value.ToString());
        }

        private string Load(string key)
        {
            return EditorPrefs.GetString($"{nameof(ActionWindow)}_Prefs_{key}", null);
        }
    }
}