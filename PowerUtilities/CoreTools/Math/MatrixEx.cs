using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace PowerUtilities
{
    public static class MatrixEx
    {
        public static ref Matrix4x4 Matrix(ref this Matrix4x4 m, float[,] nums)
        {
            if (nums.GetLength(0) != 4)
                throw new ArgumentException("nums length != 4");

            //var m = new Matrix4x4();

            for (int i = 0; i < nums.GetLength(0); i++)
            {
                var v = new Vector4();
                for (int j = 0; j < nums.GetLength(1); j++)
                {
                    v[j] = nums[i, j];
                }
                m.SetRow(i, v);
            }

            return ref m;
        }

        public static ref Matrix4x4 Matrix(ref this Matrix4x4 m, params float[] nums)
        {
            if (nums.GetLength(0) != 16)
                throw new ArgumentException("nums length != 16");

            var count = 4;

            for (int i = 0; i < count; i++)
            {
                var v = new Vector4();
                for (int j = 0; j < count; j++)
                {
                    v[j] = nums[i * count + j];
                }
                m.SetRow(i, v);
            }

            return ref m;
        }

        public static Matrix4x4 Matrix(params float[] nums)
        {
            var m = new Matrix4x4();
            m.Matrix(nums);
            return m;
        }

        public static Vector3 ScaleWorldSpace(this Matrix4x4 matrix)
        {
            //var x = matrix.MultiplyVector(Vector3.right);
            //var y = matrix.MultiplyVector(Vector3.up);
            //var z = matrix.MultiplyVector(Vector3.forward);
            var x = matrix.GetColumn(0);
            var y = matrix.GetColumn(1);
            var z = matrix.GetColumn(2);
            return new Vector3(x.magnitude, y.magnitude, z.magnitude);
        }
        public static Vector3 RightVectorWorldSpace(this Matrix4x4 matrix) => matrix.GetColumn(0).normalized;
        public static Vector3 UpVectorWorldSpace(this Matrix4x4 matrix) => matrix.GetColumn(1).normalized;
        public static Vector3 ForwardVectorWorldSpace(this Matrix4x4 matrix) => matrix.GetColumn(2).normalized;

        public static Vector4[] ToRowVectors(this Matrix4x4 matrix)
        {
            return new[]{
                matrix.GetRow(0),
                matrix.GetRow(1),
                matrix.GetRow(2),
                matrix.GetRow(3),
                };
        }

        public static Vector4[] ToColumnVectors(this Matrix4x4 matrix)
        {
            return new[]{
                matrix.GetColumn(0),
                matrix.GetColumn(1),
                matrix.GetColumn(2),
                matrix.GetColumn(3),
                };
        }

        public static float3x4 ToFloat3x4(this Matrix4x4 m)
            => new float3x4(
                (Vector3)m.GetColumn(0),
                (Vector3)m.GetColumn(1),
                (Vector3)m.GetColumn(2),
                (Vector3)m.GetColumn(3)
                );

        public static float4x3 ToFloat4x3(this Matrix4x4 m)
            => math.transpose(ToFloat3x4(m));

        public static float[] ToColumnArray(this Matrix4x4 m)
        {
            var nums = new float[16];
            for (int i = 0; i < 16; i++)
            {
                nums[i] = m[i];
            }
            return nums;
        }

        public static float[] ToRowArray(this Matrix4x4 m)
        {
            var nums = new float[16];
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    nums[r+c*4] = m[c,r];
                }
            }
            return nums;
        }

        //var m = new Matrix4x4();
        //m.From(new float[] { 
        //1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16
        //});

        //Debug.Log(m);

        //var cols = m.ToColumnArray();
        //Debug.Log(string.Join(',', cols));

        //var rows = m.ToRowArray();
        //Debug.Log(string.Join(',', rows));

        /// <summary>
        /// Construct matrix from float[16], column base
        /// </summary>
        /// <param name="m"></param>
        /// <param name="nums"></param>
        public static void From(ref this Matrix4x4 m,float[] nums)
        {
            for (int i = 0;i < nums.Length; i++)
            {
                m[i] = nums[i];
            }
        }
    }
}
