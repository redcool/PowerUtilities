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
        /// <summary>
        /// flags : private instance
        /// </summary>
        public const BindingFlags PrivateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        //cache type <-> fieldInfo 
        static CacheTool<Type,FieldInfo> typeFieldCache = new CacheTool<Type, FieldInfo>();

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

        public static IEnumerable<Type> GetAppDomainTypesDerivedFrom<T>(Func<Type, bool> predicate)
        {
            if (predicate == null)
                return Enumerable.Empty<Type>();

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes().Where(predicate))
            ;
        }

        public static bool IsImplementOf(this Type type, Type interfaceType)
        {
            return type.GetInterfaces()
                .Any(obj => (obj.IsGenericType && obj.GetGenericTypeDefinition() == interfaceType)
                    || interfaceType.IsAssignableFrom(type)
                )
                ;
        }

        /// <summary>
        /// Get a private field(check cache)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetFieldValue<T>(this Type type, object instance, string fieldName, BindingFlags flags = PrivateInstanceFlags)
        {
            var obj = GetFieldValue(type, instance, fieldName, flags);
            return obj != null ? (T)obj : default;
        }
        public static object GetFieldValue(this Type type, object instance, string fieldName, BindingFlags flags = PrivateInstanceFlags)
        {
            var field = typeFieldCache.Get(type, () => type.GetField(fieldName, flags));

            if (field == null)
                return default;

            return field.GetValue(instance);
        }

        /// <summary>
        /// Get hierarchy object value use Reflection API(slow)
        /// like  ( object1.object2.object3.name )
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldNamesPath"></param>
        /// <returns></returns>
        public static object GetObjectHierarchy(this object instance,string fieldNamesPath)
        {
            if (instance == null || string.IsNullOrEmpty(fieldNamesPath)) 
                return null;

            var fieldNames = fieldNamesPath.SplitBy('.');

            Type instType = instance.GetType();
            FieldInfo field = null;

            foreach (var fieldName in fieldNames)
            {
                field = instType.GetField(fieldName,BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                instance = field.GetValue(instance);

                // next field
                instType = instance.GetType();
            }
            return instance;
        }
    }
}
