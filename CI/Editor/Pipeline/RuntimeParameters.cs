using System.Collections.Generic;

namespace CI.Editor.Pipeline
{
    public class RuntimeParameters
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        
        public void Set<T>(string key, T value)
        {
            if (_parameters.ContainsKey(key))
            {
                _parameters[key] = value;
            }
            else
            {
                _parameters.Add(key, value);
            }
        }

        public T Get<T>(string key)
        {
            if(_parameters.TryGetValue(key, out object value) && value is T valueAsT)
            {
                return valueAsT;
            }

            return default;
        }
    }
}