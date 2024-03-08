using System;
using System.Linq;

namespace PowerUtilities
{
    public static class AssemblyTools
    {
        /// <summary>
        /// GetType from currentDomain all assemblies
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="assemblyName"></param>
        /// <param name="matchMode"></param>
        /// <returns></returns>
        public static Type GetType(string typeName, string assemblyName = "", StringEx.NameMatchMode matchMode = StringEx.NameMatchMode.Full)
        {
            var q = from dll in AppDomain.CurrentDomain.GetAssemblies()
                    where !string.IsNullOrEmpty(assemblyName) ? dll.FullName.IsMatch(assemblyName, matchMode) : true
                    let type = dll.GetType(typeName)
                    where type != null
                    select type;
            return q.FirstOrDefault();
        }
    }
}
