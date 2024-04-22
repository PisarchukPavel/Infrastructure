using System;
using System.Reflection;

namespace LightContainer.Reflection
{
    [Serializable]
    public class MethodInformation
    {
        public int Id => _id;
        public MethodInfo Method => _method;
        public ParameterInformation[] Parameters => _parameters;

        private int _id = default;
        private MethodInfo _method = null;
        private ParameterInformation[] _parameters = null;

        public MethodInformation(Type type, MethodInfo methodInfo)
        {
            _method = methodInfo;
            
            // Cache parameters
            ParameterInfo[] parameters = _method.GetParameters();
            _parameters = new ParameterInformation[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                _parameters[i] = new ParameterInformation(parameter);
            }

            
            // Cache id
            string parametersId = null;
            if (_parameters.Length == 0)
            {
                parametersId = "EMPTY";
            }
            else
            {
                parametersId = string.Empty;
                foreach (ParameterInformation parameterInformation in _parameters)
                {
                    parametersId += $"{parameterInformation.Id}, ";
                }
                parametersId = parametersId.Remove(parametersId.Length - 2, 2);
            }

            _id = $"{type.FullName}.{_method.Name}.({parametersId})".GetHashCode();
        }
    }
}