using System;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CI.Editor.Pipeline.Actions
{
    [CreateAssetMenu(order = 0, fileName = "Build", menuName = "CI/Action/Common/Build")]
    public class CI_Build : CI_Action
    {
        protected override bool Run()
        {
            BuildReport buildReport = BuildPipeline.BuildPlayer(Context.BuildOptions);
            ProcessBuildReport(buildReport);
            
            return buildReport.summary.result == BuildResult.Succeeded;
        }
        
        private static void ProcessBuildReport(BuildReport report)
        {
            BuildSummary summary = report.summary;
            
            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log($"[{nameof(CI_Build)}] Build succeeded \n{GetStepLog(report)}");
                    break;
                case BuildResult.Failed:
                case BuildResult.Unknown:
                    Debug.Log($"[{nameof(CI_Build)}] Build failed \n{GetStepLog(report)}");
                    break;
                case BuildResult.Cancelled:
                    Debug.Log($"[{nameof(CI_Build)}] Build cancelled");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetStepLog(BuildReport report)
        {
            StringBuilder log = new StringBuilder();
            
            foreach (BuildStep buildStep in report.steps)
            {
                foreach (BuildStepMessage stepMessage in buildStep.messages)
                {
                    log.Append($"{stepMessage.type.ToString().ToUpper()}: {stepMessage.content}; \n");
                }
            }

            log.Append("\n");
            
            return log.ToString();
        }
    }
}