using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    /// <summary>
    /// Auto add to project assembly name
    /// </summary>
    public static partial class AssemblyNames
    {
        public const string POWER_UTILS = nameof(POWER_UTILS);

        public static List<string> GetAssemblyNames() => ReflectionTools.GetFieldValues<string>(typeof(AssemblyNames));
    }
}
