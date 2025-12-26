namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public static class LinqEx
    {
        /// <summary>
        /// Like foreach
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="action"></param>
        public static void ForEachIndex<T>(this IEnumerable<T> q, Action<int> action)
        {
            if (q == null || action == null)
                return;

            var count = q.Count();
            for (var i = 0; i<count; i++)
                action(i);
        }
        /// <summary>
        /// Enumerate items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="action">{item}</param>
        /// <param name="validPredicate">{item}</param>
        public static void ForEach<T>(this IEnumerable<T> q, Action<T> action, Func<T, bool> validPredicate = null)
        {
            if (q == null || action == null)
                return;

            //var count = q.Count();
            //for (var i = 0; i < count; i++)
            //{
            //    var item = q.ElementAt(i);
            //    if (validPredicate != null ? validPredicate.Invoke(item) : true)
            //        action(item);
            //}

            foreach (var item in q)
            {
                if (validPredicate != null ? validPredicate.Invoke(item) : true)
                    action(item);
            }
        }
        /// <summary>
        /// Enumerate items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="q"></param>
        /// <param name="action">{item,itemindex}</param>
        /// <param name="validPredicate">{item,itemindex}</param>
        public static void ForEach<T>(this IEnumerable<T> q, Action<T, int> action, Func<T, int, bool> validPredicate = null)
        {
            if (q == null || action == null)
                return;

            var id = 0;
            foreach (var item in q)
            {
                if (validPredicate != null ? validPredicate.Invoke(item,id) : true)
                    action(item,id);

                id++;
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
            q.ForEachIndex(i => q[i] = t);
        }

        /// <summary>
        /// find MaxValue's item from list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T FindMax<T, TResult>(this IEnumerable<T> list, Func<T, TResult> predicate) where TResult : IComparable
        {
            var count = list.Count();
            TResult maxValue = default;
            T maxItem = default;
            for (int i = 0; i < count; i++)
            {
                var item = list.ElementAt(i);
                var value = predicate(item);

                if (value.CompareTo(maxValue) > 0)
                {
                    maxValue = value;
                    maxItem = item;
                }
            }
            return maxItem;
        }
        /// <summary>
        /// Split source into chunks
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
            return source.Select((item, index) => new { item, index })
                .GroupBy(x => x.index / size)
                .Select(g => g.Select(x => x.item).ToArray());
        }

    }
}