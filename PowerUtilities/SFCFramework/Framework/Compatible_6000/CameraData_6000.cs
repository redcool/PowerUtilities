#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    public static class CameraData_6000
    {
        public static bool IsCameraProjectionMatrixFlipped(this CameraData data)
        {
            var colorTarget = data.renderer.CameraColorTargetHandle();
            return data.renderer.GetCurrentPassContextContainer().Get<UniversalCameraData>().IsRenderTargetProjectionMatrixFlipped(colorTarget);
        }
    }
}
#endif