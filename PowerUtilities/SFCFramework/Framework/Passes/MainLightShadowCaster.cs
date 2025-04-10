﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using URPMainLightShadowPass = UnityEngine.Rendering.Universal.Internal.MainLightShadowCasterPass;

namespace PowerUtilities.RenderFeatures
{
    public class MainLightShadowCaster : SRPFeature
    {
        [Header("Shadow Light ")]

        [Tooltip("half of y distance ortho(box ")]
        public float orthoSize = 20;

        [Tooltip("near plane")]
        public float near = 0.1f;

        [Tooltip("far plane ")]
        public float far = 1000;

        [Tooltip("start postion(camera pos)'s offset")]
        public Vector3 camPosOffset;

        [EditorHeader("", "--- Distance")]
        public bool isOverrideShadowDistance;
        [Tooltip("urp shadow max distance")]
        public float shadowDistance = 100;

        [Tooltip("left use cam position , right use light position ")]
        [Range(0,1)] public float camLightPosRate;

        public override ScriptableRenderPass GetPass()
        {
            return new MainLightShadowCasterPass(this);
        }
    }

    public class MainLightShadowCasterPass : SRPPass<MainLightShadowCaster>
    {
        public MainLightShadowCasterPass(MainLightShadowCaster feature) : base(feature)
        {
            Init();
        }

        private static class MainLightShadowConstantBuffer
        {
            public static int _WorldToShadow;
            public static int _ShadowParams;
            public static int _CascadeShadowSplitSpheres0;
            public static int _CascadeShadowSplitSpheres1;
            public static int _CascadeShadowSplitSpheres2;
            public static int _CascadeShadowSplitSpheres3;
            public static int _CascadeShadowSplitSphereRadii;
            public static int _ShadowOffset0;
            public static int _ShadowOffset1;
            public static int _ShadowmapSize;
        }

        const int k_MaxCascades = 4;
        const int k_ShadowmapBufferBits = 16;
        float m_CascadeBorder;
        float m_MaxShadowDistanceSq;
        int m_ShadowCasterCascadesCount;

        int m_MainLightShadowmapID;
        public RTHandle m_MainLightShadowmapTexture;
        private RTHandle m_EmptyLightShadowmapTexture;
        private const int k_EmptyShadowMapDimensions = 1;
        private const string k_EmptyShadowMapName = "_EmptyLightShadowmapTexture";
        private static readonly Vector4 s_EmptyShadowParams = new Vector4(1, 0, 1, 0);
        private static readonly Vector4 s_EmptyShadowmapSize = s_EmptyShadowmapSize = new Vector4(k_EmptyShadowMapDimensions, 1f / k_EmptyShadowMapDimensions, k_EmptyShadowMapDimensions, k_EmptyShadowMapDimensions);

        Matrix4x4[] m_MainLightShadowMatrices;
        ShadowSliceData[] m_CascadeSlices;
        Vector4[] m_CascadeSplitDistances;

        bool m_CreateEmptyShadowmap;

        int renderTargetWidth;
        int renderTargetHeight;

        ProfilingSampler m_ProfilingSetupSampler = new ProfilingSampler("Setup Main Shadowmap");

        /// <summary>
        /// Creates a new <c>MainLightShadowCasterPass</c> instance.
        /// </summary>
        /// <param name="evt">The <c>RenderPassEvent</c> to use.</param>
        /// <seealso cref="RenderPassEvent"/>
        public void Init()
        {
            base.profilingSampler = new ProfilingSampler(nameof(MainLightShadowCasterPass));

            m_MainLightShadowMatrices = new Matrix4x4[k_MaxCascades + 1];
            m_CascadeSlices = new ShadowSliceData[k_MaxCascades];
            m_CascadeSplitDistances = new Vector4[k_MaxCascades];

            MainLightShadowConstantBuffer._WorldToShadow = Shader.PropertyToID("_MainLightWorldToShadow");
            MainLightShadowConstantBuffer._ShadowParams = Shader.PropertyToID("_MainLightShadowParams");
            MainLightShadowConstantBuffer._CascadeShadowSplitSpheres0 = Shader.PropertyToID("_CascadeShadowSplitSpheres0");
            MainLightShadowConstantBuffer._CascadeShadowSplitSpheres1 = Shader.PropertyToID("_CascadeShadowSplitSpheres1");
            MainLightShadowConstantBuffer._CascadeShadowSplitSpheres2 = Shader.PropertyToID("_CascadeShadowSplitSpheres2");
            MainLightShadowConstantBuffer._CascadeShadowSplitSpheres3 = Shader.PropertyToID("_CascadeShadowSplitSpheres3");
            MainLightShadowConstantBuffer._CascadeShadowSplitSphereRadii = Shader.PropertyToID("_CascadeShadowSplitSphereRadii");
            MainLightShadowConstantBuffer._ShadowOffset0 = Shader.PropertyToID("_MainLightShadowOffset0");
            MainLightShadowConstantBuffer._ShadowOffset1 = Shader.PropertyToID("_MainLightShadowOffset1");
            MainLightShadowConstantBuffer._ShadowmapSize = Shader.PropertyToID("_MainLightShadowmapSize");

            m_MainLightShadowmapID = Shader.PropertyToID("_MainLightShadowmapTexture");

            m_EmptyLightShadowmapTexture = ShadowUtils.AllocShadowRT(k_EmptyShadowMapDimensions, k_EmptyShadowMapDimensions, k_ShadowmapBufferBits, 1, 0, name: k_EmptyShadowMapName);
        }

        /// <summary>
        /// Cleans up resources used by the pass.
        /// </summary>
        public new void Dispose()
        {
            m_MainLightShadowmapTexture?.Release();
            m_EmptyLightShadowmapTexture?.Release();
        }
        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if (m_CreateEmptyShadowmap)
            {
                SetEmptyMainLightCascadeShadowmap(ref context, ref renderingData);
                cmd.SetGlobalTexture(m_MainLightShadowmapID, m_EmptyLightShadowmapTexture.nameID);

                return;
            }

            RenderMainLightCascadeShadowmap(ref context, ref renderingData, cmd);
            cmd.SetGlobalTexture(m_MainLightShadowmapID, m_MainLightShadowmapTexture.nameID);

            // keep rthandle for others purpose
            ShadowUtilsEx.currentMainLightShadowMapTexture = m_MainLightShadowmapTexture;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            renderingData.cameraData.renderer.RemoveRenderPass(typeof(URPMainLightShadowPass));
            OverrideURPParams(ref renderingData);
            Setup(ref renderingData);
#if UNITY_EDITOR
            DrawMainLightGizmos(ref renderingData);
#endif
        }

        private void OverrideURPParams(ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            if (Feature.isOverrideShadowDistance)
                cameraData.maxShadowDistance = Mathf.Min(cameraData.camera.farClipPlane, Feature.shadowDistance);
        }

        void DrawMainLightGizmos(ref RenderingData renderingData)
        {
            int shadowLightIndex = renderingData.lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return;

            VisibleLight shadowLight = renderingData.lightData.visibleLights[shadowLightIndex];
            Light light = shadowLight.light;
            var camTr = Camera.main.transform;
            if (!camTr)
                return;

            ShadowUtilsEx.DrawLightGizmos(light, camTr.position + Feature.camPosOffset, Feature.orthoSize, Feature.near, Feature.far);
        }
        /// <summary>
        /// Sets up the pass.
        /// </summary>
        /// <param name="renderingData"></param>
        /// <returns>True if the pass should be enqueued, otherwise false.</returns>
        /// <seealso cref="RenderingData"/>
        public bool Setup(ref RenderingData renderingData)
        {
            //if (!renderingData.shadowData.mainLightShadowsEnabled)
            //    return false;

            //using var profScope = new ProfilingScope(null, m_ProfilingSetupSampler);

            if (!renderingData.shadowData.supportsMainLightShadows)
                return SetupForEmptyRendering(ref renderingData);

            Clear();
            int shadowLightIndex = renderingData.lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return SetupForEmptyRendering(ref renderingData);

            VisibleLight shadowLight = renderingData.lightData.visibleLights[shadowLightIndex];
            Light light = shadowLight.light;
            var camera = renderingData.cameraData.camera;
            if (light.shadows == LightShadows.None)
                return SetupForEmptyRendering(ref renderingData);

            if (shadowLight.lightType != LightType.Directional)
            {
                Debug.LogWarning("Only directional lights are supported as main light.");
            }

            Bounds bounds;
            if (!renderingData.cullResults.GetShadowCasterBounds(shadowLightIndex, out bounds))
                return SetupForEmptyRendering(ref renderingData);

            m_ShadowCasterCascadesCount = renderingData.shadowData.mainLightShadowCascadesCount;

            int shadowResolution = ShadowUtils.GetMaxTileResolutionInAtlas(renderingData.shadowData.mainLightShadowmapWidth,
                renderingData.shadowData.mainLightShadowmapHeight, m_ShadowCasterCascadesCount);
            renderTargetWidth = renderingData.shadowData.mainLightShadowmapWidth;
            renderTargetHeight = (m_ShadowCasterCascadesCount == 2) ?
                renderingData.shadowData.mainLightShadowmapHeight >> 1 :
                renderingData.shadowData.mainLightShadowmapHeight;

            var shadowInfo = (light, camera, Feature.orthoSize, Feature.near, Feature.far, Feature.camPosOffset,Feature.camLightPosRate);

            for (int cascadeIndex = 0; cascadeIndex < m_ShadowCasterCascadesCount; ++cascadeIndex)
            {
                bool success = ShadowUtilsEx.ExtractDirectionalLightMatrix(ref renderingData.cullResults, ref renderingData.shadowData,
                    shadowLightIndex, cascadeIndex, renderTargetWidth, renderTargetHeight, shadowResolution, light.shadowNearPlane,
                    out m_CascadeSplitDistances[cascadeIndex], out m_CascadeSlices[cascadeIndex],
                    shadowInfo,
                    cascadeIndex == 0 // only main
                    );

                if (!success)
                    return SetupForEmptyRendering(ref renderingData);
            }

            ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_MainLightShadowmapTexture, renderTargetWidth, renderTargetHeight, k_ShadowmapBufferBits, name: "_MainLightShadowmapTexture");

            m_MaxShadowDistanceSq = renderingData.cameraData.maxShadowDistance * renderingData.cameraData.maxShadowDistance;
            m_CascadeBorder = renderingData.shadowData.mainLightShadowCascadeBorder;
            m_CreateEmptyShadowmap = false;
            //useNativeRenderPass = true;
            ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_EmptyLightShadowmapTexture, 1, 1, k_ShadowmapBufferBits, name: "_EmptyLightShadowmapTexture");

            return true;
        }

        bool SetupForEmptyRendering(ref RenderingData renderingData)
        {
            //if (!renderingData.cameraData.renderer.stripShadowsOffVariants)
            //    return false;

            m_CreateEmptyShadowmap = true;
            //useNativeRenderPass = false;
            ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_EmptyLightShadowmapTexture, k_EmptyShadowMapDimensions, k_EmptyShadowMapDimensions, k_ShadowmapBufferBits, name: k_EmptyShadowMapName);

            return true;
        }

        /// <inheritdoc />
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (m_CreateEmptyShadowmap)
                ConfigureTarget(m_EmptyLightShadowmapTexture);
            else
                ConfigureTarget(m_MainLightShadowmapTexture);
            ConfigureClear(ClearFlag.All, Color.black);
        }




        void Clear()
        {
            for (int i = 0; i < m_MainLightShadowMatrices.Length; ++i)
                m_MainLightShadowMatrices[i] = Matrix4x4.identity;

            for (int i = 0; i < m_CascadeSplitDistances.Length; ++i)
                m_CascadeSplitDistances[i] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            for (int i = 0; i < m_CascadeSlices.Length; ++i)
                m_CascadeSlices[i].Clear();
        }

        void SetEmptyMainLightCascadeShadowmap(ref ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferEx.defaultCmd;
            CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadows, true);
            SetEmptyMainLightShadowParams(cmd);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        internal static void SetEmptyMainLightShadowParams(CommandBuffer cmd)
        {
            cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowParams, s_EmptyShadowParams);
            cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowmapSize, s_EmptyShadowmapSize);
        }


        void RenderMainLightCascadeShadowmap(ref ScriptableRenderContext context, ref RenderingData renderingData,CommandBuffer cmd)
        {
            var cullResults = renderingData.cullResults;
            var lightData = renderingData.lightData;
            var shadowData = renderingData.shadowData;

            int shadowLightIndex = lightData.mainLightIndex;
            if (shadowLightIndex == -1)
                return;
            var asset = UniversalRenderPipeline.asset;

            VisibleLight shadowLight = lightData.visibleLights[shadowLightIndex];

            using (new ProfilingScope(cmd, profilingSampler))
            {
                var settings = new ShadowDrawingSettings(cullResults, shadowLightIndex, BatchCullingProjectionType.Orthographic);
                settings.useRenderingLayerMaskTest = UniversalRenderPipeline.asset.useRenderingLayers;
                // Need to start by setting the Camera position as that is not set for passes executed before normal rendering
                cmd.SetGlobalVector(ShaderPropertyIds.worldSpaceCameraPos, renderingData.cameraData.worldSpaceCameraPos);

                for (int cascadeIndex = 0; cascadeIndex < m_ShadowCasterCascadesCount; ++cascadeIndex)
                {
                    settings.splitData = m_CascadeSlices[cascadeIndex].splitData;

                    Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight, shadowLightIndex, ref renderingData.shadowData, m_CascadeSlices[cascadeIndex].projectionMatrix, m_CascadeSlices[cascadeIndex].resolution);
                    ShadowUtils.SetupShadowCasterConstantBuffer(cmd, ref shadowLight, shadowBias);
                    CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.CastingPunctualLightShadow, false);
                    ShadowUtils.RenderShadowSlice(cmd, ref context, ref m_CascadeSlices[cascadeIndex],
                        ref settings, m_CascadeSlices[cascadeIndex].projectionMatrix, m_CascadeSlices[cascadeIndex].viewMatrix);
                }

                //renderingData.shadowData.isKeywordSoftShadowsEnabled = shadowLight.light.shadows == LightShadows.Soft && renderingData.shadowData.supportsSoftShadows;
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadows, renderingData.shadowData.mainLightShadowCascadesCount == 1);
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadowCascades, renderingData.shadowData.mainLightShadowCascadesCount > 1);
                //var isSoft = Feature.isSoftShadow;
                var isSoft = renderingData.shadowData.supportsSoftShadows && shadowLight.light.shadows == LightShadows.Soft;
                var softQuality = asset.GetSoftShadowQuality();
                ShadowUtilsEx.SetSoftShadowQualityShaderKeywords(cmd, isSoft, softQuality);

                SetupMainLightShadowReceiverConstants(cmd, ref shadowLight, ref renderingData.shadowData);
            }
        }

        void SetupMainLightShadowReceiverConstants(CommandBuffer cmd, ref VisibleLight shadowLight, ref ShadowData shadowData)
        {
            Light light = shadowLight.light;
            bool softShadows = shadowLight.light.shadows == LightShadows.Soft && shadowData.supportsSoftShadows;

            int cascadeCount = m_ShadowCasterCascadesCount;
            for (int i = 0; i < cascadeCount; ++i)
                m_MainLightShadowMatrices[i] = m_CascadeSlices[i].shadowTransform;

            // We setup and additional a no-op WorldToShadow matrix in the last index
            // because the ComputeCascadeIndex function in Shadows.hlsl can return an index
            // out of bounds. (position not inside any cascade) and we want to avoid branching
            Matrix4x4 noOpShadowMatrix = Matrix4x4.zero;
            noOpShadowMatrix.m22 = (SystemInfo.usesReversedZBuffer) ? 1.0f : 0.0f;
            for (int i = cascadeCount; i <= k_MaxCascades; ++i)
                m_MainLightShadowMatrices[i] = noOpShadowMatrix;

            float invShadowAtlasWidth = 1.0f / renderTargetWidth;
            float invShadowAtlasHeight = 1.0f / renderTargetHeight;
            float invHalfShadowAtlasWidth = 0.5f * invShadowAtlasWidth;
            float invHalfShadowAtlasHeight = 0.5f * invShadowAtlasHeight;
            float softShadowsProp = ShadowUtilsEx.SoftShadowQualityToShaderProperty(light, softShadows);

            ShadowUtilsEx.GetScaleAndBiasForLinearDistanceFade(m_MaxShadowDistanceSq, m_CascadeBorder, out float shadowFadeScale, out float shadowFadeBias);

            cmd.SetGlobalMatrixArray(MainLightShadowConstantBuffer._WorldToShadow, m_MainLightShadowMatrices);
            cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowParams,
                new Vector4(light.shadowStrength, softShadowsProp, shadowFadeScale, shadowFadeBias));

            if (m_ShadowCasterCascadesCount > 1)
            {
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres0,
                    m_CascadeSplitDistances[0]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres1,
                    m_CascadeSplitDistances[1]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres2,
                    m_CascadeSplitDistances[2]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres3,
                    m_CascadeSplitDistances[3]);
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSphereRadii, new Vector4(
                    m_CascadeSplitDistances[0].w * m_CascadeSplitDistances[0].w,
                    m_CascadeSplitDistances[1].w * m_CascadeSplitDistances[1].w,
                    m_CascadeSplitDistances[2].w * m_CascadeSplitDistances[2].w,
                    m_CascadeSplitDistances[3].w * m_CascadeSplitDistances[3].w));
            }

            // Inside shader soft shadows are controlled through global keyword.
            // If any additional light has soft shadows it will force soft shadows on main light too.
            // As it is not trivial finding out which additional light has soft shadows, we will pass main light properties if soft shadows are supported.
            // This workaround will be removed once we will support soft shadows per light.
            if (shadowData.supportsSoftShadows)
            {
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowOffset0,
                    new Vector4(-invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight,
                        invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight));
                cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowOffset1,
                    new Vector4(-invHalfShadowAtlasWidth, invHalfShadowAtlasHeight,
                        invHalfShadowAtlasWidth, invHalfShadowAtlasHeight));

                cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowmapSize, new Vector4(invShadowAtlasWidth,
                    invShadowAtlasHeight,
                    renderTargetWidth, renderTargetHeight));
            }
        }
    }
}
