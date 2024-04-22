using System;
using System.Collections.Generic;
using CI.Editor.Target;
using UnityEditor;
using UnityEngine;

namespace CI.Editor.Pipeline.Actions
{
    [CreateAssetMenu(order = 0, fileName = "Build Option", menuName = "CI/Action/Common/Build Option")]
    public class CI_BuildOption : CI_Action
    {
        [SerializeField] 
        private BuildOptions _baseOptions = BuildOptions.None;
        
        [SerializeField]
        private List<OptionData<ePlatformType>> _platformOptions = new List<OptionData<ePlatformType>>();
        
        [SerializeField]
        private List<OptionData<eEnvironmentType>> _environmentOptions = new List<OptionData<eEnvironmentType>>();
        
        protected override bool Run()
        {
            ModifyOptions(Context.EnvironmentType, Context.PlatformType);
            return true;
        }

        private void ModifyOptions(eEnvironmentType environmentType, ePlatformType platformType)
        {
            int baseOptions = (int)_baseOptions;
            BuildOptions result = (BuildOptions)baseOptions;
            
            OptionData<ePlatformType> platformDefines = _platformOptions.Find(x => x.Key == platformType);
            if (platformDefines != null)
            {
                result = result | platformDefines.BuildOptions;
            }
            
            OptionData<eEnvironmentType> environmentDefines = _environmentOptions.Find(x => x.Key == environmentType);
            if (environmentDefines != null)
            {
                result = result | environmentDefines.BuildOptions;
            }
            
            Context.ChangeOptions(Context.BuildOptions.options | result);
        }

        [Serializable]
        private class OptionData<T> where T : Enum
        {
            public T Key => _key;
            public BuildOptions BuildOptions
            {
                get
                {
                    int value = (int) _buildOptions;
                    return (BuildOptions) value;
                }
            }

            [SerializeField] 
            private T _key = default;
           
            [SerializeField] 
            private BuildOptions _buildOptions = BuildOptions.None;
        }
    }
}