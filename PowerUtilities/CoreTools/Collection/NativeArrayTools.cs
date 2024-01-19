using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace PowerUtilities
{
    public static class NativeArrayTools
    {
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
        }


        public static void CreateIfNull<T>(ref NativeArray<T> inst, int count, Allocator allocator) where T : struct
        {
            if (inst.IsCreated)
                return;

            inst = new NativeArray<T>(count, allocator);
        }
    }
}
