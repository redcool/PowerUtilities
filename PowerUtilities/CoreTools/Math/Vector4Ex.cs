using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.CRP
{
    public static class Vector4Ex
    {

        public static Vector4 Mul(this Vector4 a, Vector4 b)
        {
            return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }

        public static ref Vector4 Vector4(ref this Vector4 v, params float[] nums)
        {
            if (nums.Length > 4 || nums.Length < 2)
                throw new ArgumentException("length must be 2 <= nums length <= 4");

            //var v = new Vector4();
            for (int i = 0; i < nums.Length; i++)
            {
                v[i] = nums[i];
            }
            return ref v;
        }


        public static Vector4 Vector4(params float[] nums)
        {
            var v = new Vector4();
            var count = Mathf.Min(nums.Length, 4);
            for (int i = 0; i < count; i++)
            {
                v[i] = nums[i];
            }
            return v;
        }
    }
}
