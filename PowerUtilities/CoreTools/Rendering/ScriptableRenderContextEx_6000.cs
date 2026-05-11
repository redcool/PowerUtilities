#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace PowerUtilities
{
    /// <summary>
    /// handle compatibility
    /// </summary>
    public static class ScriptableRenderContextEx_6000
    {
        static RasterGraphContext rasterContext;
        static ComputeGraphContext computeContext;
        static UnsafeGraphContext unsafeContext;

        public static void SetRasterContext(this ScriptableRenderContext c, RasterGraphContext context)
        {
            rasterContext = context;
        }
        public static void SetComputeContext(this ScriptableRenderContext c, ComputeGraphContext context)
        {
            computeContext = context;
        }
        public static void SetUnsafeContext(this ScriptableRenderContext c, UnsafeGraphContext context)
        {
            unsafeContext = context;
        }
        public static RasterGraphContext GetRasterContext(this ScriptableRenderContext c) => rasterContext;
        public static ComputeGraphContext GetComputeContext(this ScriptableRenderContext c) => computeContext;
        public static UnsafeGraphContext GetUnsaftContext(this ScriptableRenderContext c) => unsafeContext;
    }
}
#endif