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

        public static bool IsValid<T>(this NativeArray<T> arr) where T : struct
        => (arr.IsCreated);

        /// <summary>
        /// inst is not Created create new
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        /// <param name="array"></param>
        /// <param name="allocator"></param>
        public static void CreateIfNull<T>(ref NativeArray<T> inst, T[] array, Allocator allocator=Allocator.Domain) where T : struct
        {
            if (IsValid(inst))
                return;

            inst = new NativeArray<T>(array, allocator);
        }


        public static void CreateIfNull<T>(ref NativeArray<T> inst, int count, Allocator allocator = Allocator.Domain) where T : struct
        {
            
            if (IsValid(inst))
                return;
            
            inst = new NativeArray<T>(count,allocator);
        }
    }
}
