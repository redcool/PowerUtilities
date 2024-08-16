using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class RectEx
    {
        public static Rect Add(this Rect a, Rect b)
        {
            return new Rect(a.x+b.x, a.y+b.y, a.width+b.width, a.height+b.height);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rect Mul(this Rect a, Rect b)
        {
            return new Rect(a.x*b.x, a.y*b.y, a.width*b.width, a.height*b.height);
        }

        public static float Area(this Rect a)
        => a.width *a.height;

        /// <summary>
        /// Resize tto encapsulate b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static void Encapsulate(this Rect a, Rect b)
        {
            float xMin = Mathf.Min(a.xMin, b.xMin);
            float yMin = Mathf.Min(a.yMin, b.yMin);
            float xMax = Mathf.Max(a.xMax, b.xMax);
            float yMax = Mathf.Max(a.yMax, b.yMax);
            a.Set(xMin, yMin, xMax - xMin, yMax - yMin);
        }
    }
}
