using System;
using System.Reflection;

namespace LightContainer.Reflection
{
    [Serializable]
    public class ParameterInformation
    {
        public int Id => _id;
        public Type Type => _type;
        public bool HasDefault => _hasDefault;
        public bool IsOut => _isOut;
        public bool IsRef => _isRef;
        
        private int _id = default;
        private Type _type = null;
        private bool _hasDefault = false;
        private bool _isOut = false;
        private bool _isRef = false;
        
        public ParameterInformation(ParameterInfo parameterInfo)
        {
            _type = parameterInfo.ParameterType;
            _id = $"{_type.FullName}".GetHashCode();
            _hasDefault = parameterInfo.HasDefaultValue;
            _isOut = parameterInfo.IsOut;
            _isRef = parameterInfo.ParameterType.IsByRef;
        }
    }
}