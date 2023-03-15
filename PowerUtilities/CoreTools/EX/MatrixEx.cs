using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class MatrixEx
    {
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

    }
}
