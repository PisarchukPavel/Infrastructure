using System;
using UnityEngine;

namespace Selectable
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class SelectorEnumAttribute : PropertyAttribute
    {
        public string[] Categories { get; }

        public SelectorEnumAttribute(params string[] categories)
        {
            Categories = categories;
        }
    }
}