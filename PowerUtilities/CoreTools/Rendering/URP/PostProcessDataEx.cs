using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{

    public static class PostProcessDataEx
    {
        public static string GetPostProcessDataPath() 
            => Path.Combine(UniversalRenderPipelineAssetEx.PACKAGE_PATH, "Runtime/Data/PostProcessData.asset");

        public static string GetUberPostShaderPath()
            => Path.Combine(UniversalRenderPipelineAssetEx.PACKAGE_PATH, "Shaders/PostProcessing/UberPost.shader");
#if UNITY_EDITOR
        public static PostProcessData GetDefaultPostProcessData()
        {
            return AssetDatabase.LoadAssetAtPath<PostProcessData>(GetPostProcessDataPath());
        }
#endif
    }
}
