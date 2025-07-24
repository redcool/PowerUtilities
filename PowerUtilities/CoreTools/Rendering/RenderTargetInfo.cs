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
        public enum RTSizeMode
        {
            [Tooltip("set camera.Width * renderScale")]
            RenderScale,
            [Tooltip("set camera.Width directly")]
            Size
        }
        public enum RTFastFormatMode
        {
            ColorOnly, DepthOnly, ColorDepth
        }
        public enum RTSampleMode
        {
            x1=1,x2=2,x4=4,x8=8
        }
        // show as title
        [HideInInspector]
        public string title="";

        [Tooltip("rt name")]
        public string name;

        [Header("Format")]
        [Tooltip("color format,set none when create DepthTarget")]
        [EnumSearchable(typeof(GraphicsFormat))]
        public GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm;

        [Tooltip("depth target format,D24_UNorm_S8_UInt is preferred")]
        [EnumSearchable(typeof(GraphicsFormat))]
        public GraphicsFormat depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;

        [Header("Override Size")]
        public bool isOverrideSize;

        [Tooltip("set rt size use renderScale or size")]
        [EditorDisableGroup(targetPropName = "isOverrideSize")]
        public RTSizeMode rtSizeMode;

        [EditorDisableGroup(targetPropName = "isOverrideSize")]
        [Range(0, 1)] public float renderScale = 1;

        [EditorDisableGroup(targetPropName = "isOverrideSize")]
        public int width = 512;

        [EditorDisableGroup(targetPropName = "isOverrideSize")]
        public int height = 512;

        public FilterMode filterMode = FilterMode.Bilinear;
        public RTSampleMode sampleMode = RTSampleMode.x1;

        [Header("RenderTexture")]
        [Tooltip("create renderTexture when check,otherwist GetTemporaryRT")]
        public bool isCreateRenderTexture;
        public RenderTexture rt;

        [EditorHeader("","Auto ColorFormat Options")]
        [Tooltip("auto create a color texture format,hdr:(B10G11R11_UFloatPack32, R16G16B16A16_SFloat ,R32G32B32A32_SFloat)")]
        public bool isAutoGraphicsFormat;

        [Tooltip("hdr or ldr")]
        [EditorDisableGroup(targetPropName = "isAutoGraphicsFormat")]
        public bool isHdr;
        //[Tooltip("hdr rt need alpha channel?")]
        //public bool isHdrHasAlpha = true;

        [EditorHeader("","Auto NormalTexture Format")]
        [Tooltip("Get a suitable format form NormalTexture(Vector)")]
        public bool isNormalColorTexture;


        [Header("Other Options")]
        [Tooltip("skip this target")]
        public bool isSkip;

        [EditorHeader("", "--FastFormat Buttons--")]
        [EditorToolbar(texts = new[] { "ColorOnly", "DepthOnly", "ColorDepth" }
            , onClickCall = "FastSetupFormat"
            , tooltips = new []{"only colour buffer,no depth buffer","only depth buffer,no colour buffer","full fb,with color and depth buffer"}
            )]
        public bool isFastSetFormat;

        public int GetTextureId()
            => Shader.PropertyToID(name);

        public bool IsValid(Camera cam = null)
        {
            var isValid = !string.IsNullOrEmpty(name) && !isSkip;
#if UNITY_2022_1_OR_NEWER
            if (cam)
                isValid = isValid && !RTHandleTools.IsURPRTAlloced(cam, name);
#endif
            return isValid;
        }

        void UpdateTitle()
        {
            var skipState = isSkip ? "(Skip)" : "";
            title = $"{name}{skipState}";
        }
        /// <summary>
        /// Set suitable format and get
        /// </summary>
        /// <returns></returns>
        public GraphicsFormat GetFinalFormat()
        {
            if (isAutoGraphicsFormat)
            {
                format = GraphicsFormatTools.GetColorTextureFormat(isHdr, true);
            }
            if (isNormalColorTexture)
                format = GraphicsFormatTools.GetNormalTextureFormat();
            return format;
        }
        public GraphicsFormat GetFinalDepthFormat()
            => depthStencilFormat;

        public void CreateRT(CommandBuffer cmd, RenderTextureDescriptor desc, Camera cam)
        {
            UpdateTitle();
            if (!IsValid(cam))
            {
                RenderTextureTools.DestroyRT(name);
                return;
            }

            SetupDescFormat(ref desc);
            SetupDescSize(ref desc, cam);

            desc.sRGB = false;
            desc.msaaSamples = (int)sampleMode;
            desc.bindMS = sampleMode > RTSampleMode.x1;

            if (isCreateRenderTexture)
            {
                RenderTextureTools.CreateRT(ref rt, desc, name, filterMode);
            }
            else
            {
                RenderTextureTools.DestroyRT(name);

                cmd.GetTemporaryRT(GetTextureId(), desc, filterMode);
            }
        }


        void SetupDescSize(ref RenderTextureDescriptor desc, Camera camera)
        {
            if (!isOverrideSize)
                return;

            (int width, int height) size = rtSizeMode switch
            {
                RTSizeMode.Size => (width, height),
                _ => ((int)(camera.pixelWidth * renderScale), (int)(camera.pixelHeight * renderScale)),
            };

            desc.width = size.width;
            desc.height = size.height;
        }

        void FastSetupFormat(int modeId)
        {
            format = modeId == 1 ? GraphicsFormat.None : isAutoGraphicsFormat ? GraphicsFormatTools.GetColorTextureFormat(isHdr, true) : GraphicsFormat.R8G8B8A8_UNorm;
            depthStencilFormat = modeId == 0 ? GraphicsFormat.None : GraphicsFormat.D24_UNorm_S8_UInt;
        }

        void SetupDescFormat(ref RenderTextureDescriptor desc)
        {
            desc.graphicsFormat = GetFinalFormat();

            if (GraphicsFormatUtility.IsDepthFormat(depthStencilFormat))
            {
#if UNITY_2020
                desc.depthBufferBits = 24;
#else
                desc.depthStencilFormat = GetFinalDepthFormat();
#endif
            }
        }
    }
}
