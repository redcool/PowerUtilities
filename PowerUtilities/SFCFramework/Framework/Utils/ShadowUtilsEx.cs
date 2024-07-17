using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    public static class ShadowUtilsEx
    {
        public static RTHandle currentMainLightShadowMapTexture;
        public static float SoftShadowQualityToShaderProperty(Light light, bool softShadowsEnabled)
        {
            float softShadows = softShadowsEnabled ? 1.0f : 0.0f;
            if (light.TryGetComponent(out UniversalAdditionalLightData additionalLightData))
            {
                var softShadowQuality = (additionalLightData.softShadowQuality == SoftShadowQuality.UsePipelineSettings)
                    //? UniversalRenderPipeline.asset?.softShadowQuality
                    ? UniversalRenderPipeline.asset.GetSoftShadowQuality()
                    : additionalLightData.softShadowQuality;
                softShadows *= Math.Max((int)softShadowQuality, (int)SoftShadowQuality.Low);
            }

            return softShadows;
        }

        /// copy from ShadowUtils
        public static void GetScaleAndBiasForLinearDistanceFade(float fadeDistance, float border, out float scale, out float bias)
        {
            // To avoid division from zero
            // This values ensure that fade within cascade will be 0 and outside 1
            if (border < 0.0001f)
            {
                float multiplier = 1000f; // To avoid blending if difference is in fractions
                scale = multiplier;
                bias = -fadeDistance * multiplier;
                return;
            }

            border = 1 - border;
            border *= border;

            // Fade with distance calculation is just a linear fade from 90% of fade distance to fade distance. 90% arbitrarily chosen but should work well enough.
            float distanceFadeNear = border * fadeDistance;
            scale = 1.0f / (fadeDistance - distanceFadeNear);
            bias = -distanceFadeNear / (fadeDistance - distanceFadeNear);
        }

        static readonly string[] softShadowQualityKeywords = new[]
        {
            ShaderKeywordStrings.SoftShadowsLow, ShaderKeywordStrings.SoftShadowsMedium, ShaderKeywordStrings.SoftShadowsHigh
        };
        public static void SetSoftShadowQualityShaderKeywords(CommandBuffer cmd, bool isSoftShadow,SoftShadowQuality quality = SoftShadowQuality.UsePipelineSettings)
        {
            CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, isSoftShadow);
            // off all
            ShaderEx.SetKeywords(false, softShadowQualityKeywords);
            ShaderEx.SetKeywords(true, softShadowQualityKeywords[(int)quality - 1]);
        }


        public static bool ExtractDirectionalLightMatrix(ref CullingResults cullResults, ref ShadowData shadowData, int shadowLightIndex, int cascadeIndex, int shadowmapWidth, int shadowmapHeight, int shadowResolution, float shadowNearPlane, out Vector4 cascadeSplitDistance, out ShadowSliceData shadowSliceData
            ,(Light shadowLight,Camera cam, float orthoSize, float near, float far, Vector3 camPosOffset) shadowInfo,bool isOverrideVP
            )
        {
            bool success = cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowLightIndex,
                cascadeIndex, shadowData.mainLightShadowCascadesCount, shadowData.mainLightShadowCascadesSplit, shadowResolution, shadowNearPlane, out shadowSliceData.viewMatrix, out shadowSliceData.projectionMatrix,
                out shadowSliceData.splitData);

            if (isOverrideVP)
            {
                // calc vp
                //var lightTr = shadowInfo.shadowLight.transform;
                float4x4 view, proj;
                SetupVP(shadowInfo, out view, out proj);

                shadowSliceData.viewMatrix = math.fastinverse(view);
                shadowSliceData.projectionMatrix = proj;
            }

            cascadeSplitDistance = shadowSliceData.splitData.cullingSphere;
            shadowSliceData.offsetX = (cascadeIndex % 2) * shadowResolution;
            shadowSliceData.offsetY = (cascadeIndex / 2) * shadowResolution;
            shadowSliceData.resolution = shadowResolution;
            shadowSliceData.shadowTransform = GetShadowTransform(shadowSliceData.projectionMatrix, shadowSliceData.viewMatrix);

            // It is the culling sphere radius multiplier for shadow cascade blending
            // If this is less than 1.0, then it will begin to cull castors across cascades
            shadowSliceData.splitData.shadowCascadeBlendCullingFactor = 1.0f;

            // If we have shadow cascades baked into the atlas we bake cascade transform
            // in each shadow matrix to save shader ALU and L/S
            if (shadowData.mainLightShadowCascadesCount > 1)
                ApplySliceTransform(ref shadowSliceData, shadowmapWidth, shadowmapHeight);

            return success;
        }

        private static void SetupVP((Light shadowLight,Camera cam, float orthoSize, float near, float far,Vector3 camPosOffset) shadowInfo, out float4x4 view, out float4x4 proj)
        {
            var lightTr = shadowInfo.shadowLight.transform;
            var camTr = shadowInfo.cam.transform;

            var pos = camTr.position + shadowInfo.camPosOffset;

            view = float4x4.LookAt(pos, pos + lightTr.forward, lightTr.up);
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
            {
                //view = math.mul(float4x4.Scale(1, 1, -1), view);
                view[0][2] *= -1;
                view[1][2] *= -1;
                view[2][2] *= -1;
                view[3][2] *= -1;
            }


            var size = shadowInfo.orthoSize * 2;
            proj = float4x4.Ortho(size, size, shadowInfo.near, shadowInfo.far);
            if (SystemInfo.usesReversedZBuffer)
            {
                //proj[0][2] *= -1;
                //proj[1][2] *= -1;
                proj[2][2] = (proj[2][2] * -0.5f);
                proj[3][2] = (proj[3][2] + 0.5f);
            }
        }

        public static Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
        {
            // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
            // apply z reversal to projection matrix. We need to do it manually here.
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            Matrix4x4 worldToShadow = proj * view;

            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            // textureScaleAndBias maps texture space coordinates from [-1,1] to [0,1]

            // Apply texture scale and offset to save a MAD in shader.
            return textureScaleAndBias * worldToShadow;
        }

        public static void ApplySliceTransform(ref ShadowSliceData shadowSliceData, int atlasWidth, int atlasHeight)
        {
            Matrix4x4 sliceTransform = Matrix4x4.identity;
            float oneOverAtlasWidth = 1.0f / atlasWidth;
            float oneOverAtlasHeight = 1.0f / atlasHeight;
            sliceTransform.m00 = shadowSliceData.resolution * oneOverAtlasWidth;
            sliceTransform.m11 = shadowSliceData.resolution * oneOverAtlasHeight;
            sliceTransform.m03 = shadowSliceData.offsetX * oneOverAtlasWidth;
            sliceTransform.m13 = shadowSliceData.offsetY * oneOverAtlasHeight;

            // Apply shadow slice scale and offset
            shadowSliceData.shadowTransform = sliceTransform * shadowSliceData.shadowTransform;
        }

        public static void DrawLightGizmos(Light light,Vector3 startPos,float orthoSize,float near,float far)
        {
#if UNITY_EDITOR
            var h = orthoSize;
            var w = h;
            var n = near;
            var f = far;

            var p0 = new Vector3(-w, -h, n);
            var p1 = new Vector3(-w, h, n);
            var p2 = new Vector3(w, h, n);
            var p3 = new Vector3(w, -h, n);

            var p4 = new Vector3(-w, -h, f);
            var p5 = new Vector3(-w, h, f);
            var p6 = new Vector3(w, h, f);
            var p7 = new Vector3(w, -h, f);

            var rot = light.transform.rotation;

            var vertices = new[] { p0, p1, p2, p3, p4, p5, p6, p7 };
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = (startPos + rot * vertices[i]);
            }
            // view frustum
            DebugTools.DrawLineCube(vertices);
#endif
        }
    }
}
