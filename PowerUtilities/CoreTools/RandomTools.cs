using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PowerUtilities
{
    public static class RandomTools
    {
        public static Vector3 Range(Vector3 a,Vector3 b)
        {
            a.x = Random.Range(a.x, b.x);
            a.y = Random.Range(a.y, b.y);
            a.z = Random.Range(a.z, b.z);
            return a;
        }


        /// <summary>
        /// shuffle originalList,keep maxCount
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalList"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static List<T> Shuffle<T>(List<T> originalList, int maxCount)
        {
            //shuffle
            var set = new HashSet<int>();
            while (set.Count < maxCount)
            {
                var id = Random.Range(0, originalList.Count);
                if (set.Contains(id))
                    continue;

                set.Add(id);
            }
            // copy into list
            var arr = set.ToArray();
            var list = new List<T>();
            for (int i = 0; i < arr.Length; i++)
            {
                list.Add(originalList[i]);
            }
            return list;
        }

    }
}
