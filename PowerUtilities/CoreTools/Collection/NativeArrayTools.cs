using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace PowerUtilities
{
    public static class NativeArrayTools
    {
        [RuntimeInitializeOnLoadMethod]
        static void OnFirstRun()
        {
            Application.quitting -= Dispose;
            Application.quitting += Dispose;
        }

        [CompileStarted]
        private static void Dispose()
        {
            var arr = allArraySet.ToArray();
            foreach (var item in arr)
            {
                item.Dispose();
            }
        }

        static HashSet<IDisposable> allArraySet = new HashSet<IDisposable>();
        /// <summary>
        /// inst is not Created create new
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <param name="array"></param>
        /// <param name="allocator"></param>
        public static void CreateIfNull<T>(ref NativeArray<T> inst, T[] array, Allocator allocator) where T : struct
        {
            if (inst.IsCreated)
                return;

            inst = new NativeArray<T>(array, allocator);
            allArraySet.Add(inst);
        }


        public static void CreateIfNull<T>(ref NativeArray<T> inst, int count, Allocator allocator) where T : struct
        {
            if (inst.IsCreated)
                return;

            inst = new NativeArray<T>(count, allocator);
            allArraySet.Add(inst);
        }
    }
}
