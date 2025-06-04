using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_2020
    using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif

namespace PowerUtilities
{
    /// <summary>
    /// return urp's RenderTargetIdentifier if rt exists
    /// otherwist return Shader.PropertyToID 
    /// </summary>
    public static class ShaderPropertyIdentifier
    {
        public static RenderTargetIdentifier _CameraOpaqueTexture => ShaderPropertyIds.PropertyToID(nameof(_CameraOpaqueTexture));
        public static RenderTargetIdentifier _CameraDepthTexture => ShaderPropertyIds.PropertyToID(nameof(_CameraDepthTexture));
        public static RenderTargetIdentifier _CameraColorTexture => ShaderPropertyIds.PropertyToID(nameof(_CameraColorTexture));
        public static RenderTargetIdentifier _CameraColorAttachmentA => ShaderPropertyIds.PropertyToID(nameof(_CameraColorAttachmentA));
        public static RenderTargetIdentifier _CameraColorAttachmentB => ShaderPropertyIds.PropertyToID(nameof(_CameraColorAttachmentB));
        public static RenderTargetIdentifier _CameraDepthAttachment => ShaderPropertyIds.PropertyToID(nameof(_CameraDepthAttachment));
        public static RenderTargetIdentifier _CameraNormalsTexture => ShaderPropertyIds.PropertyToID(nameof(_CameraNormalsTexture));
        public static RenderTargetIdentifier _MotionVectorTexture => ShaderPropertyIds.PropertyToID(nameof(_MotionVectorTexture));
        public static RenderTargetIdentifier _AfterPostProcessTexture => ShaderPropertyIds.PropertyToID(nameof(_AfterPostProcessTexture));
        public static RenderTargetIdentifier _GBuffer0 => ShaderPropertyIds.PropertyToID(nameof(_GBuffer0));
        public static RenderTargetIdentifier _GBuffer1 => ShaderPropertyIds.PropertyToID(nameof(_GBuffer1));
        public static RenderTargetIdentifier _GBuffer2 => ShaderPropertyIds.PropertyToID(nameof(_GBuffer2));
        public static RenderTargetIdentifier _GBuffer3 => ShaderPropertyIds.PropertyToID(nameof(_GBuffer3));
    }

    public static class ShaderPropertyIds
    {
        /// <summary>
        /// if name is urp builtin rtName,use urp rtName(unity2022+),
        /// otherwise return Shader.PropertyToID(below unity2022)
        /// </summary>
        /// <param name="name">empty or CameraTarget return BuiltinRenderTextureType.CameraTarget</param>
        /// <returns></returns>
        public static RenderTargetIdentifier PropertyToID(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "CameraTarget")
                return BuiltinRenderTextureType.CameraTarget;

            var id = Shader.PropertyToID(name);
#if UNITY_2022_3_OR_NEWER
            var renderer = UniversalRenderPipeline.asset.GetDefaultRenderer();
            RenderTargetIdentifier rtId = id;
            renderer.FindTarget(name, ref rtId);

#endif
            return id;
        }

        /// <summary>
        /// urp built texture ids,
        /// before get need call UniversalRenderer.SetupURPShaderPropertyIds
        /// </summary>
        public static int
            _MainTex = Shader.PropertyToID(nameof(_MainTex)),
            _CameraOpaqueTexture = Shader.PropertyToID(nameof(_CameraOpaqueTexture)),
            _CameraDepthTexture = Shader.PropertyToID(nameof(_CameraDepthTexture)),
            _CameraColorTexture = Shader.PropertyToID(nameof(_CameraColorTexture)),
            _CameraColorAttachmentA = Shader.PropertyToID(nameof(_CameraColorAttachmentA)),
            _CameraColorAttachmentB = Shader.PropertyToID(nameof(_CameraColorAttachmentB)),
            _CameraDepthAttachment = Shader.PropertyToID(nameof(_CameraDepthAttachment)),
            _CameraNormalsTexture = Shader.PropertyToID(nameof(_CameraNormalsTexture)),
            _MotionVectorTexture = Shader.PropertyToID(nameof(_MotionVectorTexture)),
            _AfterPostProcessTexture = Shader.PropertyToID(nameof(_AfterPostProcessTexture)),
            _GBuffer0 = Shader.PropertyToID(nameof(_GBuffer0)),
            _GBuffer1 = Shader.PropertyToID(nameof(_GBuffer1)),
            _GBuffer2 = Shader.PropertyToID(nameof(_GBuffer2)),
            _GBuffer3 = Shader.PropertyToID(nameof(_GBuffer3)),
            _MainLightShadowmapTexture = Shader.PropertyToID(nameof(_MainLightShadowmapTexture)),
            _AdditionalLightsShadowmapTexture = Shader.PropertyToID(nameof(_AdditionalLightsShadowmapTexture))
            ;
        /// <summary>
        /// keywords
        /// </summary>
        public const string _DEBUG = nameof(_DEBUG),
            _CUSTOM_DEPTH_TEXTURE = nameof(_CUSTOM_DEPTH_TEXTURE),
            _LINEAR_TO_SRGB_CONVERSION = nameof(_LINEAR_TO_SRGB_CONVERSION),
            _SRGB_TO_LINEAR_CONVERSION = nameof(_SRGB_TO_LINEAR_CONVERSION),
            _OUTPUT_DEPTH = nameof(_OUTPUT_DEPTH)
            ;

        /// <summary>
        /// urp ShaderPropertyId (UniversalRenderPipelineCore.cs)
        /// </summary>
        public static readonly int

            glossyEnvironmentColor = Shader.PropertyToID("_GlossyEnvironmentColor"),
            subtractiveShadowColor = Shader.PropertyToID("_SubtractiveShadowColor"),

            glossyEnvironmentCubeMap = Shader.PropertyToID("_GlossyEnvironmentCubeMap"),
            glossyEnvironmentCubeMapHDR = Shader.PropertyToID("_GlossyEnvironmentCubeMap_HDR"),

            ambientSkyColor = Shader.PropertyToID("unity_AmbientSky"),
            ambientEquatorColor = Shader.PropertyToID("unity_AmbientEquator"),
            ambientGroundColor = Shader.PropertyToID("unity_AmbientGround"),

            time = Shader.PropertyToID("_Time"),
            sinTime = Shader.PropertyToID("_SinTime"),
            cosTime = Shader.PropertyToID("_CosTime"),
            deltaTime = Shader.PropertyToID("unity_DeltaTime"),
            timeParameters = Shader.PropertyToID("_TimeParameters"),

            scaledScreenParams = Shader.PropertyToID("_ScaledScreenParams"),
            worldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos"),
            screenParams = Shader.PropertyToID("_ScreenParams"),
            projectionParams = Shader.PropertyToID("_ProjectionParams"),
            zBufferParams = Shader.PropertyToID("_ZBufferParams"),
            orthoParams = Shader.PropertyToID("unity_OrthoParams"),
            globalMipBias = Shader.PropertyToID("_GlobalMipBias"),

            screenSize = Shader.PropertyToID("_ScreenSize"),

            viewMatrix = Shader.PropertyToID("unity_MatrixV"),
            projectionMatrix = Shader.PropertyToID("glstate_matrix_projection"),
            viewAndProjectionMatrix = Shader.PropertyToID("unity_MatrixVP"),

            inverseViewMatrix = Shader.PropertyToID("unity_MatrixInvV"),
            inverseProjectionMatrix = Shader.PropertyToID("unity_MatrixInvP"),
            inverseViewAndProjectionMatrix = Shader.PropertyToID("unity_MatrixInvVP"),

            cameraProjectionMatrix = Shader.PropertyToID("unity_CameraProjection"),
            inverseCameraProjectionMatrix = Shader.PropertyToID("unity_CameraInvProjection"),
            worldToCameraMatrix = Shader.PropertyToID("unity_WorldToCamera"),
            cameraToWorldMatrix = Shader.PropertyToID("unity_CameraToWorld"),

            cameraWorldClipPlanes = Shader.PropertyToID("unity_CameraWorldClipPlanes"),

            billboardNormal = Shader.PropertyToID("unity_BillboardNormal"),
            billboardTangent = Shader.PropertyToID("unity_BillboardTangent"),
            billboardCameraParams = Shader.PropertyToID("unity_BillboardCameraParams"),

            sourceTex = Shader.PropertyToID("_SourceTex"),
            sourceTex2 = Shader.PropertyToID("_SourceTex2"),
            sourceTex3 = Shader.PropertyToID("_SourceTex3"),
            sourceTex4 = Shader.PropertyToID("_SourceTex4"),
            scaleBias = Shader.PropertyToID("_ScaleBias"),
            scaleBiasRt = Shader.PropertyToID("_ScaleBiasRt"),

            _SrcMode = Shader.PropertyToID(nameof(_SrcMode)),
            _DstMode = Shader.PropertyToID(nameof(_DstMode)),
            _SrcAlphaMode = Shader.PropertyToID(nameof(_SrcAlphaMode)),
            _DstAlphaMode = Shader.PropertyToID(nameof(_DstAlphaMode)),

            // Required for 2D Unlit Shadergraph master node as it doesn't currently support hidden properties.
            rendererColor = Shader.PropertyToID("_RendererColor"),

            // custom vars
            shadows_ShadowMaskOn = Shader.PropertyToID("_Shadows_ShadowMaskOn"),

            //
            _ScalingSetupTexture = Shader.PropertyToID(nameof(_ScalingSetupTexture)),
            _UpscaledTexture = Shader.PropertyToID(nameof(_UpscaledTexture)),

            _ShadowBias = Shader.PropertyToID(nameof(_ShadowBias)),
            _LightDirection = Shader.PropertyToID(nameof(_LightDirection)),
            _PrevViewProjMatrix = Shader.PropertyToID(nameof(_PrevViewProjMatrix)),
            _ViewProjMatrix = Shader.PropertyToID(nameof(_ViewProjMatrix)),
            _GUIZTestMode = Shader.PropertyToID(nameof(_GUIZTestMode)),

            _MainLightPosition = Shader.PropertyToID(nameof(_MainLightPosition)),
            _MainLightColor = Shader.PropertyToID(nameof(_MainLightColor)),

            _FinalSrcMode = Shader.PropertyToID(nameof(_FinalSrcMode)),
            _FinalDstMode = Shader.PropertyToID(nameof(_FinalDstMode)),
            _LightAttenuation = Shader.PropertyToID(nameof(_LightAttenuation)),
            _ZTestMode = Shader.PropertyToID(nameof(_ZTestMode)),
            _ZWriteMode = Shader.PropertyToID(nameof(_ZWriteMode)),
            _SpotLightScale = Shader.PropertyToID(nameof(_SpotLightScale)),
            _SpotLightBias = Shader.PropertyToID(nameof(_SpotLightBias)),
            _SpotLightGuard = Shader.PropertyToID(nameof(_SpotLightGuard)),
            _SpotLightAngle = Shader.PropertyToID(nameof(_SpotLightAngle)),
            _LightRadiusIntensityFalloff = Shader.PropertyToID(nameof(_LightRadiusIntensityFalloff)),
            _CullMode = Shader.PropertyToID(nameof(_CullMode))
            ;
    }

}