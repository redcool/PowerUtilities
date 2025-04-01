using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class ArrayTools
    {
        public static T[] Create<T>(int count,T initValue)
        {
            var arr = new T[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = initValue;
            }
            return arr;
        }

        public static T[] Create<T>(int count, params T[] values)
        {
            count = Mathf.Max(count, values.Length);

            var arr = new T[count];
            for (int i = 0; i < values.Length; i++)
            {
                arr[i] = values[i];
            }
            return arr;
        }

        public static bool IsValid<T>(this T[] array)
        => array != null && array.Length > 0;
        
    }
}
