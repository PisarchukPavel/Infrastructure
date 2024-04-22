using System;
using System.Collections.Generic;

namespace LightContainer.Reflection.Filter
{
    public interface IFilterer
    {
        int Order { get; }
        bool Enabled { get; }

        void Process(IReadOnlyList<Type> original, HashSet<Type> result);
    }
}