using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class CameraEx
    {
        public static bool IsReflectionCamera(this Camera c) => c.cameraType == CameraType.Reflection;
        public static bool IsMainCamera(this Camera c) => c.CompareTag("MainCamera");

    }
}
