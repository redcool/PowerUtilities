namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public static class LinqEx
    {

        public static void For<T>(this IEnumerable<T> q, Action<int> action)
        {
            if (q == null || action == null)
                return;

            var count = q.Count();
            for (var i = 0; i<count; i++)
                action(i);
        }

        public static void ForEach<T>(this IEnumerable<T> q, Action<T> action, Func<T, bool> validPredicate = null)
        {
            if (q == null || action == null)
                return;

            var count = q.Count();
            for (var i = 0; i < count; i++)
            {
                var item = q.ElementAt(i);
                if (validPredicate != null ? validPredicate.Invoke(item) : true)
                    action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> q, Action<T, int> action, Func<T, int, bool> validPredicate = null)
        {
            if (q == null || action == null)
                return;

            var count = q.Count();
            for (var i = 0; i < count; i++)
            {
                var item = q.ElementAt(i);
                if (validPredicate != null ? validPredicate.Invoke(item, i) : true)
                    action(item, i);
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
            var count = q.Count();
            for (int i = 0; i< count; i++)
            {
                if(predicate(q.ElementAt(i)))
                    list.Add(i);
            }
            return list;
        }

        public static void SetData<T>(this IList<T> q,T t)
        {
            q.For(i => q[i] = t);
        }
    }
}