using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightContainer.Reflection
{
    [Serializable]
    public class TypeInformation
    {
        public Type Type => _type;
        public MethodInformation[] Methods => _methods;
        public ConstructorInformation[] Constructors => _constructors;
        
        private Type _type = null;
        private MethodInformation[] _methods = null;
        private ConstructorInformation[] _constructors = null;

        private const string CONSTRUCT_METHOD = "Construct";
        private const BindingFlags SEARCH_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;
            
        public TypeInformation(Type type)
        {
            _type = type;
            CacheMethods(type);
            CacheConstructors(type);
        }

        private void CacheMethods(Type type)
        {
            Type currentType = type;
            List<MethodInfo> methods = new List<MethodInfo>();

            while (currentType != null)
            {
                IEnumerable<MethodInfo> currentTypeMethods = currentType.GetMethods(SEARCH_FLAGS).Where(MatchedMethod);
                methods.AddRange(currentTypeMethods);
                currentType = currentType.BaseType;
            }

            _methods = new MethodInformation[methods.Count];
            for (int i = 0; i < methods.Count; i++)
            {
                MethodInfo methodInfo = methods[i];
                MethodInformation methodInformation = new MethodInformation(type, methodInfo);
                _methods[i] = methodInformation;
            }
        }

        private void CacheConstructors(Type type)
        {
            List<ConstructorInfo> constructors = new List<ConstructorInfo>(type.GetConstructors());
            constructors.RemoveAll(info => !MatchedConstructor(info));
            
            _constructors = new ConstructorInformation[constructors.Count];
            for (int i = 0; i < constructors.Count; i++)
            {
                ConstructorInfo constructorInfo = constructors[i];
                ConstructorInformation constructorInformation = new ConstructorInformation(type, constructorInfo);
                _constructors[i] = constructorInformation;
            }
        }
        
        private bool MatchedMethod(MethodInfo methodInfo)
        {
            string[] attributes = methodInfo
                .GetCustomAttributes(true)
                .Select(x => x.GetType().FullName)
                .ToArray();
          
            foreach (string attribute in attributes)
            {
                if (BreakByAttribute(attribute))
                    return false;
            }
            
            if (methodInfo.Name == CONSTRUCT_METHOD)
                return true;
            
            foreach (string attribute in attributes)
            {
                if (MatchedByAttribute(attribute))
                    return true;
            }
            
            return false;
        }
        
        private bool MatchedConstructor(ConstructorInfo constructorInfo)
        {
            string[] attributes = constructorInfo
                .GetCustomAttributes(true)
                .Select(x => x.GetType().FullName)
                .ToArray();
          
            foreach (string attribute in attributes)
            {
                if (BreakByAttribute(attribute))
                    return false;
            }
            
            return true;
        }
        
        private bool BreakByAttribute(string attribute)
        {
            return attribute.EndsWith("NonInject") || attribute.EndsWith("NonInjectAttribute");
        }
        
        private bool MatchedByAttribute(string attribute)
        {
            return attribute.EndsWith("Inject") || attribute.EndsWith("InjectAttribute");
        }
    }
}