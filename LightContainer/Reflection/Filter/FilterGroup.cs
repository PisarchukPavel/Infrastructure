using System;
using System.Collections.Generic;
using System.Linq;

namespace LightContainer.Reflection.Filter
{
    public class FilterGroup
    {
        private readonly List<Filter> _filters = null;

        public FilterGroup(params Filter[] filters)
        {
            _filters = filters.ToList();
        }
        
        public FilterGroup(IEnumerable<Filter> filters)
        {
            _filters = filters.ToList();
        }

        public void Process(IReadOnlyList<Type> original, HashSet<Type> result)
        {
            foreach (Filter filter in _filters)
            {
                filter.Process(original, result);
            }
        }
    }
}