using System;
using UnityEngine;

namespace Selectable
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SelectorAttribute : PropertyAttribute
    {
        public string Name { get; }
        public string Group { get; }

        public SelectorAttribute(string name)
        {
            Name = name;
            Group = null;
        }
        
        public SelectorAttribute(string name, string group)
        {
            Name = name;
            Group = group;
        }
    }
}