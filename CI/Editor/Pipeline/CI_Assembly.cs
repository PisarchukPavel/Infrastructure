using System.Collections.Generic;
using UnityEngine;

namespace CI.Editor.Pipeline
{
    [CreateAssetMenu(order = 0, fileName = "Assembly", menuName = "CI/Assembly")]
    public class CI_Assembly : ScriptableObject
    {
        public string Id => _id;
        
        [SerializeField]
        private string _id = string.Empty;
        
        [SerializeField]
        private List<CI_Action> _actions = new List<CI_Action>();

        public bool Execute(BuildContext context)
        {
            Debug.Log($"[{nameof(CI_Assembly)}] Start {_id} build assembly");
   
            foreach (CI_Action action in _actions)
            {
                Debug.Log($"[{nameof(CI_Assembly)}] Start {action.GetType().Name} ci action");

                if (action is IConditionalAction conditionalAction && !conditionalAction.Check())
                {
                    Debug.Log($"[{nameof(CI_Assembly)}] Skip {action.GetType().Name} ci action");
                    continue;
                }
                
                if (!action.Execute(context))
                {
                    if(action == null)
                        continue;
                    
                    Debug.Log($"[{nameof(CI_Assembly)}] Fail {action.GetType().Name} ci action");
                    return false;
                }
                
                if(action == null)
                    continue;
                
                Debug.Log($"[{nameof(CI_Assembly)}] Complete {action.GetType().Name} ci action");
            }
            
            return true;
        }
    }
}