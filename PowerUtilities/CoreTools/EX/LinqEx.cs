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

            //foreach (var item in q)
            //    act(item);
            var count = q.Count();
            for (int i = 0; i < q.Count(); i++)
            {
                act(q.ElementAt(i));
            }
        }

        public static void ForEach<T>(this IEnumerable<T> q, Action<T, int> act)
        {
            if (q == null || act == null)
                return;

            var id = 0;
            //foreach (var item in q)
            //    act(item, id++);
            var count = q.Count();
            for (int i = 0; i < q.Count(); i++)
            {
                act(q.ElementAt(i), i);
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> q,Func<T,bool> predicate,int startIndex=0)
        {
            if (predicate == null)
                return -1;

            for (int i = startIndex; i < q.Count(); i++)
            {
                if (predicate(q.ElementAt(i)))
                {
                    return i;
                }
            }
            return -1;
        }

        public static List<int> FindIndexAll<T>(this IEnumerable<T> q,Func<T,bool> predicate)
        {
            var list = new List<int>();
            for (int i = 0; i<q.Count(); i++)
            {
                if(predicate(q.ElementAt(i)))
                    list.Add(i);
            }
            return list;
        }
    }
}