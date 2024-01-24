using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Auto amanage Persistent NativeArray 
    /// </summary>
    public static class NativeArrayTools
    {
        [RuntimeInitializeOnLoadMethod]
        static void OnFirstRun()
        {
            Dispose();
            Application.wantsToQuit -= Application_wantsToQuit;
            Application.wantsToQuit += Application_wantsToQuit;
        }

        private static bool Application_wantsToQuit()
        {
            Dispose();
            return true;
        }

        [CompileStarted]
        private static void OnDestroyNative(object context)
        {
            Dispose();
        }

        private static void Dispose()
        {
            var arr = allArraySet.ToArray();
            for (int i = 0;i<arr.Length;i++)
            {
                var item = arr[i];
#if UNITY_EDITOR
                Debug.Log("dispose arr:"+item);
#endif
                item.Dispose();
                item = default;
            }
            allArraySet.Clear();
        }


        static HashSet<IDisposable> allArraySet = new HashSet<IDisposable>();

        public static bool IsValid<T>(this NativeArray<T> arr) where T : struct
        => (arr != default && arr.IsCreated);

        /// <summary>
        /// inst is not Created create new
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <param name="array"></param>
        /// <param name="allocator"></param>
        public static void CreateIfNull<T>(ref NativeArray<T> inst, T[] array, Allocator allocator) where T : struct
        {
            if (IsValid(inst))
                return;

            inst = new NativeArray<T>(array, allocator);
            //allArraySet.Add(inst);
        }


        public static void CreateIfNull<T>(ref NativeArray<T> inst, int count, Allocator allocator) where T : struct
        {
            
            if (IsValid(inst))
                return;

            inst = new NativeArray<T>(count, allocator);
            //allArraySet.Add(inst);
        }
    }
}
