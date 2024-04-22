using System;
using System.Collections.Generic;
using CI.Editor.Target;
using UnityEngine;

namespace CI.Editor.Pipeline.Actions
{
    [CreateAssetMenu(order = 0, fileName = "Build Define", menuName = "CI/Action/Common/Build Define")]
    public class CI_BuildDefine : CI_Action
    {
        [SerializeField]
        private List<string> _baseDefines = new List<string>();
        
        [SerializeField]
        private List<DefineData<ePlatformType>> _platformDefines = new List<DefineData<ePlatformType>>();
        
        [SerializeField]
        private List<DefineData<eEnvironmentType>> _environmentDefines = new List<DefineData<eEnvironmentType>>();
        
        protected override bool Run()
        {
            ModifyDefine(Context.EnvironmentType, Context.PlatformType);
            return true;
        }

        private void ModifyDefine(eEnvironmentType environmentType, ePlatformType platformType)
        {
            List<string> result = new List<string>(_baseDefines);

            string branchDefine = BuildUtils.GetBranch();
            if (!string.IsNullOrEmpty(branchDefine))
            {
                result.Add($"BRANCH_{branchDefine}");    
            }
            
            string ciDefines = BuildUtils.GetDefines();
            if (!string.IsNullOrEmpty(ciDefines))
            {
                result.AddRange(ciDefines.Split(";"));
            }

            DefineData<ePlatformType> platformDefines = _platformDefines.Find(x => x.Key == platformType);
            if (platformDefines != null)
            {
                result.AddRange(platformDefines.Values);
            }
            
            DefineData<eEnvironmentType> environmentDefines = _environmentDefines.Find(x => x.Key == environmentType);
            if (environmentDefines != null)
            {
                result.AddRange(environmentDefines.Values);
            }

            PlatformHelper.ReplaceDefines(result);
        }

        [Serializable]
        private class DefineData<T> where T : Enum
        {
            public T Key => _key;
            public IReadOnlyList<string> Values => _values;

            [SerializeField] 
            private T _key = default;
            
            [SerializeField]
            private List<string> _values = new List<string>();
        }
    }
}