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
        public static List<T> Shuffle<T>(List<T> originalList, int maxCount,out List<int> shuffleIds)
        {
            shuffleIds = GetShuffle(maxCount);

            var list = new List<T>();
            for (int i = 0; i < shuffleIds.Count; i++)
            {
                var newId = shuffleIds[i];
                list.Add(originalList[newId]);
            }
            return list;
        }


        public static List<int> GetShuffle(int maxCount)
        {
            var tmpList = Enumerable.Range(0, maxCount).ToList();
            var list = new List<int>();
            var c = tmpList.Count;
            for (int i = 0; i < c; i++)
            {
                var r = Random.Range(0, tmpList.Count);
                list.Add(tmpList[r]);

                tmpList.RemoveAt(r);
            }
            return list;
        }
    }
}
