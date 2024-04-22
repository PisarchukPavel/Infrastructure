using System;
using System.Collections.Generic;
using System.Linq;
using CI.Editor.Pipeline;
using CI.Editor.Target;
using UnityEditor;
using UnityEngine;

namespace CI.Editor.Menu
{
    public class ActionWindow : EditorWindow
    {
        private string _actionName = string.Empty;
        private string _buildPath = $"C:";
        private ePlatformType _platformType = ePlatformType.Windows;
        private eEnvironmentType _environmentType = eEnvironmentType.DEV;
        private string _additionalArguments = string.Empty;

        private int _selected = 0;
        private string[] _actionNames = Array.Empty<string>();
        private List<CI_Action> _actions = new List<CI_Action>();

        [MenuItem("Window/CI/Action")]
        private static void ShowWindow()
        {
            ActionWindow buildWindow = (ActionWindow)EditorWindow.GetWindow(typeof(ActionWindow));
            buildWindow.CacheActions();
            buildWindow.Show();
        }
        
        private void CacheActions()
        {
            _actions = AssetDatabase
                .FindAssets($"t:{nameof(CI_Action)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<CI_Action>)
                .ToList();
            
            _actionNames = new string[_actions.Count];
            for (var i = 0; i < _actionNames.Length; i++)
            {
                _actionNames[i] = $"{i + 1}: {_actions[i].name}";
            }
        }

        private void OnEnable()
        {
            Load();
        }

        private void OnGUI()
        {
            if (_actions.Count > 0)
            {
                _selected = EditorGUILayout.Popup("Action", _selected, _actionNames);
                _actionName = _actions[_selected].name;
            }
            else
            {
                _selected = EditorGUILayout.Popup("Action", 0, new []{ "NONE" });
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
                
                BuildContext buildContext = new BuildContext(false, _actionName, _buildPath, _platformType, _environmentType);
                _actions[_selected].Execute(buildContext);
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