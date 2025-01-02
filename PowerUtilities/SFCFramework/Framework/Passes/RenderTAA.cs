//#define USE_RENDER_TEXTURE


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
#if UNITY_2021_1_OR_NEWER
    [Tooltip("Render TAA")]
#endif
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU + "/RenderTAA")]
    public class RenderTAA : SRPFeature
    {
        public enum SamplesMode
        {
            _3x3,
            _2x2,
        }
        //================= rt
        [Header("TAA RT Options")]
        public FilterMode filterMode = FilterMode.Bilinear;
        [Range(0.1f,1)]
        public float renderScale = 1.0f;

        //================== taa
        [Header("TAA Options")]
        [ListItemDraw("qualityLevel:,qualityLevel,samplesMode:,samplesMode", "100,100,50,100")]
        public List<RenderTAAQualitySetting> qualitySettings = new List<RenderTAAQualitySetting>
        {
            new RenderTAAQualitySetting{qualityLevel=0, samplesMode=SamplesMode._3x3},
            new RenderTAAQualitySetting{qualityLevel=1, samplesMode=SamplesMode._3x3},
            new RenderTAAQualitySetting{qualityLevel=2, samplesMode=SamplesMode._3x3},
            new RenderTAAQualitySetting{qualityLevel=3, samplesMode=SamplesMode._3x3},
            new RenderTAAQualitySetting{qualityLevel=4, samplesMode=SamplesMode._3x3},
            new RenderTAAQualitySetting{qualityLevel=5, samplesMode=SamplesMode._3x3},
            new RenderTAAQualitySetting{qualityLevel=6, samplesMode=SamplesMode._3x3},
        };

        [Range(0.001f, 1)]
        [Tooltip(" projection matrix jitter strength,")]
        public float jitterSpeed = 0.02f;

        [Range(1, 256)]
        [Tooltip("halton sequence length")]
        public int haltonLength = 256;

        //====================== mat
        [Header("TAA Mat")]
        [LoadAsset("Hidden_Unlit_TAA.mat")]
        public Material taaMat;
        [EditorGroup("MatProp", true)]
        [Tooltip("update mat params by script or use mat's params")]
        public bool isOverrideMatProps;

        [EditorGroup("MatProp")]
        [Range(0, 1)]
        [Tooltip("blend rate,")]
        public float temporalFade = 0.95f;

        [EditorGroup("MatProp")]
        [Range(0, 100)]
        [Tooltip("exp atten,default 100")]
        public float movementBlending = 100;

        [EditorGroup("MatProp")]
        [Range(0, 1)]
        [Tooltip("samples uv texel scale,more big more blur")]
        public float texelSizeScale;

        [EditorGroup("MatProp")]
        [Range(0,1)]
        [Tooltip("Sharpen strength")]
        public float sharpen = 0.1f;

        public override ScriptableRenderPass GetPass()
        {
            return new RenderTAAPass(this);
        }
    }

    [Serializable]
    public class RenderTAAQualitySetting
    {
        [EnumFlags(isFlags = false, type = typeof(QualitySettings), memberName = "names")]
        public int qualityLevel = 0;

        [Tooltip("use 3x3 or 2x2")]
        public RenderTAA.SamplesMode samplesMode;
    }

    public class RenderTAAPass : SRPPass<RenderTAA>
    {
        Matrix4x4 prevVP;
        int _TemporalAATexture = Shader.PropertyToID(nameof(_TemporalAATexture));

#if USE_RENDER_TEXTURE
        RenderTexture tempRT1, tempRT2;
#else
        int tempRT1 = Shader.PropertyToID(nameof(tempRT1));
        int tempRT2 = Shader.PropertyToID(nameof(tempRT2));
#endif
        public RenderTAAPass(RenderTAA feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return Feature.taaMat && base.CanExecute();
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.width = (int)(desc.width * Feature.renderScale);
            desc.height = (int)(desc.height * Feature.renderScale);

            desc.graphicsFormat = GraphicsFormatTools.GetColorTextureFormat();
            desc.depthBufferBits = 0;
#if USE_RENDER_TEXTURE
            RenderTextureTools.TryCreateRT(ref tempRT1, desc, nameof(tempRT1), Feature.filterMode);
            RenderTextureTools.TryCreateRT(ref tempRT2, desc, nameof(tempRT2), Feature.filterMode);
#else
            cmd.GetTemporaryRT(tempRT1, desc, Feature.filterMode);
            cmd.GetTemporaryRT(tempRT2, desc, Feature.filterMode);
#endif
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
#if USE_RENDER_TEXTURE
            if (tempRT1)
                tempRT1.Destroy();
            if (tempRT2)
                tempRT2.Destroy();
#endif
        }

        void UpdateKeywordByQualitySettings(CommandBuffer cmd)
        {
            var qLevel = QualitySettings.GetQualityLevel();

            var is3x3On = true;

            if (qLevel < Feature.qualitySettings.Count)
            {
                var setting = Feature.qualitySettings[qLevel];
                is3x3On = setting.samplesMode == RenderTAA.SamplesMode._3x3;
            }
            cmd.SetShaderKeywords(is3x3On, "_SAMPLES_3X3");
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            UpdateKeywordByQualitySettings(cmd);
            var cam = renderingData.cameraData.camera;

            var mat = Feature.taaMat;
            var matrix = cam.nonJitteredProjectionMatrix.inverse;
            mat.SetMatrix("_invP", matrix);

            matrix = prevVP * cam.cameraToWorldMatrix;
            mat.SetMatrix("_FrameMatrix", matrix);

            if (Feature.isOverrideMatProps)
            {
                mat.SetFloat("_TemporalFade", Feature.temporalFade);
                mat.SetFloat("_MovementBlending", Feature.movementBlending);
                mat.SetFloat("_TexelSizeScale", Feature.texelSizeScale);
                mat.SetFloat("_Sharpen", Feature.sharpen);
            }

            var isEven = (Time.frameCount) % 2 == 0;
            var writeTarget = isEven ? tempRT1 : tempRT2;
            var readTarget = isEven ? tempRT2 : tempRT1;

            cmd.SetGlobalTexture(_TemporalAATexture, readTarget);

            cmd.BlitTriangle(BuiltinRenderTextureType.CurrentActive, writeTarget, mat, 0,drawTriangleMesh:true);
            cmd.BlitTriangle(writeTarget, renderingData.cameraData.renderer.cameraColorTargetHandle, cmd.GetDefaultBlitTriangleMat(), 0);

            cmd.Execute(ref context);

            prevVP = cam.nonJitteredProjectionMatrix * cam.worldToCameraMatrix;

            //reset
            ResetCameraMatrix(cam);
            JitterCameraProjectionMatrix(cam);
        }

        void ResetCameraMatrix(Camera cam)
        {
            cam.ResetProjectionMatrix();
            cam.ResetWorldToCameraMatrix();
        }

        void JitterCameraProjectionMatrix(Camera cam)
        {
            cam.nonJitteredProjectionMatrix = cam.projectionMatrix;

            var p = cam.projectionMatrix;
            //float2 jitter = (float2)(2 * Halton2364Seq[Time.frameCount % Feature.haltonLength] * Feature.jitterSpeed);
            float2 jitter = NoiseTools.GetHalton2364(Feature.haltonLength) * Feature.jitterSpeed;
            p.m02 = jitter.x / Screen.width;
            p.m12 = jitter.y / Screen.height;
            cam.projectionMatrix = p;
        }
    }
}
