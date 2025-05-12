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
        /// <summary>
        /// Add x, y, width and height of a by b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rect Add(this Rect a, Rect b)
        {
            return new Rect(a.x+b.x, a.y+b.y, a.width+b.width, a.height+b.height);
        }

        /// <summary>
        /// Multiply x, y, width and height of a by b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rect Mul(this Rect a, Rect b)
        {
            return new Rect(a.x*b.x, a.y*b.y, a.width*b.width, a.height*b.height);
        }
        /// <summary>
        /// Multiply width and height of a by b, but keep x and y the same
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rect MulWH(this Rect a, Rect b)
        {
            return new Rect(a.x, a.y, a.width * b.width, a.height * b.height);
        }
        /// <summary>
        /// Add x, y, width and height of a by b, but keep x and y the same
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Convert Rect to Vector4
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector4 ToVector4(this Rect a)
        {
            return new Vector4(a.x, a.y, a.width, a.height);
        }
    }
}
