using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CI.Editor.Pipeline.Actions
{
    [CreateAssetMenu(order = 0, fileName = "Scene Link", menuName = "CI/Action/Common/Scene Link")]
    public class CI_SceneLink : CI_Action
    {
        [SerializeField] 
        private List<SceneAsset> _scenes = new List<SceneAsset>();
     
        protected override bool Run()
        {
            string[] scenes = new string[_scenes.Count];
            for (int i = 0; i < _scenes.Count; i++)
            {
                scenes[i] = AssetDatabase.GetAssetOrScenePath(_scenes[i]);
            }
            
            Context.ChangeScenes(scenes);
            
            return true;
        }
    }
}