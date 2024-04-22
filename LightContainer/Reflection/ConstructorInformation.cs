using System;
using System.Reflection;

namespace LightContainer.Reflection
{
    [Serializable]
    public class ConstructorInformation
    {
        public int Id => _id;
        public ConstructorInfo Constructor => _constructor;
        public ParameterInformation[] Parameters => _parameters;

        private int _id = default;
        private ConstructorInfo _constructor = null;
        private ParameterInformation[] _parameters = null;

        public ConstructorInformation(Type type, ConstructorInfo constructorInfo)
        {
            _constructor = constructorInfo;
            
            // Cache parameters
            ParameterInfo[] parameters = _constructor.GetParameters();
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

            _id = $"{type.FullName}.Constructor.({parametersId})".GetHashCode();
        }
    }
}