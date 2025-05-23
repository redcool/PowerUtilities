﻿using System;
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
    /// <summary>
    /// Reference:
    /// 
    /// https://zhuanlan.zhihu.com/p/142922246
    /// 
    /// https://github.com/CMDRSpirit/URPTemporalAA
    /// </summary>
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
        [Range(1, 2)]
        [Tooltip("samples uv texel scale,more big more blur")]
        public float texelSizeScale = 1;

        [EditorGroup("MatProp")]
        [Range(0,1)]
        [Tooltip("Sharpen strength")]
        public float sharpen = 0.1f;

        public override ScriptableRenderPass GetPass()
        {
            // default settings
            if(renderPassEvent < RenderPassEvent.BeforeRenderingPostProcessing)
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            cameraType = CameraType.Game;

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
        int _TemporalAATexture = Shader.PropertyToID(nameof(_TemporalAATexture));

        //Matrix4x4 prevVP;
        Matrix4x4 prevP;

        RenderTexture tempRT1, tempRT2;
        int lastQualityLevel;

        public RenderTAAPass(RenderTAA feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return Feature.taaMat && base.CanExecute();
        }

        private void TrySetupTempRTs(CommandBuffer cmd, RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.width = (int)(desc.width * Feature.renderScale);
            desc.height = (int)(desc.height * Feature.renderScale);

            desc.graphicsFormat = GraphicsFormatTools.GetColorTextureFormat();
            desc.depthBufferBits = 0;

            RenderTextureTools.TryCreateRT(ref tempRT1, desc, nameof(tempRT1), Feature.filterMode);
            RenderTextureTools.TryCreateRT(ref tempRT2, desc, nameof(tempRT2), Feature.filterMode);

        }

        public override void OnDestroy()
        {
            TryDestroyRTs();
        }

        private void TryDestroyRTs()
        {
            if (tempRT1)
                tempRT1.Destroy();
            if (tempRT2)
                tempRT2.Destroy();
        }

        /// <summary>
        /// when qualityLevel changed will check 
        /// </summary>
        /// <param name="cmd"></param>
        void UpdateKeywordQualityLevelChanged(CommandBuffer cmd)
        {
            var qLevel = QualitySettings.GetQualityLevel();
            if (!CompareTools.CompareAndSet(ref lastQualityLevel, ref qLevel))
                return;

            var setting = Feature.qualitySettings.Find(item => item.qualityLevel == qLevel);
            if (setting == null)
                return;

            var is3x3On = setting.samplesMode == RenderTAA.SamplesMode._3x3;
            cmd.SetShaderKeywords(is3x3On, "_SAMPLES_3X3");
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            UpdateKeywordQualityLevelChanged(cmd);

            TrySetupTempRTs(cmd, renderingData);

            var cam = renderingData.cameraData.camera;

            var mat = Feature.taaMat;
            var matrix = cam.nonJitteredProjectionMatrix.inverse;
            mat.SetMatrix("_invP", matrix);

            //matrix = prevVP * cam.cameraToWorldMatrix;
            mat.SetMatrix("_FrameMatrix", prevP);

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
            //mat.SetTexture(_TemporalAATexture, readTarget);
            cmd.BlitTriangle(BuiltinRenderTextureType.CurrentActive, writeTarget, mat, 0);
            cmd.BlitTriangle(writeTarget, renderingData.cameraData.renderer.cameraColorTargetHandle, cmd.GetDefaultBlitTriangleMat(), 0);

            cmd.Execute(ref context);

            //prevVP = cam.nonJitteredProjectionMatrix * cam.worldToCameraMatrix;
            prevP = cam.nonJitteredProjectionMatrix;
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
