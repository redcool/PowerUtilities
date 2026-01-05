using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Unity.Mathematics float4x4 extension
    /// </summary>
    public static class Float4x4Ex
    {

        /// <summary>
        /// Construct float3x4 from float[12],column base
        /// </summary>
        /// <param name="m"></param>
        /// <param name="nums"></param>
        public static void From(ref this float3x4 m, float[] nums)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 4; j++)
                    m[j][i] = nums[i + j * 3];
        }
        public static float3 GetRow(this float3x3 m, int id)
            => new float3(m.c0[id], m.c1[id], m.c2[id]);

        public static float4 GetRow(this float3x4 m, int id)
        => new float4(m.c0[id], m.c1[id], m.c2[id], m.c3[id]);

        public static float4 GetRow(this float4x4 m, int id)
        => new float4(m.c0[id], m.c1[id], m.c2[id], m.c3[id]);


        public static void SetRow(ref this float4x4 m, int rowId, float4 v)
        {
            m.c0[rowId] = v.x;
            m.c1[rowId] = v.y;
            m.c2[rowId] = v.z;
            m.c3[rowId] = v.w;
        }

        public static float[] ToColumnArray(this float3x4 m)
        => new float[12]
            {
                m.c0.x,m.c0.y,m.c0.z,
                m.c1.x,m.c1.y,m.c1.z,
                m.c2.x,m.c2.y,m.c2.z,
                m.c3.x,m.c3.y,m.c3.z,
            };
        
        public static float[] ToRowArray(this float3x4 m)
        => new float[12]
            {
                m.c0.x,m.c1.x,m.c2.x,m.c3.x,
                m.c0.y,m.c1.y,m.c2.y,m.c3.y,
                m.c0.z,m.c1.z,m.c2.z,m.c3.z,
            };

        public static Vector4[] ToRowVectors(this float3x4 m)
        => new Vector4[]{
                m.GetRow(0),
                m.GetRow(1),
                m.GetRow(2),
                };

        public static Vector3[] ToColumnVectors(this float3x4 m)
        => new Vector3[]{
                m.c0,m.c1,m.c2,m.c3
                };

        /// <summary>
        /// Fast get inverse matrix from (rotation and translation) matrix
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static float4x4 FastInverse(this float4x4 tr)
        {
            // pos inverse
            var pos = -tr.c3.xyz;
            //rot inverse
            tr = math.transpose(tr);
            var t0 = math.dot(tr.GetRow(0).xyz, pos);
            var t1 = math.dot(tr.GetRow(1).xyz, pos);
            var t2 = math.dot(tr.GetRow(2).xyz, pos);

            tr.c3 = new float4(t0, t1, t2, 1.0f);
            // set row3
            tr.SetRow(3, new float4(0, 0, 0, 1));

            return tr;
        }
        /// <summary>
        /// Construct a inversed look-at matrix from the eye position, target position, and up vector.
        /// </summary>
        /// <param name="eye"></param>
        /// <param name="target"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        public static float4x4 LookAtInverse(Vector3 eye, Vector3 target, Vector3 up)
        {
            float3x3 rot = float3x3.LookRotation(math.normalize(target - eye), up);
            if (GraphicsDeviceTools.IsGLDevice())
            {
                rot.c2 *= -1;
            }

            return FastInverse(new float4x4(rot, eye));

            // simple version
            /*
            rot = math.transpose(rot);
            eye *= -1;

            var t0 = math.dot(rot.GetRow(0), eye);
            var t1 = math.dot(rot.GetRow(1), eye);
            var t2 = math.dot(rot.GetRow(2), eye);

            return math.float4x4(rot, new float3(t0, t1, t2));
            */
        }
        /// <summary>
        /// Row major matrix mulplication
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static float4x4 Mul(this float4x4 m, float4x4 n)
        {
            return new float4x4(
                math.mul(n, m.c0),
                math.mul(n, m.c1),
                math.mul(n, m.c2),
                math.mul(n, m.c3));
        }

        /// <summary>
        /// Show matrix in a good look string format
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string ToStringNice(this float4x4 m)
        {
            return string.Format("{0}f    {1}f    {2}f    {3}f \n{4}f    {5}f    {6}f    {7}f \n{8}f    {9}f    {10}f    {11}f\n {12}f    {13}f    {14}f    {15}f\n\n", m.c0.x, m.c1.x, m.c2.x, m.c3.x, m.c0.y, m.c1.y, m.c2.y, m.c3.y, m.c0.z, m.c1.z, m.c2.z, m.c3.z, m.c0.w, m.c1.w, m.c2.w, m.c3.w);
        }
        /// <summary>
        /// Show matrices in a good look string format
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToString(params float4x4[] list)
        {
            return string.Join("", list.Select(m => m.ToStringNice()));
        }
    }
}