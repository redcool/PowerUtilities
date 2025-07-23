using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class ObjectEx
    {
        /// <summary>
        /// Call method use reflection with delegate(faster than methodInfo)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="m"></param>
        /// <param name="args"></param>
        public static object InvokeDelegate<T>(this object target,MethodInfo m,params object[] args)
            where T : Delegate
        {
            if (m == null)
                return default;
            return DelegateEx.GetOrCreate<T>(target, m).DynamicInvoke(args);
        }

        /// <summary>
        /// Call method use reflection with delegate(faster than methodInfo)
        /// Get methodInfo from dict
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="dict"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public static object InvokeDelegate<T>(this object target, ref Dictionary<string, MethodInfo> dict, string methodName, params object[] args) where T : Delegate
        {
            if (dict.ContainsKey(methodName))
                return target.InvokeDelegate<T>(dict[methodName], args);
            return default;
        }

    }
}
