using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Selectable
{
    public class SelectorDatabase : ScriptableObject, IEnumerable<SelectorElement>
    {
        public string Name => _name;
        public string Separator => _separator;
        public IReadOnlyList<SelectorElement> Entries => _entries;

        [SerializeField] 
        private int _last = 0;
        
        [SerializeField]
        private string _name = string.Empty;
        
        [SerializeField]
        private string _separator = "/";
        
        [SerializeField] 
        private List<SelectorElement> _entries = new List<SelectorElement>();
        
        private static readonly SelectorElement NullElement = new SelectorElement("-1", "null");
        private static readonly Dictionary<Type, SelectorEnumAttribute> EnumCache = new Dictionary<Type, SelectorEnumAttribute>();

        public string FindId(Enum key)
        {
            return FindElement(key).Id;
        }
        
        public string FindId(string key)
        {
            return FindElement(key).Id;
        }
        
        public string FindName(Enum key)
        {
            return FindElement(key).Name;
        }
        
        public string FindName(string key)
        {
            return FindElement(key).Name;
        }
        
        private SelectorElement FindElement(Enum key)
        {
            Type enumType = key.GetType();
            if (!EnumCache.TryGetValue(enumType, out SelectorEnumAttribute selectorCategoryAttribute))
            {
                selectorCategoryAttribute = enumType.GetCustomAttributes(typeof(SelectorEnumAttribute), true).FirstOrDefault() as SelectorEnumAttribute;
                EnumCache.Add(enumType, selectorCategoryAttribute);
            }

            if (selectorCategoryAttribute != null)
            {
                foreach (string category in selectorCategoryAttribute.Categories)
                {
                    string smartKey = string.IsNullOrEmpty(category) ?
                        key.ToString() : 
                        $"{category.Replace("/", _separator)}{_separator}{key.ToString()}";

                    SelectorElement result = _entries.Find(x => x.IsMatch(smartKey));
                    if (result != null)
                        return result;
                }

                return NullElement;
            }
            else
            {
                string typeName = key.GetType().Name;
                if (typeName.Length >= 2 && typeName.StartsWith("e") && Char.IsUpper(typeName[1]))
                {
                    typeName = typeName.Remove(0, 1);
                }
            
                string shortKey = key.ToString();
                string fullKey = $"{typeName}{_separator}{shortKey}";
            
                SelectorElement result = _entries.Find(x => x.IsMatch(fullKey)) ?? 
                                         _entries.Find(x => x.IsMatch(shortKey));

                return result ?? NullElement;
            }
        }
        
        private SelectorElement FindElement(string key)
        {
            return _entries.Find(x => x.IsMatch(key)) ?? NullElement;
        }

        IEnumerator<SelectorElement> IEnumerable<SelectorElement>.GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<SelectorElement>) this).GetEnumerator();
        }
    }
}