using System;
using System.Collections.Generic;
using System.Linq;

namespace CI.Editor
{
    public static class CommandLine
    {
        private static readonly List<string> AdditionalArguments = new List<string>();
        
        public static string[] Collect()
        {
            List<string> commandLineArgs = Environment.GetCommandLineArgs().ToList();
            commandLineArgs.AddRange(AdditionalArguments);

            return commandLineArgs.ToArray();
        }

        public static void Append(string additionalArguments)
        {
            AdditionalArguments.AddRange(additionalArguments.Split(" "));
        }
        
        public static void Append(params string[] additionalArguments)
        {
            AdditionalArguments.AddRange(additionalArguments);
        }

        public static void Append(IEnumerable<string> additionalArguments)
        {
            AdditionalArguments.AddRange(additionalArguments);
        }
        
        public static void Reset()
        {
            AdditionalArguments.Clear();
        }
    }
}