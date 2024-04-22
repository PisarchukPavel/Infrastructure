using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;

namespace LightContainer.Unity.Reflection
{
    public class ReflectionBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        int IOrderedCallback.callbackOrder => int.MaxValue;
        
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            ReflectionProvider.Save();

            if (!SourceGeneratorExist())
            {
                ReflectionProvider.Generate();
                CompilationPipeline.RequestScriptCompilation();
            }
            
            AssetDatabase.Refresh();
        }
        
        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
        {
            ReflectionProvider.Clear();
            AssetDatabase.Refresh();
        }

        private bool SourceGeneratorExist()
        {
            return AssetDatabase.FindAssets("LightContainer.Generator.dll").Length > 0;
        }
    }
}