namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public class RenderTargetInfo
    {
        [Tooltip("rt name")]
        public string name;

        [Tooltip("rt format")]
        [EnumSearchable(typeof(GraphicsFormat))]
        public GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm;

        [Tooltip("create renderTexture when check,otherwist GetTemporaryRT")]
        public bool isCreateRenderTexture;
        public RenderTexture rt;

        [Tooltip("setup depth buffer")]
        public bool hasDepthBuffer;

        [Tooltip("auto select a normal color texture format")]
        public bool isNormalColorTexture;

        [Tooltip("skip this target")]
        public bool isSkip;

        public int GetTextureId()
            => Shader.PropertyToID(name);

        public bool IsValid(Camera cam = null)
        {
            var isValid = !string.IsNullOrEmpty(name) && format != default && !isSkip;
#if UNITY_2022_1_OR_NEWER
            if (cam)
                isValid = isValid && !RTHandleTools.IsURPRTAlloced(cam, name);
#endif
            return isValid;
        }

        /// <summary>
        /// get final graphics format
        /// </summary>
        /// <returns></returns>
        public GraphicsFormat GetFinalFormat()
        => isNormalColorTexture || ! RenderingUtils.SupportsGraphicsFormat(format,FormatUsage.Linear|FormatUsage.Render) 
            ? RenderingTools.GetNormalTextureFormat() : format;

        public void CreateRT(CommandBuffer cmd, RenderTextureDescriptor desc,Camera cam )
        {
            if (! IsValid(cam))
                return;

            desc.graphicsFormat = GetFinalFormat();
            //desc.colorFormat = info.isHdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;

            // depth format
            if (GraphicsFormatUtility.IsDepthFormat(desc.graphicsFormat))
            {
                desc.colorFormat = RenderTextureFormat.Depth;
                hasDepthBuffer = true;
            }

            // sync info's format 
            if (format != desc.graphicsFormat)
            {
                format = desc.graphicsFormat;
            }

            if (hasDepthBuffer)
            {
#if UNITY_2020
                desc.depthBufferBits = 24;
#else
                desc.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
#endif
            }
            if(isCreateRenderTexture && rt.IsNeedRealloc(desc.width,desc.height))
            {
                if (rt)
                    rt.Release();

                rt = new RenderTexture(desc);
                rt.Create();
            }
            else
            {
                cmd.GetTemporaryRT(GetTextureId(), desc);
            }
        }
    }
}
