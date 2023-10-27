using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// handle compatibility
    /// </summary>
    public static class ScriptableRenderContextEx
    {
        public static ShaderTagId[] defaultTag = new ShaderTagId[] { default };

        static ScriptableRenderContextEx()
        {
            var tags = new ShaderTagId[] { default };
        }

        /// <summary>
        /// use cmd.DrawRendererList(unity 2023) or context.DrawRenderers(unity 2021)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cmd"></param>
        /// <param name="cullingResults"></param>
        /// <param name="drawingSettings"></param>
        /// <param name="filteringSettings"></param>
        public static void DrawRenderers(this ScriptableRenderContext context, CommandBuffer cmd,CullingResults cullingResults, 
            ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings,
            NativeArray<ShaderTagId>? tagValues = null,
            NativeArray<RenderStateBlock>? stateBlocks = null)
        {
            var tagNA = new NativeArray<ShaderTagId>(defaultTag, Allocator.Temp);

            if(!tagValues.HasValue)
                tagValues = tagNA;

#if UNITY_2023_1_OR_NEWER
            var param = new RendererListParams(cullingResults, drawingSettings, filteringSettings);
            param.stateBlocks = stateBlocks;
            param.tagValues = tagValues;
            var list = context.CreateRendererList(ref param);
            cmd.DrawRendererList(list);
#else // below 2021
            if (!stateBlocks.HasValue)
                stateBlocks = new NativeArray<RenderStateBlock>(new[] { default(RenderStateBlock) }, Allocator.Temp);

            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings, tagValues.Value, stateBlocks??default);
#endif
        }
    }
}
