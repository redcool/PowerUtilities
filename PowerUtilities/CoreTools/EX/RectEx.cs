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

        public static Rect Mul(this Rect a, Rect b)
        {
            return new Rect(a.x*b.x, a.y*b.y, a.width*b.width, a.height*b.height);
        }
    }
}
