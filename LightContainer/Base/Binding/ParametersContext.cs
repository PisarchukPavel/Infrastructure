using System;
using System.Collections.Generic;

namespace LightContainer.Base.Binding
{
    public class ParametersContext
    {
        public IReadOnlyDictionary<Type, object> Additional => _additional;

        private readonly Dictionary<Type, object> _additional = null;

        public ParametersContext(params object[] additional)
        {
            _additional = new Dictionary<Type, object>();
            foreach (object obj in additional)
            {
                Type type = obj.GetType();
                _additional.Add(type, obj);
            }
        }
        
        public ParametersContext(IEnumerable<object> additional)
        {
            _additional = new Dictionary<Type, object>();
            foreach (object obj in additional)
            {
                Type type = obj.GetType();
                _additional.Add(type, obj);
            }
        }
        
        public ParametersContext(IReadOnlyDictionary<Type, object> additional)
        {
            _additional = new Dictionary<Type, object>();
            foreach (KeyValuePair<Type, object> kv in additional)
            {
                _additional.Add(kv.Key, kv.Value);
            }
        }
        
        public ParametersContext(params (Type type, object obj)[] parameters)
        {
            _additional = new Dictionary<Type, object>();
            foreach ((Type type, object obj) tuple in parameters)
            {
                _additional.Add(tuple.type, tuple.obj);
            }
        }

        public object Get(Type type)
        {
            _additional.TryGetValue(type, out object value);
            return value;
        }
        
        public void Set(Type type, object value)
        {
            if (!_additional.ContainsKey(type))
            {
                _additional.Add(type, value);
            }
            else
            {
                _additional[type] = value;
            }
        }
    }
}