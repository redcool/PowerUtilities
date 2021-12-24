namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class LinqEx
    {
        public static void ForEach<T>(this IEnumerable<T> q, Action<T> act)
        {
            if (act == null)
                return;

            foreach (var item in q)
                act(item);
        }

    }
}