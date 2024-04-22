using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using LightContainer.Reflection.Filter;
using DotNetAssembly = System.Reflection.Assembly;

namespace LightContainer.Reflection
{
    [Serializable]
    public class TypeStorage
    {
        private static TypeStorage _instance = null;
        private static TypeStorage Instance => _instance ??= new TypeStorage(Array.Empty<Type>());
        private static List<string> ExcludeIn { get; set; } = new() { "UnityEngine.", "UnityEditor.", "Unity.", "LightContainer." };
        private static List<string> ExcludeBased { get; set; } = new() { "System.Attribute", "UnityEditor.", "LightContainer."};
        
        private readonly Dictionary<Type, TypeInformation> _source = null;
        
        private TypeStorage(IEnumerable<Type> types)
        {
            _source = new Dictionary<Type, TypeInformation>();

            foreach (Type type in types)
            {
                if (!_source.ContainsKey(type))
                {
                    _source.Add(type, new TypeInformation(type));
                }
            }
        }

        public static bool Available(Type type)
        {
            return Get(type) != null;
        }

        public static TypeInformation Get(Type type)
        {
            if (Instance._source.TryGetValue(type, out TypeInformation result))
            {
                return result;
            }
            else
            {
                if (IsExclude(type))
                    return null;
                
                result = new TypeInformation(type);
                Instance._source.Add(type, result);
                
                PrintLog($"Save missing {type.FullName} type in cache");

                return result;
            }
        }

        public static List<TypeInformation> Get()
        {
            return Instance._source.Values.ToList();
        }
        
        public static byte[] Collect()
        {
            List<Type> types = CollectTypes();
            TypeStorage storage = new TypeStorage(types);
            
            BinaryFormatter formatter = new BinaryFormatter();
            using MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, storage);
            byte[] data = stream.ToArray();

            return data;
        }

        public static void Upload(byte[] data)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using MemoryStream stream = new MemoryStream(data);
                TypeStorage result = (TypeStorage) formatter.Deserialize(stream);
              
                _instance = result;
                
                PrintLog("Deserialize binary reflection success");
            }
            catch (Exception e)
            {
                _instance ??= new TypeStorage(Array.Empty<Type>());

                PrintError($"Deserialize binary reflection failed\n {e}");
            }
        }
        
        private static List<Type> CollectTypes()
        {
            List<Type> types = FindAssembliesTypes();
            
            List<Type> current = new List<Type>(types);
            HashSet<Type> result = new HashSet<Type>(types);
            
            List<IFilterer> filterers = types
                .FindAll(t => !t.IsAbstract && !t.IsInterface && typeof(IFilterer).IsAssignableFrom(t))
                .Select(t => Activator.CreateInstance(t) as IFilterer)
                .Where(t => t != null && t.Enabled)
                .OrderBy(t => t.Order)
                .ToList();

            foreach (IFilterer filterer in filterers)
            {
                filterer.Process(current, result);
            }

            //StringBuilder sb = new StringBuilder();
            string resultLog = $"Cache {result.Count} types\n";
            foreach (Type type in result)
            {
                resultLog += $"{type.FullName ?? type.Name}\n";
            }
            PrintLog(resultLog);
            
            return result.ToList();
        }

        private static List<Type> FindAssembliesTypes()
        {
            List<Type> result = new List<Type>(2000);
            List<DotNetAssembly> dotNetAssemblies = FindAssemblies();

            foreach (DotNetAssembly assembly in dotNetAssemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (IsInstanceType(type) && IsPublicType(type))
                    {
                        result.Add(type);
                    }
                }
            }

            result.RemoveAll(IsExclude);
            
            return result;
        }

        private static bool IsInstanceType(Type t)
        {
            return
                ((t.IsValueType && !t.IsEnum) || t.IsClass)
                && !t.IsAbstract
                && !t.IsGenericType;
        }
        
        private static bool IsPublicType(Type t)
        {
            return
                t.IsVisible
                && t.IsPublic
                && !t.IsNotPublic
                && !t.IsNested
                && !t.IsNestedPublic
                && !t.IsNestedFamily
                && !t.IsNestedPrivate
                && !t.IsNestedAssembly
                && !t.IsNestedFamORAssem
                && !t.IsNestedFamANDAssem;
        }
        
        private static List<DotNetAssembly> FindAssemblies()
        {
            List<DotNetAssembly> netAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

#if UNITY_5_3_OR_NEWER
            List<UnityEditor.Compilation.Assembly> unityAssemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player).ToList();
            netAssemblies.RemoveAll(x => !unityAssemblies.Exists(y => x.GetName().Name == y.name));
#endif
            
            return netAssemblies;
        }
        
        private static bool IsExclude(Type type)
        {
            string typeName = type.FullName ?? string.Empty;
            if (ExcludeIn.Exists(x => typeName.StartsWith(x)))
                return true;
            
            Type current = type;
            while (current != null)
            {
                if (current.FullName != null && ExcludeBased.Exists(x => current.FullName.StartsWith(x)))
                    return true;

                current = current.BaseType;
            }

            return false;
        }
        
        private static void PrintLog(object message)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.Log($"[{nameof(TypeStorage)}] {message}");
#else
            System.Console.WriteLine($"[{nameof(TypeStorage)}] {message}");
#endif
        }

        private static void PrintError(object message)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogError($"[{nameof(TypeStorage)}] {message}");
#else
            System.Console.WriteLine($"[{nameof(TypeStorage)}] {message}");
#endif
        }
    }
}