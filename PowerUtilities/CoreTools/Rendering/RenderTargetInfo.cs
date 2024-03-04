namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public class RenderTargetInfo
    {
        [Tooltip("rt name")]
        public string name;

        [Tooltip("rt format")]
        public GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm;

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
    }
}
