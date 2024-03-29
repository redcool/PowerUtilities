﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class CameraEx
    {
        public static bool IsMainCamera(this Camera c) => c.CompareTag("MainCamera");
        public static bool IsReflectionCamera(this Camera c) => c.cameraType == CameraType.Reflection;
        public static bool IsGameCamera(this Camera c) => c.cameraType  == CameraType.Game;
        public static bool IsPreviewCamera(this Camera c) => c.cameraType == CameraType.Preview;
        public static bool IsSceneViewCamera(this Camera c) => c.cameraType == CameraType.SceneView;

        public static bool IsSceneViewPreviewCamera(this Camera c) => IsSceneViewCamera(c) && c.targetTexture;

        public static RenderTargetIdentifier GetCameraTarget(this Camera c) => c && c.targetTexture ? (RenderTargetIdentifier)c.targetTexture : BuiltinRenderTextureType.CameraTarget;
    }
}
