using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Selectable.Editor
{
    public abstract class SelectorDatabaseInspectorBase<T> : UnityEditor.Editor where T : ScriptableObject
    {
        private T _target = null;

        private int _index = 0;
        private int _removeIndex = -1;
        private string _separator = null;
        private SerializedProperty _elementsProperty = null;
        
        private readonly Dictionary<string, SerializedProperty> _propertiesCache = new Dictionary<string, SerializedProperty>();
        
        private const string NONE_GROUP = "No Category";

        public void OnEnable()
        {
            _target = target as T;
            _separator = Find("_separator").stringValue;
            _separator = string.IsNullOrEmpty(_separator) ? "/" : _separator;
            _propertiesCache.Clear();
        }
    
        public override void OnInspectorGUI()
        {
            OnBeforeElements();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject(_target), typeof(T), false);
            GUI.enabled = true;

            bool expandedSettings = EditorGUILayout.Foldout(GetExpand("Settings"), "Settings", true);
            SetExpand("Settings", expandedSettings);

            if (expandedSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(Find("_name"));
                EditorGUILayout.PropertyField(Find("_display"));

                EditorGUI.BeginChangeCheck();

                SerializedProperty separatorProperty = Find("_separator");
                string prevSeparator = separatorProperty.stringValue;
                EditorGUILayout.DelayedTextField(separatorProperty);

                if (EditorGUI.EndChangeCheck())
                {
                    SerializedProperty elementsProperty = Find("_entries");
                    foreach (SerializedProperty prop in elementsProperty)
                    {
                        SerializedProperty nameProperty = Find(prop, "_name");
                        nameProperty.stringValue = nameProperty.stringValue.Replace(prevSeparator, separatorProperty.stringValue);
                    }

                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();

                    EditorUtility.SetDirty(_target);
                    AssetDatabase.SaveAssetIfDirty(_target);
                    AssetDatabase.Refresh();

                    return;
                }
                
                EditorGUI.indentLevel--;
            }

            bool expandedEntries = EditorGUILayout.Foldout(GetExpand("Entries"), "Entries", true);
            SetExpand("Entries", expandedEntries);
            
            if (expandedEntries)
            {
                EditorGUI.indentLevel++;
                DrawElements();
                EditorGUI.indentLevel--;
            }

            OnAfterElements();
            
            EditorUtility.SetDirty(_target);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawElements()
        {
            SerializedProperty elementsProperty = Find("_entries");
            List<SerializedProperty> elementProperties = new List<SerializedProperty>();
            _elementsProperty = elementsProperty;
            
            foreach (SerializedProperty p in elementsProperty)
            {
                elementProperties.Add(p);
            }

            bool haveEmptyGroupItem = false;
            HashSet<string> groupsSet = new HashSet<string>();
            foreach (SerializedProperty prop in elementProperties)
            {
                string group = Find(prop, "_name").stringValue;
                group = GetGroup(group);
                string[] subGroups = group.Split(_separator);
                haveEmptyGroupItem = string.IsNullOrEmpty(group) || haveEmptyGroupItem;
                
                group = subGroups[0];
                for (int i = 0; i < subGroups.Length; i++)
                {
                    groupsSet.Add(group);
                    
                    if(subGroups.Length > i + 1)
                        group += $"{_separator}{subGroups[i + 1]}";
                }
            }

            List<string> groupList = groupsSet.OrderBy(x => x).ToList();
            if (haveEmptyGroupItem)
            {
                groupList.RemoveAt(0);
                groupList.Add(string.Empty);
            }

            foreach (string group in groupList)
            {
                DrawGroup(group, elementProperties);
            }
            
            if (_removeIndex != -1)
            {
                elementsProperty.DeleteArrayElementAtIndex(_removeIndex);
                _removeIndex = -1;
            }

            if (!haveEmptyGroupItem)
            {
                DrawAddButton();
            }
        }

        private void DrawGroup(string group, List<SerializedProperty> elementProperties)
        {
            string[] subGroups = group.Split(_separator);
            string shortGroupName = subGroups.Last();

            EditorGUI.indentLevel += subGroups.Length;

            if (string.IsNullOrEmpty(shortGroupName))
                shortGroupName = NONE_GROUP;
            
            if (IsRootGroupExpanded(group))
            {
                Rect rect = EditorGUILayout.GetControlRect();
                bool expanded = EditorGUI.Foldout(rect, GetExpand(group), GUIContent.none, false, EditorStyles.foldout) || string.IsNullOrEmpty(group);

                Event e = Event.current;
                if (rect.Contains(e.mousePosition) && shortGroupName != NONE_GROUP)
                {
                    EditorGUI.BeginChangeCheck();
                    string newGroupName = EditorGUI.DelayedTextField(rect, group);
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (SerializedProperty property in elementProperties)
                        {
                            SerializedProperty nameProperty = Find(property, "_name");
                            string propertyName = nameProperty.stringValue;
                            if (!propertyName.StartsWith(group))
                                continue;
                            
                            propertyName = propertyName.Remove(0, group.Length);
                            if (propertyName.StartsWith(_separator))
                            {
                                propertyName = propertyName.Remove(0, 1);
                            }
                            
                            if (newGroupName.Length != 0)
                            {
                                propertyName = $"{newGroupName}{_separator}{propertyName}";
                            }
                            
                            nameProperty.stringValue = propertyName;
                        }

                        SetExpand(newGroupName, expanded);
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }
                }
                else
                {
                    EditorGUI.LabelField(rect, shortGroupName);
                }
                
                SetExpand(group, expanded);
                
                if (expanded)
                {
                    EditorGUI.indentLevel++;
                    bool haveElements = false;
                    _index = 0;
                    foreach (SerializedProperty elementProperty in elementProperties)
                    {
                        string fullName = Find(elementProperty, "_name").stringValue;
                        string elementGroup = GetGroup(fullName);
                        
                        if (elementGroup == group)
                        {
                            DrawElement(elementProperty);
                            haveElements = true;
                        }
                        _index++;
                    }
                    EditorGUI.indentLevel--;

                    if (haveElements)
                    {
                        DrawAddButton(group);
                    }
                }
            }

            EditorGUI.indentLevel -= subGroups.Length;
        }

        private string GetGroup(string fullName)
        {
            string[] splitGroup = fullName.Split(_separator);
            fullName = string.Empty;
            for (int i = 0; i < splitGroup.Length - 1; i++)
            {
                fullName += $"{splitGroup[i]}{_separator}";
            }
            if (fullName.EndsWith(_separator))
            {
                fullName = fullName.Remove(fullName.Length - 1, 1);
            }

            return fullName;
        }

        private bool IsRootGroupExpanded(string group)
        {
            string partGroup = string.Empty;
            string[] split = group.Split(_separator);

            for (int i = 0; i < split.Length - 1; i++)
            {
                string s = split[i];
                partGroup += $"{s}";

                bool expanded = GetExpand(partGroup);

                if (!expanded)
                    return false;

                partGroup += _separator;
            }

            return true;
        }

        private void DrawElement(SerializedProperty property)
        {
            EditorGUILayout.BeginHorizontal();
            {
                int identLevel = EditorGUI.indentLevel; 
                {
                    SerializedProperty idProperty = Find(property, "_id");
                    idProperty.stringValue = string.IsNullOrEmpty(idProperty.stringValue) ? Guid.NewGuid().ToString("N").ToLower() : idProperty.stringValue;

                    Event e = Event.current;
                    Rect rect = EditorGUILayout.GetControlRect();
                    if (rect.Contains(e.mousePosition))
                    {
                        SerializedProperty nameProperty = Find(property, "_name");
                        nameProperty.stringValue = EditorGUI.DelayedTextField(rect, nameProperty.stringValue);
                    }
                    else
                    {
                        SerializedProperty nameProperty = Find(property, "_name");
                        string bundleName = nameProperty.stringValue.Split(_separator).Last();
                        EditorGUI.DelayedTextField(rect, bundleName);   
                    }

                    EditorGUI.indentLevel = 0;
                    OnAfterElement(property);
                    
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_winbtn_mac_close_a"), GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight * 1.5f)))
                    {
                        _removeIndex = _index;
                    }
                }
                EditorGUI.indentLevel = identLevel;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAddButton(string group = null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_CreateAddNew")),
                EditorStyles.miniButtonRight, GUILayout.Width(EditorGUIUtility.singleLineHeight * 2)))
            {
                _elementsProperty.InsertArrayElementAtIndex(_elementsProperty.arraySize);
                SerializedProperty last = _elementsProperty.GetArrayElementAtIndex(_elementsProperty.arraySize - 1);
                ResetElement(last, group);
                OnCreateElement(last);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ResetElement(SerializedProperty property, string group = null)
        {
            int highestId = Find("_last").intValue;

            if (_elementsProperty.arraySize > 0)
            {
                foreach (SerializedProperty prop in _elementsProperty)
                {
                    string stringId = Find(prop, "_id").stringValue;
                    if(int.TryParse(stringId, out int id))
                    {
                        if (id > highestId)
                        {
                            highestId = id;
                        }
                    }
                }
            }

            highestId++;
            Find("_last").intValue = highestId;
            
            Find(property, "_id").stringValue = highestId.ToString();
            Find(property, "_name").stringValue = string.IsNullOrEmpty(group) ? string.Empty : $"{group}{_separator}";
        }
        
        protected virtual void OnBeforeElements()
        { 
            // NONE
        }
        
        protected virtual void OnAfterElements()
        { 
            // NONE
        }

        protected virtual void OnAfterElement(SerializedProperty parent)
        {
            // NONE
        }

        protected virtual void OnCreateElement(SerializedProperty parent)
        {
            // NONE
        }

        protected bool GetExpand(string key)
        {
            return EditorPrefs.GetBool($"{nameof(SelectorDatabaseInspector)}_{_target.GetInstanceID()}_{key}", false);
        }

        protected void SetExpand(string key, bool value)
        {
            EditorPrefs.SetBool($"{nameof(SelectorDatabaseInspector)}_{_target.GetInstanceID()}_{key}", value);
        }

        protected SerializedProperty Find(string path)
        {
            if (_propertiesCache.TryGetValue(path, out SerializedProperty result))
            {
                return result;
            }
        
            result = serializedObject.FindProperty(path);
            _propertiesCache.Add(path, result);
            
            return result;
        }
        
        protected SerializedProperty Find(SerializedProperty parent, string path)
        {
            string key = $"{parent.propertyPath}_{path}";
            if (_propertiesCache.TryGetValue(key, out SerializedProperty result))
            {
                return result;
            }
        
            result = parent.FindPropertyRelative(path);
            _propertiesCache.Add(key, result);
            
            return result;
        }
    }
}