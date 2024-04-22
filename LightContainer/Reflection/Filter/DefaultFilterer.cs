using System;
using System.Collections.Generic;

namespace LightContainer.Reflection.Filter
{
    public class DefaultFilterer : IFilterer
    {
        int IFilterer.Order => int.MinValue;
        bool IFilterer.Enabled => true;

        void IFilterer.Process(IReadOnlyList<Type> original, HashSet<Type> result)
        {
            Filter excludeAll = new Filter(eFilterAction.Exclude, eFilterTarget.Assembly, eFilterType.All, string.Empty);
            Filter includeBase = new Filter(eFilterAction.Include, eFilterTarget.Assembly, eFilterType.StartWith, "Assembly-CSharp");
            Filter excludeEditor = new Filter(eFilterAction.Exclude, eFilterTarget.Assembly, eFilterType.StartWith, "Assembly-CSharp-Editor");
            
            FilterGroup group = new FilterGroup(excludeAll, includeBase, excludeEditor);
            group.Process(original, result);
        }
    }
}