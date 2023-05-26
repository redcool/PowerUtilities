using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class ReflectionTools
    {
        public static IEnumerable<Type> GetTypesDerivedFrom<T>(Func<Type, bool> predication)
        {
            var types = typeof(T).Assembly.GetTypes();

            if (predication == null)
                return Enumerable.Empty<Type>();

            return types.Where(predication);
        }
        public static IEnumerable<Type> GetTypesDerivedFrom<T>()
        {
            var tType = typeof(T);
            return GetTypesDerivedFrom<T>(t => t.BaseType != null && t.BaseType == tType);
        }

        public static IEnumerable<Type> GetAppDomainTypesDerivedFrom<T>()
        {
            var tType = typeof(T);
            return GetAppDomainTypesDerivedFrom<T>(t => t.BaseType != null && t.BaseType == tType);
        }

        public static IEnumerable<Type> GetAppDomainTypesDerivedFrom<T>(Func<Type,bool> predication)
        {
            if(predication == null)
                return Enumerable.Empty<Type>();

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes().Where(predication))
            ;
        }
    }
}
