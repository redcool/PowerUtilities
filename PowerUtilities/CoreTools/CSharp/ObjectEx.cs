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
        /// <typeparam name="DelegateType"></typeparam>
        /// <param name="target"></param>
        /// <param name="m"></param>
        /// <param name="args"></param>
        public static object InvokeDelegate<DelegateType>(this object target,MethodInfo m,params object[] args)
            where DelegateType : Delegate
        {
            return DelegateEx.GetOrCreate<DelegateType>(target, m).DynamicInvoke(args);
        }

        /// <summary>
        /// Call method use reflection with delegate(
        /// </summary>
        /// <typeparam name="DelegateType"></typeparam>
        /// <typeparam name="ReturnType"></typeparam>
        /// <param name="target"></param>
        /// <param name="m"></param>
        /// <param name="args"></param>
        /// <returns>ReturnType</returns>
        public static ReturnType InvokeDelegate<DelegateType,ReturnType>(this object target, MethodInfo m, params object[] args)
            where DelegateType : Delegate
        {
            return (ReturnType)DelegateEx.GetOrCreate<DelegateType>(target, m).DynamicInvoke(args);
        }
    }
}
