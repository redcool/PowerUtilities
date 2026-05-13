using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class RendererListParamsEx
    {
        public static NativeArray<ShaderTagId> GetDefaultTagArr(this RendererListParams context)
        {
            return ScriptableRenderContextEx.DefaultTagArr;
        }

        public static NativeArray<RenderStateBlock> GetDefaultBlockArr(this RendererListParams context)
        {
            return ScriptableRenderContextEx.DefaultBlockArr;
        }

    }
}
