namespace PowerUtilities
{
    using UnityEngine;

    public static class MathTools
    {
        public static float Frac(float v)
        {
            var n = Mathf.Abs(v);
            return n - Mathf.Floor(n);
        }
    }
}