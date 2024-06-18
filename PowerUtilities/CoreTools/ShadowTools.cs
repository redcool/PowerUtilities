using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PowerUtilities
{
    public static class ShadowTools
    {


        public static Matrix4x4 ConvertToShadowAtlasMatrix(Matrix4x4 m, Vector2 offset, float rowCount)
        {
            if (SystemInfo.usesReversedZBuffer)
            {
                m.SetRow(2, m.GetRow(2) * -1);
            }

            float scale = 1f / rowCount;

            /* 
            var splitRatio = 1f / rowCount;
            var objectMat = MatrixEx.Matrix(
                0.5f * splitRatio, 0, 0, 0.5f * splitRatio + splitRatio * offset.x,
                0, 0.5f * splitRatio, 0, 0.5f * splitRatio + splitRatio * offset.y,
                0, 0, 0.5f, 0.5f,
                0, 0, 0, 1
                );
            return objectMat * m;
            */

            var r0 = m.GetRow(0);
            var r1 = m.GetRow(1);
            var r2 = m.GetRow(2);
            var r3 = m.GetRow(3);

            OffsetScale(ref r0, r3, offset.x, scale);
            OffsetScale(ref r1, r3, offset.y, scale);
            OffsetScale(ref r2, r3, 0, 1);

            m.SetRow(0, r0);
            m.SetRow(1, r1);
            m.SetRow(2, r2);

            return m;

            void OffsetScale(ref Vector4 v, Vector4 r3, float offset, float scale)
            {
                //v.x = (0.5f *(v.x + r3.x) + offset * r3.x) * scale;
                for (int i = 0; i < 4; i++)
                {
                    v[i] = (0.5f * (v[i] + r3[i]) + offset * r3[i]) * scale;
                }
            }
        }
    }
}
