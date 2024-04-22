using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Selectable.Editor
{
    [CustomPropertyDrawer(typeof(SelectorAttribute), useForChildren: true)]
    public class SelectorAttributeDrawer : PropertyDrawer
    {
        private List<string> _names = null;
        private List<SelectorElement> _entries = null;
        private SelectorDatabase _database = null;

        private const string EMPTY_NAME = "None";
        private const string EDIT_NAME = "Edit";
        private const string CREATE_PATH = "Assets/Selectors";

        private void PrepareData(SerializedProperty property)
        {
            if(_database != null && _entries != null)
                return;
            
            SelectorAttribute selectorAttribute = (SelectorAttribute)attribute;

            _names = new List<string>();
            _entries = new List<SelectorElement>();
            _database ??= Get(selectorAttribute.Name);
            
            _entries.Add(new SelectorElement("-1", string.Empty));
            _entries.Add(new SelectorElement("-1", string.Empty));
            _names.Add(EMPTY_NAME);
            _names.Add(string.Empty);

            foreach (SelectorElement entry in _database.Entries)
            {
                if (string.IsNullOrEmpty(selectorAttribute.Group) || entry.Name.StartsWith(selectorAttribute.Group))
                {
                    _entries.Add(entry);
                    _names.Add($"{entry.Name.Replace(_database.Separator, "/")}");
                }
            }

            _names.Add(string.Empty);
            _names.Add(EDIT_NAME);

            if (!_entries.Exists(x => x.IsMatch(property.stringValue)) && !string.IsNullOrEmpty(property.stringValue))
            {
                SelectorElement unknown = new SelectorElement(property.stringValue, $"Unknown: {property.stringValue}");
                _entries.Insert(1, unknown);
                _names.Insert(1, unknown.Name);
            }
        }
        
        private SelectorDatabase Get(string name)
        {
            List<SelectorDatabase> result = Load(name);
            
            if (result.Count == 0)
            {
                Create(name);
                result = Load(name);
            }

            if (result.Count > 1)
            {
                string paths = string.Empty;
                foreach (string path in result.Select(AssetDatabase.GetAssetPath))
                {
                    paths += $"\n{path}";
                }
                
                Debug.LogError($"Find {name} selector {result.Count} duplicates, use first. \nPaths: {paths}");
            }

            return result[0];
        }
        
        private static List<SelectorDatabase> Load(string name)
        {
            List<SelectorDatabase> result = AssetDatabase
                .FindAssets($"t:{typeof(SelectorDatabase).FullName}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SelectorDatabase>)
                .ToList();

            return result.FindAll(x => x.Name == name);
        }        
        
        private static void Create(string name)
        {
            if (!Directory.Exists(CREATE_PATH))
            {
                Directory.CreateDirectory(CREATE_PATH);
            }

            string path = $"{CREATE_PATH}/{nameof(SelectorDatabase)}_{name}.asset";
            SelectorDatabase instance = ScriptableObject.CreateInstance<SelectorDatabase>();
           
            FieldInfo nameField = typeof(SelectorDatabase).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(x => x.Name == "_name");
            nameField?.SetValue(instance, name);
            
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Create {name} selector database at {path}. This is just the initial location, you can transfer it to any folder without consequences", instance);
        }
        
        private int FindSelected(SerializedProperty property)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Id == property.stringValue || _entries[i].Name == property.stringValue)
                {
                    return i;
                }
            }

            return 0;
        }

        private void RefreshName(SerializedProperty property, string display)
        {
            string path = ReplaceLastOccurrence(property.propertyPath, property.name, $"{property.name}ArrayDisplay");
            SerializedProperty nameProperty = property.serializedObject.FindProperty(path);
            if (nameProperty != null)
            {
                if (nameProperty.stringValue != display)
                {
                    nameProperty.stringValue = string.IsNullOrEmpty(display) ? EMPTY_NAME : display;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
            }
        }
        
        private string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);
    
            if (place == -1)
                return source;
    
            return source.Remove(place, find.Length).Insert(place, replace);
        }
        
        // Draw old
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.Integer)
            {
                PrepareData(property);
                DrawGenericMenu(position, property);
                DrawField(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, $"{nameof(SelectorAttribute)} support only string or int value type.");
            }
        }
        
        private void DrawGenericMenu(Rect position, SerializedProperty property)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 1 && position.Contains(e.mousePosition))
            {
                GenericMenu context = new GenericMenu();
                
                context.AddItem(new GUIContent("Copy"), false, () =>
                {
                    EditorGUIUtility.systemCopyBuffer = property.stringValue;
                });
           
                if (!string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer))
                {
                    Tuple<string, SerializedObject> pathWithObject = new Tuple<string, SerializedObject>(property.propertyPath, property.serializedObject);
                    context.AddItem(new GUIContent("Paste"), false, SetValue, pathWithObject);
                }
                else
                {
                    context.AddDisabledItem(new GUIContent("Paste"), false);
                }

                context.ShowAsContext();
            }
        }
        
        private void SetValue(object data)
        {
            Tuple<string, SerializedObject> pathWithObject = (Tuple<string, SerializedObject>) data;
            SerializedObject serializedObject = (SerializedObject) pathWithObject.Item2;
            SerializedProperty property = serializedObject.FindProperty(pathWithObject.Item1);
            property.stringValue = EditorGUIUtility.systemCopyBuffer;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawField(Rect position, SerializedProperty property, GUIContent label)
        {
            bool array = property.propertyPath.Contains("Array") && property.propertyPath.EndsWith("]");
            if (!array && label != null && !string.IsNullOrEmpty(label.text))
            {
                EditorGUI.LabelField(position, label);
                position = RecalculateAfterLabel(position, 0, 1);
            }

            int selectedIndex = EditorGUI.Popup(position, FindSelected(property), _names.ToArray());

            if (_names[selectedIndex] == EDIT_NAME)
            {
                Selection.activeObject = _database;
            }
            else
            {
                property.stringValue = _entries[selectedIndex].Id;
            }

            RefreshName(property, _entries[selectedIndex].Name);
        }

        private Rect RecalculateAfterLabel(Rect original, int order, int max)
        {
            float space = 2.0f;
            float startX = original.x;
            float maxWidth = original.width - EditorGUIUtility.labelWidth;
            float widthPerElement = maxWidth / max - space;
            float offset = EditorGUIUtility.labelWidth + space;
           
            Rect pos = new Rect(startX + offset + widthPerElement * order, original.y, widthPerElement, original.height);
            return pos;
        }
        
        
        // Draw new
        private PopupField<string> _popup = null;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                PrepareData(property);
                RefreshName(property, _entries[FindSelected(property)].Name);

                PopupField<string> popup = new PopupField<string>(_names, FindSelected(property));
                {
                    popup.style.flexGrow = 1.0f;
                    popup.AddManipulator(new ContextualMenuManipulator(evt => ShowContextMenu(evt, property)));
                    popup.RegisterCallback<ChangeEvent<string>>(evt =>
                    {
                        int selectedIndex = popup.index;
                        if (popup.value == EDIT_NAME)
                        {
                            Selection.activeObject = _database;
                        }
                        else
                        {
                            property.stringValue = _entries[selectedIndex].Id;
                            property.serializedObject.ApplyModifiedProperties();
                            popup.MarkDirtyRepaint();
                        }
                        
                        RefreshName(property, _entries[selectedIndex].Name);
                    });
                    
                    bool arrayPart = property.propertyPath.Contains("Array") && property.propertyPath.EndsWith("]");
                    if (!arrayPart)
                    {
                        popup.label = property.displayName;
                        popup.AddToClassList(BaseField<string>.alignedFieldUssClassName);
                    }
                }

                _popup = popup;
                return popup;
            }
            else
            {
                Label label = new Label();
                label.text = $"{nameof(SelectorAttributeDrawer)} support only string value type.";

                return label;
            }
        }
        
        private void ShowContextMenu(ContextualMenuPopulateEvent evt, SerializedProperty property)
        {
            evt.menu.AppendAction("Copy", copy =>
            {
                EditorGUIUtility.systemCopyBuffer = property.stringValue;
            }, DropdownMenuAction.AlwaysEnabled);
            
            evt.menu.AppendAction("Paste", copy =>
            {
                property.stringValue = EditorGUIUtility.systemCopyBuffer;
                property.serializedObject.ApplyModifiedProperties();

                _popup.index = FindSelected(property);
                RefreshName(property, _entries[_popup.index].Name);

                _popup.MarkDirtyRepaint();
            }, string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
        }
    }
}