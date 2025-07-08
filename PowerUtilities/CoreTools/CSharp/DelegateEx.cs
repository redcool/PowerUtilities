using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class DelegateEx
    {
        /// <summary>
        /// instance method
        /// {MethodInfo -> {caller -> Delegate}}
        /// </summary>
        static Dictionary<MethodInfo, Dictionary<object, Delegate>> methodTargetDelegateDict = new Dictionary<MethodInfo, Dictionary<object, Delegate>>();
        /// <summary>
        /// static method
        /// {methdoInfo -> delegate}
        /// </summary>
        static Dictionary<MethodInfo, Delegate> methodDelegataDict = new Dictionary<MethodInfo, Delegate>();


        [CompileFinished]
        static void OnCompiledDone()
        {
            methodTargetDelegateDict.Clear();
            methodDelegataDict.Clear();
        }
        /// <summary>
        /// Get a delegate's invocation list,if null return empty array
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Delegate[] GetInvocationListSafe(this Delegate d)
        {
            return d != null ? d.GetInvocationList() : Array.Empty<Delegate>();
        }
        /// <summary>
        /// Get a delegate or get from cache
        /// </summary>
        /// <typeparam name="T">delegate Type</typeparam>
        /// <param name="target">caller,static method target set nul</param>
        /// <param name="method">caller's method info</param>
        /// <param name="isForceCreate">true: force create a new delegate</param>
        /// <returns></returns>
        public static T GetOrCreate<T>(object target, MethodInfo method,bool isForceCreate=false) where T : Delegate
        {
            if (method == null)
                throw new ArgumentNullException("method is null");

            var dType = typeof(T);
            if (isForceCreate)
            {
                return Delegate.CreateDelegate(dType,target,method) as T;
            }

            //----------- static delegate
            if (target == null || method.IsStatic)
            {
                return (T)DictionaryTools.Get(methodDelegataDict, method, (m) =>
                {
                    return Delegate.CreateDelegate(dType, target, method) as T;
                });
            }
            //------------ target delegate
            var dict = DictionaryTools.Get(methodTargetDelegateDict, method, (m) =>
            {
                if (!methodTargetDelegateDict.TryGetValue(m,out var dict))
                {
                    dict = methodTargetDelegateDict[m] = new Dictionary<object, Delegate>();
                }
                return dict;
            });

            return (T)DictionaryTools.Get(dict, target, (target) =>
            {
                return Delegate.CreateDelegate(dType, target, method) as T;
            });

        }

        /// <summary>
        /// Create a delegate from method info
        /// </summary>
        /// <param name="method"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Delegate CreateDelegate(this MethodInfo method, object target)
        {
            var paramReturnTypes = method.GetParameters()
                .Select(p => p.ParameterType)
                .Append(method.ReturnType)
                .ToArray();
            var delegateType = Expression.GetDelegateType(paramReturnTypes);

            if (method.IsStatic)
                return method.CreateDelegate(delegateType);
            return method.CreateDelegate(delegateType, target);
        }

    }
}
