using UnityEngine;

namespace CI.Editor.Pipeline
{
    public abstract class CI_Action : ScriptableObject
    {
        protected BuildContext Context { get; private set; }
        
        public bool Execute(BuildContext context)
        {
            Context = context;
            return Run();
        }
        
        protected abstract bool Run();
    }
}