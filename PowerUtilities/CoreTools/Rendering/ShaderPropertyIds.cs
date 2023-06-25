using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public static class ShaderPropertyIds
    {
        public static readonly int
            _MainTex = Shader.PropertyToID("_MainTex"),
            _CameraOpaqueTexture = Shader.PropertyToID(nameof(_CameraOpaqueTexture)),
            _CameraDepthTexture = Shader.PropertyToID(nameof(_CameraDepthTexture)),
            _CameraColorAttachmentA = Shader.PropertyToID(nameof(_CameraColorAttachmentA)),
            _CameraColorAttachmentB = Shader.PropertyToID(nameof(_CameraColorAttachmentB)),
            _CameraDepthAttachment = Shader.PropertyToID(nameof(_CameraDepthAttachment)),
            _CameraNormalsTexture = Shader.PropertyToID(nameof(_CameraNormalsTexture)),
            _MotionVectorTexture = Shader.PropertyToID(nameof(_MotionVectorTexture)),
            _GBuffer0 = Shader.PropertyToID(nameof(_GBuffer0)),
            _GBuffer1 = Shader.PropertyToID(nameof(_GBuffer1)),
            _GBuffer2 = Shader.PropertyToID(nameof(_GBuffer2)),
            _GBuffer3 = Shader.PropertyToID(nameof(_GBuffer3))
            ;

        public const string _DEBUG = nameof(_DEBUG),
            _CUSTOM_DEPTH_TEXTURE = nameof(_CUSTOM_DEPTH_TEXTURE)
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
            scaleBias = Shader.PropertyToID("_ScaleBias"),
            scaleBiasRt = Shader.PropertyToID("_ScaleBiasRt"),

            // Required for 2D Unlit Shadergraph master node as it doesn't currently support hidden properties.
            rendererColor = Shader.PropertyToID("_RendererColor")
            ;
    }

}