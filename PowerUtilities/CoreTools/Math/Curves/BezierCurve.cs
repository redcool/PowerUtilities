using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// https://pomax.github.io/bezierinfo/zh-CN/index.html#control
    /// 
    /// </summary>
    public static class BezierCurve
    {
        /// <summary>
        /// bezier square
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            var mt = 1 - t;
            var mt2 = mt * mt;
            var t2 = t * t;

            var f0 = mt2;
            var f1 = 2 * t * mt;
            var f2 = t2;

            return f0 * p0 + f1 * p1 + f2 * p2;
        }
        /// <summary>
        /// rational bezier square
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="t"></param>
        /// <param name="w0"></param>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <returns></returns>
        public static Vector3 Bezier(Vector3 p0,Vector3 p1,Vector3 p2,float t,float w0=1,float w1=1,float w2=1)
        {
            var mt = 1 - t;
            var mt2 = mt * mt;
            var t2 = t * t;

            var f0 = w0 * mt2;
            var f1 = w1 * 2 * t * mt;
            var f2 = w2* t2;
            
            var weights = f0 + f1 + f2;
            var result = f0 * p0 + f1 *  p1 + f2 * p2;
            return result/weights;
        }
        /// <summary>
        /// bezier cubic
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var mt = 1 - t;
            var mt2 = mt * mt;
            var mt3 = mt2 * mt;
            var t2 = t * t;
            var t3 = t2 * t;
            return mt3 * p0 + 3 * mt2 * t * p1 + 3 *  mt * t2 * p2 + t3 * p3;
        }
        /// <summary>
        /// rational bezier cubic
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <param name="w0"></param>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <param name="w3"></param>
        /// <returns></returns>
        public static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float w0 = 1, float w1 = 1, float w2 = 1, float w3 = 1)
        {
            var mt = 1 - t;
            var mt2 = mt * mt;
            var mt3 = mt2 * mt;
            var t2 = t * t;
            var t3 = t2 * t;

            var f0 = w0 * mt3;
            var f1 = w1 * 3 * mt2 * t;
            var f2 = w2 * 3 * mt * t2;
            var f3 = w3 * t3;

            var weights = f0 + f1 + f2 + f3;
            var result = f0 * p0 + f1 * p1 + f2 * p2 + f3 * p3;
            return result / weights;
        }
        /// <summary>
        /// rational bezier cubic(closed p0)
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="t"></param>
        /// <param name="w0"></param>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <param name="w3"></param>
        /// <returns></returns>
        public static Vector3 BezierClosed(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float w0 = 1, float w1 = 1, float w2 = 1, float w3 = 1)
        {
            var pos0 = Bezier(p0, p1, p2, p3, t, w0, w1, w2, w3);
            var pos1 = Bezier(p1, p2, p3,p0, t,  w1, w2, w3,w0);
            return Vector3.Lerp(pos0, pos1, t);
        }
    }
}
