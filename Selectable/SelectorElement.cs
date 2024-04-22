using System;
using UnityEngine;

namespace Selectable
{
    [Serializable]
    public class SelectorElement
    {
        public string Id => _id;
        public string Name => _name;

        [SerializeField] 
        private string _id = null;
        
        [SerializeField] 
        private string _name = null;

        public SelectorElement(string id, string name)
        {
            _id = id;
            _name = name;
        }
        
        public bool IsMatch(string key)
        {
            return _id == key || _name == key;
        }
    }
}