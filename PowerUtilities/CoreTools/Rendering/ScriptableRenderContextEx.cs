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
        public static ShaderTagId[] defaultTags = new ShaderTagId[] { default };
        public static RenderStateBlock[] defaultBlocks = new[] { new RenderStateBlock(RenderStateMask.Nothing) };

        static NativeArray<ShaderTagId> defaultTagArr;
        static NativeArray<RenderStateBlock> defaultBlockArr;


        static ScriptableRenderContextEx()
        {
        }

        /// <summary>
        /// use cmd.DrawRendererList(unity 2023) or context.DrawRenderers(unity 2021)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cmd"></param>
        /// <param name="cullingResults"></param>
        /// <param name="drawingSettings"></param>
        /// <param name="filteringSettings"></param>
        public static void DrawRenderers(this ScriptableRenderContext context, CommandBuffer cmd, CullingResults cullingResults,
            ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings,
            NativeArray<ShaderTagId>? tagValues = null,
            NativeArray<RenderStateBlock>? stateBlocks = null)
        {
            NativeArrayTools.CreateIfNull(ref defaultTagArr, 1, Allocator.Persistent);
            NativeArrayTools.CreateIfNull(ref defaultBlockArr, 1, Allocator.Persistent);
#if UNITY_2022_1_OR_NEWER
            if (!tagValues.HasValue && stateBlocks.HasValue)
                tagValues = defaultTagArr;

            var param = new RendererListParams(cullingResults, drawingSettings, filteringSettings);
            param.stateBlocks = stateBlocks;
            param.tagValues = tagValues;
            var list = context.CreateRendererList(ref param);
            cmd.DrawRendererList(list);
            cmd.Execute(ref context);
#else // below 2021
            // dummy
            if (!tagValues.HasValue)
                tagValues = defaultTagArr;// new NativeArray<ShaderTagId>(defaultTags, Allocator.Temp);
            if (!stateBlocks.HasValue)
                stateBlocks = defaultBlockArr;// new NativeArray<RenderStateBlock>(defaultBlocks, Allocator.Temp);

            //context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings, tagValues??default, stateBlocks ?? default);
#endif
        }
    }
}
