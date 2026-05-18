#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace PowerUtilities
{
    public static partial class RTHandleTools
    {
        public static ImportResourceParams DefaultImportParams = new ImportResourceParams()
        {
            clearColor = Color.clear,
            clearOnFirstUse = false,
            discardOnLastUse = false,
            textureUVOrigin = TextureUVOrigin.BottomLeft
        };

        public static TextureHandle GetTextureHandle(this RTHandle rt, RenderGraph renderGraph, ImportResourceParams importParams=default)
        {
            return renderGraph.ImportTexture(rt, importParams);
        }
    }
}
#endif // UNITY_6000_4_OR_NEWER