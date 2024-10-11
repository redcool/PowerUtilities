using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class DelegateEx
    {
        public static Delegate[] GetInvocationListSafe(this Delegate d)
        {
            return d != null ? d.GetInvocationList() : Array.Empty<Delegate>();
        }
        /// <summary>
        /// {MethodInfo -> {caller -> Delegate}}
        /// </summary>
        static Dictionary<MethodInfo, Dictionary<object, Delegate>> delegateDict = new Dictionary<MethodInfo, Dictionary<object, Delegate>>();

        static Dictionary<MethodInfo, Delegate> staticMethoddelegataDict = new Dictionary<MethodInfo, Delegate>();

        [CompileFinished]
        static void OnCompiledDone()
        {
            delegateDict.Clear();
            staticMethoddelegataDict.Clear();
        }
        /// <summary>
        /// get a delegate with cache,if not exists create by method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">caller</param>
        /// <param name="method">caller's method or null(static method)</param>
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
                return (T)DictionaryTools.Get(staticMethoddelegataDict, method, (m) =>
                {
                    return Create(target, method, dType);
                });
            }
            //------------ target delegate
            var dict = DictionaryTools.Get(delegateDict, method, (m) =>
            {
                if (!delegateDict.TryGetValue(m,out var dict))
                {
                    dict = delegateDict[m] = new Dictionary<object, Delegate>();
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
    }
}
