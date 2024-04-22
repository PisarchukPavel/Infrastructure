using CI.Editor.Pipeline;
using CI.Editor.Target;
using UnityEditor.Build;
using UnityEngine;

namespace CI.Editor
{
    public static class Builder
    {
        // For external call
        public static void Build()
        {
            string assemblyId = BuildUtils.GetAssembly();
            string buildPath = BuildUtils.GetPath();
            ePlatformType platformType = BuildUtils.GetPlatform();
            eEnvironmentType environmentType = BuildUtils.GetEnvironment();

            Build(new BuildContext(true, assemblyId, buildPath, platformType, environmentType));
        }
        
        // For editor call
        public static void Build(BuildContext context)
        {
            if(context.PlatformType == 0)
            {
                throw new BuildFailedException($"Unknown platform!");
            }

            CI_Assembly assembly = BuildUtils.FindAssembly<CI_Assembly>(context.AssemblyId);
            if (assembly != null)
            {
                Debug.Log($"[{nameof(Builder)}] Start build {context.PlatformType}-{context.EnvironmentType} by ({assembly.Id}) custom pipeline");
                
                if (!assembly.Execute(context))
                {
                    throw new BuildFailedException("CI Action fail");
                }
            }
            else
            {
                throw new BuildFailedException($"Assembly with {context.AssemblyId} id not found!");
            }
        }
    }
}