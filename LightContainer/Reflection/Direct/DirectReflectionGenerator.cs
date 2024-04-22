using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LightContainer.Reflection.Direct
{
    [Obsolete("Legacy solution, use LightContainer.Generator.dll (based on Microsoft.CodeAnalysis.ISourceGenerator)")]
    public class DirectReflectionGenerator
    {
        private static readonly string CLASS_NAME = $"DirectReflectionGenerated";

        private static readonly string BodyTemplate = @"
using System;
using System.Collections.Generic;

namespace LightContainer.Reflection.Direct
{
    public sealed class DirectReflectionGenerated : DirectReflection
    {
        private readonly Dictionary<int, Action<object, object[]>> _injectMethods = null;
        private readonly Dictionary<int, Func<object[], object>> _resolveMethods = null;

        public DirectReflectionGenerated()
        {
            _injectMethods = new Dictionary<int, Action<object, object[]>>();
[INJECT_METHODS_DECLARATION]

            _resolveMethods = new Dictionary<int, Func<object[], object>>();
[RESOLVE_METHODS_DECLARATION]
        }
        
        public override bool Inject(int id, object target, object[] parameters)
        {
            if (_injectMethods.TryGetValue(id, out Action<object, object[]> injectMethod))
            {
                injectMethod.Invoke(target, parameters);
                return true;
            }

            return false;
        }

        public override bool Resolve(int id, out object result, object[] parameters)
        {
            result = null;
            
            if (_resolveMethods.TryGetValue(id, out Func<object[], object> resolveMethod))
            {
                result = resolveMethod.Invoke(parameters);
                return true;
            }
            
            return false;
        }

[METHODS]
    }
}
";

        public static void Generate(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string path = directory.Replace("\\", "/");
            path = $"{path}/{CLASS_NAME}.cs";

            string source = Generate();
            File.WriteAllText(path, source);
        }

        public static void Clear(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
        
        private static string Generate()
        {
            StringBuilder methods = new StringBuilder();
            StringBuilder injectionMethodsDeclaration = new StringBuilder();
            StringBuilder resolveMethodsDeclaration = new StringBuilder();

            int index = 0;
            List<TypeInformation> cachedTypes = TypeStorage.Get();
            
            foreach (TypeInformation typeInformation in cachedTypes)
            {
                if(typeInformation.Type.IsGenericType)
                    continue;
                
                foreach (MethodInformation methodInformation in typeInformation.Methods)
                {
                    if(!methodInformation.Method.IsPublic)
                        continue;

                    index++;
                    injectionMethodsDeclaration.Append($"{Tab(3)}_injectMethods.Add({methodInformation.Id}, {GenerateMethodName(index)});\n");
                    methods.Append(GenerateInjectMethod(index, typeInformation.Type, methodInformation));
                }
                
                foreach (ConstructorInformation constructorInformation in typeInformation.Constructors)
                {
                    if(!constructorInformation.Constructor.IsPublic)
                        continue;

                    index++;
                    resolveMethodsDeclaration.Append($"{Tab(3)}_resolveMethods.Add({constructorInformation.Id}, {GenerateMethodName(index)});\n");
                    methods.Append(GenerateResolveMethod(index, typeInformation.Type, constructorInformation));
                }
            }

            string source = BodyTemplate;
            source = source.Replace("[CLASS_NAME]", CLASS_NAME);
            source = source.Replace("[INJECT_METHODS_DECLARATION]", injectionMethodsDeclaration.ToString());
            source = source.Replace("[RESOLVE_METHODS_DECLARATION]", resolveMethodsDeclaration.ToString());
            source = source.Replace("[METHODS]", methods.ToString());
            
            return source;
        }

        private static string GenerateInjectMethod(int index, Type target, MethodInformation methodInformation)
        {
            string source = @"
        private void [METHOD_NAME](object target, object[] parameters) 
        { 
            [BODY] 
        }";

            string methodName = GenerateMethodName(index);
            string typeName = ConvertedTypeName(target);
            string methodParameters = GenerateParameters(methodInformation.Parameters);

            source = source.Replace("[METHOD_NAME]", methodName);
            source = source.Replace("[BODY]", $"(({typeName})target).{methodInformation.Method.Name}({methodParameters});");
            
            return $"{source} \n";
        }
        
        private static string GenerateResolveMethod(int index, Type target, ConstructorInformation constructorInformation)
        {
            string source = @"
        private object [METHOD_NAME](object[] parameters) 
        { 
            [BODY] 
        }";

            string methodName = GenerateMethodName(index);
            string typeName = ConvertedTypeName(target);
            string constructorParameters = GenerateParameters(constructorInformation.Parameters);

            source = source.Replace("[METHOD_NAME]", methodName);
            source = source.Replace("[BODY]", $"return new {typeName}({constructorParameters});");
  
            return $"{source} \n";
        }

        private static string GenerateMethodName(int index)
        {
            return $"Generated_{index}";
        }
        
        private static string GenerateParameters(ParameterInformation[] parameters)
        {
            if (parameters.Length == 0)
                return string.Empty;
            
            string result = string.Empty;
            
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInformation parameter = parameters[i];
                string typeName = ConvertedTypeName(parameter.Type);

                if(parameter.IsOut)
                {
                    result += $"out _, ";
                    continue;
                }

                if(parameter.IsRef)
                {
                    result += $"ref ({typeName})parameters[{i}], ";
                    continue;
                }
                
                result += $"({typeName})parameters[{i}], ";
            }

            result = result.Remove(result.Length - 2, 2);
            result = result.Replace("+", ".");
            
            return result;
        }
        
        private static string ConvertedTypeName(Type type)
        {
            string fullTypeName = FullTypeName(type);
            if (fullTypeName.StartsWith("System.Tuple") || fullTypeName.StartsWith("System.ValueTuple"))
            {
                type = type.GetElementType() ?? throw new NullReferenceException();
            }

            if (type.IsArray)
            {
                fullTypeName = ConvertedTypeName(type.GetElementType());
                return $"{fullTypeName}[]".Replace("+", ".");
            }

            if (!type.IsGenericType)
            {
                return (type.FullName ?? type.Name).Replace("+", ".");;
            }
            else
            {
                string fullGenericTypeName = FullTypeName(type.GetGenericTypeDefinition());
                int genericPrefixIndex = fullGenericTypeName.IndexOf('`');
                string genericName =  genericPrefixIndex == -1 ? fullGenericTypeName : fullGenericTypeName.Substring(0, genericPrefixIndex);
                
                List<string> list = type.GetGenericArguments().Select(ConvertedTypeName).ToList();
            
                return $"{genericName}<{string.Join(", ", list)}>".Replace("+", ".");;   
            }
        }

        private static string FullTypeName(Type type)
        {
            string fullname = $"{type.Namespace}." + type.Name.Replace($", {type.Assembly}", string.Empty);
            fullname = fullname.StartsWith(".") ? fullname.Remove(0, 1) : fullname;

            return fullname;
        }

        private static string Tab(int count)
        {
            string result = new string(' ', 4 * count);
            return result;
        }
    }
}