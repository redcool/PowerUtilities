namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public static class LinqEx
    {
        public static void ForEach<T>(this IEnumerable<T> q, Action<T> act)
        {
            if (q == null || act == null)
                return;

            foreach (var item in q)
                act(item);
        }

        public static void ForEach<T>(this IEnumerable<T> q, Action<T, int> act)
        {
            if (q == null || act == null)
                return;

            var id = 0;
            foreach (var item in q)
                act(item, id++);
        }

        public static int FindIndex<T>(this IEnumerable<T> q,Func<T,bool> predication)
        {
            if (predication == null)
                return -1;

            var index = 0;
            foreach (var item in q)
            {
                if (predication(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

    }
}