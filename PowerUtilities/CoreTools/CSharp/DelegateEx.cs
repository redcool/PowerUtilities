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
        /// {MethodInfo -> {caller -> Delegate}}
        /// </summary>
        static Dictionary<MethodInfo, Dictionary<object, Delegate>> methodTargetDelegateDict = new Dictionary<MethodInfo, Dictionary<object, Delegate>>();
        /// <summary>
        /// {methdoInfo -> delegate}
        /// </summary>
        static Dictionary<MethodInfo, Delegate> methodDelegataDict = new Dictionary<MethodInfo, Delegate>();

        /// <summary>
        /// {(object inst,MethodInfo info) -> Delegate}
        /// </summary>
        public static Dictionary<(object inst,MethodInfo info),Delegate> targetMethodDelegateDict = new ();
        /// <summary>
        /// {(object inst,string methodName) -> Delegate}
        /// </summary>
        public static Dictionary<(object inst, string methodName), Delegate> targetMethodNameDelegateDict = new();

        [CompileFinished]
        static void OnCompiledDone()
        {
            methodTargetDelegateDict.Clear();
            methodDelegataDict.Clear();
            targetMethodDelegateDict.Clear();
            targetMethodNameDelegateDict.Clear();
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
        /// Get a delegate with cache,if not exists create by method
        /// </summary>
        /// <typeparam name="T">delegate Type</typeparam>
        /// <param name="target">caller,static method target set nul</param>
        /// <param name="method">caller's method info</param>
        /// <param name="isForceCreate">true: force create a new delegate</param>
        /// <returns></returns>
        public static T GetOrCreate<T>(object target, MethodInfo method,bool isForceCreate=false) where T : Delegate
        {
            var dType = typeof(T);
            if (isForceCreate)
            {
                return Create(target, method, dType);
            }
            //----------- static delegate
            if (target == null)
            {
                return (T)DictionaryTools.Get(methodDelegataDict, method, (m) =>
                {
                    return Create(target, method, dType);
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
                return Create(target, method, dType);
            });

            //------------ inner methods
            static T Create(object target, MethodInfo method, Type dType)
            {
                var d = Delegate.CreateDelegate(dType, target, method);
                return (T)d;
            }
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

        /// <summary>
        /// Create a delegate from method info
        /// </summary>
        /// <param name="target"></param>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static Delegate CreateDelegate(this Type targetType, object target, string methodName, BindingFlags flags, params Type[] paramTypes)
        {
            var m = targetType.GetMethod(methodName, flags, null, paramTypes, null);
            return m.CreateDelegate(target);
        }

        /// <summary>
        /// Create a delegate from method info with cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Delegate GetDelegate(this MethodInfo method, object target)
        {
            return DictionaryTools.Get(targetMethodDelegateDict, (target, method), info => CreateDelegate(method, target));
        }

        /// <summary>
        /// Get a delegate from targetType and method name with cache
        /// </summary>
        /// <param name="target"></param>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static Delegate GetDelegate(this Type targetType, object target, string methodName, BindingFlags flags, params Type[] paramTypes)
        {
            return DictionaryTools.Get(targetMethodNameDelegateDict, (target, methodName), info => CreateDelegate(targetType,target, methodName, flags, paramTypes));
        }

    }
}
