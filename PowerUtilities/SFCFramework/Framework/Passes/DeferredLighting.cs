using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PowerUtilities;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Experimental.GlobalIllumination;
using LightType = UnityEngine.LightType;
using Unity.Collections;

#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// Draw light geometry one by one
    /// 
    /// dir : plane
    /// spot : cone
    /// point : sphere
    /// other : use texture
    /// 
    /// 
    /// * GBuffer, check PowerLit's GBuffer pass
    /// </summary>
    [Tooltip("Defered Lighting")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU + "/DeferedLighting")]
    public class DeferredLighting : SRPFeature
    {


        /// <summary>
        /// 
        ///
        /// sv_target0 , xyz : albedo+giColor, w: emission.z
        /// sv_target1, xy:normal.xy,zw:emission.xy
        /// sv_target2, xy(16) : motion vector.xy
        /// sv_target3 , xyz:pbrMask, w:mainLightShadow
        /// 
        /// </summary>
        [Header("RT Names")]
        public bool IsCreateAndSetRTs = true;
        public string _GBuffer0 = nameof(_GBuffer0);
        public string _GBuffer1 = nameof(_GBuffer1);
        public string _GBuffer2 = nameof(_GBuffer2);
        public string _MotionVectorTexture = nameof(_MotionVectorTexture);

        [Header("Objects")]
        public string deferedTag = "UniversalGBuffer";
        public int layers = -1;
        [Header("Override Shader")]
        public Shader objectShader;
        public int objectShaderPass;

        [Header("Lights")]
        public Material lightMat;

        public Mesh dirLightMesh;
        public Mesh pointLightMesh;
        public Mesh spotLightMesh;

        public float falloff=1;

        //[Header("Output")]
        //public string targetName;
        public override ScriptableRenderPass GetPass() => new DeferedLightingPass(this);
    }

    public class DeferedLightingPass : SRPPass<DeferredLighting>
    {

        RenderTargetIdentifier[] colorTargets = new RenderTargetIdentifier[4];
        const float kStencilShapeGuard = 1.06067f; // stencil geometric shapes must be inflated to fit the analytic shapes.
        float4 defaultLightAtten = new float4(0, 1, 0, 1);

        public DeferedLightingPass(DeferredLighting feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute()
                ;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            
            ref var cameraDate = ref renderingData.cameraData;
            var renderer = (UniversalRenderer)cameraDate.renderer;
            var colorAttachmentA = renderer.GetCameraColorAttachmentA();

            SetupTargets(ref context, ref renderingData, cmd);
            DrawScene(ref context, ref renderingData, cmd, cameraDate);

            if (!Feature.lightMat)
                return;

            cmd.SetRenderTarget(colorAttachmentA);
            cmd.Execute(ref context);

            var lightGroups = renderingData.lightData.visibleLights
                .OrderBy(vl => vl.lightType == LightType.Directional ? -1 : 0)
                .Select(vl => vl.light)
                .GroupBy(l => l.type)
                .ToArray()
                ;

            var lightId = 0;
            foreach (var g in lightGroups)
            {
                foreach (var light in g)
                {
                    var isDirLight = light.type == LightType.Directional;
                    var isPointLight = light.type == LightType.Point;
                    var isSpotLight = light.type == LightType.Spot;

                    var lightDir = isDirLight ? -light.transform.forward : light.transform.position;
                    var lightW = isDirLight ? 0 : 1;
                    var spotDir = Vector3.forward;



                    Vector4 lightAtten = defaultLightAtten;//{xy: distance(point,spot), zw:angle(spot)}
                    GetPunctualLightDistanceAttenuation(light.range, ref lightAtten);
                    if (light.type == LightType.Spot)
                    {
                        GetSpotAngleAttenuation(light.spotAngle, light.innerSpotAngle, ref lightAtten);
                        cmd.SetGlobalVector(ShaderPropertyIds._SpotLightAngle, new Vector4(math.radians(light.spotAngle), math.radians(light.innerSpotAngle)));

                        spotDir = -light.transform.forward;

                        //var alpha = Mathf.Deg2Rad * light.spotAngle * 0.5f;
                        //math.sincos(alpha, out var sinAlpha, out var cosAlpha);

                        //var guard = Mathf.Lerp(1f, kStencilShapeGuard, sinAlpha);
                        //cmd.SetGlobalVector(ShaderPropertyIds._SpotLightScale, new Vector4(sinAlpha, sinAlpha, 1.0f - cosAlpha, light.range));
                        //cmd.SetGlobalVector(ShaderPropertyIds._SpotLightBias, new Vector4(0.0f, 0.0f, cosAlpha, 0.0f));
                        //cmd.SetGlobalVector(ShaderPropertyIds._SpotLightGuard, new Vector4(guard, guard, guard, cosAlpha * light.range));
                    }

                    cmd.SetGlobalVector(ShaderPropertyIds._LightAttenuation, lightAtten);
                    cmd.SetGlobalVector(ShaderPropertyIds._MainLightPosition, new float4(lightDir, lightW));
                    cmd.SetGlobalColor(ShaderPropertyIds._MainLightColor, light.color * light.intensity);
                    cmd.SetGlobalVector(ShaderPropertyIds._LightDirection, spotDir);

                    cmd.SetGlobalVector(ShaderPropertyIds._LightRadiusIntensityFalloff, new float4(light.range, light.intensity, Feature.falloff, isSpotLight ? 1 : 0));


                    var lightTr = isDirLight ? float4x4.identity : float4x4.TRS(light.transform.position, light.transform.rotation, new float3(1) * (light.range * 2));


                    //---
                    SetupBlend(cmd, lightId, isDirLight);

                    var targetMesh = GetTargetMesh(light);
                    var passId = isDirLight ? 0 : 1;
                    if (targetMesh)
                        cmd.DrawMesh(targetMesh, lightTr, Feature.lightMat, 0, 0);

                    //
                    lightId++;
                }
            }

        }
        /// <summary>
        /// dir:  Blend One srcAlpha, Zero One
        /// other : Blend One one, Zero One
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="lightId"></param>
        /// <param name="isDirLight"></param>
        private static void SetupBlend(CommandBuffer cmd, int lightId, bool isDirLight)
        {
            var srcMode = BlendMode.One;
            var dstMode = BlendMode.One;
            var srcAlphaMode = BlendMode.Zero;
            var dstAlphaMode = BlendMode.One;
            var cullMode = CullMode.Off; // dir off
            if (isDirLight || lightId == 0)
            {
                dstMode = BlendMode.SrcAlpha;
            }
            else
            {
                cullMode = CullMode.Front;
            }

            cmd.SetGlobalInt(ShaderPropertyIds._SrcMode, (int)srcMode);
            cmd.SetGlobalInt(ShaderPropertyIds._DstMode, (int)dstMode);
            cmd.SetGlobalInt(ShaderPropertyIds._SrcAlphaMode, (int)srcAlphaMode);
            cmd.SetGlobalInt(ShaderPropertyIds._DstAlphaMode, (int)dstAlphaMode);
            cmd.SetGlobalInt(ShaderPropertyIds._CullMode, (int)cullMode);
        }

        Mesh GetTargetMesh(Light light) => light.type switch
        {
            LightType.Directional => Feature.dirLightMesh,
            LightType.Point => Feature.pointLightMesh,
            LightType.Spot => Feature.spotLightMesh,
            _ => null,
        };
        private void DrawScene(ref ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd, CameraData cameraDate)
        {
            var sortingSettings = new SortingSettings(cameraDate.camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            var drawSettings = new DrawingSettings(new ShaderTagId(Feature.deferedTag), sortingSettings)
            {
                enableDynamicBatching = true,
                perObjectData = PerObjectData.LightProbe | PerObjectData.ReflectionProbes | PerObjectData.Lightmaps | PerObjectData.LightData
                | PerObjectData.MotionVectors | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask,
                overrideShader = Feature.objectShader,
                overrideShaderPassIndex = Feature.objectShaderPass,
            };

            //drawSettings.SetShaderPassNames(RenderingTools.urpForwardShaderPassNames);

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque, Feature.layers);

            context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSettings);
        }

        private void SetupTargets(ref ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            int gbuffer0 = Shader.PropertyToID(Feature._GBuffer0);
            int gbuffer1 = Shader.PropertyToID(Feature._GBuffer1);
            int gbuffer2 = Shader.PropertyToID(Feature._GBuffer2);
            int gbuffer3 = Shader.PropertyToID(Feature._MotionVectorTexture);

            var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
            colorDesc.depthBufferBits = 0;
            colorDesc.msaaSamples = 1;
            colorDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;

            var motionDesc = colorDesc;
            motionDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16_SFloat;

            if (Feature.IsCreateAndSetRTs)
            {
                cmd.GetTemporaryRT(gbuffer0, colorDesc);
                cmd.GetTemporaryRT(gbuffer1, colorDesc);
                cmd.GetTemporaryRT(gbuffer2, colorDesc);
                cmd.GetTemporaryRT(gbuffer3, motionDesc);
            }

            var depthDesc = colorDesc;
            depthDesc.colorFormat = RenderTextureFormat.Depth;
            depthDesc.depthBufferBits = 24;
            //cmd.GetTemporaryRT(ShaderPropertyIds._CameraDepthAttachment, depthDesc);

            colorTargets[0] = gbuffer0;
            colorTargets[1] = gbuffer1;
            colorTargets[2] = gbuffer2;
            colorTargets[3] = gbuffer3;

            //var depthId = ShaderPropertyIdentifier._CameraDepthAttachment;
            var depthId = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            if (Feature.IsCreateAndSetRTs)
                cmd.SetRenderTarget(colorTargets, depthId);

            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.Execute(ref context);
        }

        /**
         unity's api
         */
        public static void GetPunctualLightDistanceAttenuation(float lightRange, ref Vector4 lightAttenuation)
        {
            // Light attenuation in universal matches the unity vanilla one (HINT_NICE_QUALITY).
            // attenuation = 1.0 / distanceToLightSqr
            // The smoothing factor makes sure that the light intensity is zero at the light range limit.
            // (We used to offer two different smoothing factors.)

            // The current smoothing factor matches the one used in the Unity lightmapper.
            // smoothFactor = (1.0 - saturate((distanceSqr * 1.0 / lightRangeSqr)^2))^2
            float lightRangeSqr = lightRange * lightRange;
            float fadeStartDistanceSqr = 0.8f * 0.8f * lightRangeSqr;
            float fadeRangeSqr = (fadeStartDistanceSqr - lightRangeSqr);
            float lightRangeSqrOverFadeRangeSqr = -lightRangeSqr / fadeRangeSqr;
            float oneOverLightRangeSqr = 1.0f / Mathf.Max(0.0001f, lightRangeSqr);

            // On all devices: Use the smoothing factor that matches the GI.
            lightAttenuation.x = oneOverLightRangeSqr;
            lightAttenuation.y = lightRangeSqrOverFadeRangeSqr;
        }

        public static void GetSpotAngleAttenuation(
            float spotAngle, float? innerSpotAngle,
            ref Vector4 lightAttenuation)
        {
            // Spot Attenuation with a linear falloff can be defined as
            // (SdotL - cosOuterAngle) / (cosInnerAngle - cosOuterAngle)
            // This can be rewritten as
            // invAngleRange = 1.0 / (cosInnerAngle - cosOuterAngle)
            // SdotL * invAngleRange + (-cosOuterAngle * invAngleRange)
            // If we precompute the terms in a MAD instruction
            float cosOuterAngle = Mathf.Cos(Mathf.Deg2Rad * spotAngle * 0.5f);
            // We need to do a null check for particle lights
            // This should be changed in the future
            // Particle lights will use an inline function
            float cosInnerAngle;
            if (innerSpotAngle.HasValue)
                cosInnerAngle = Mathf.Cos(innerSpotAngle.Value * Mathf.Deg2Rad * 0.5f);
            else
                cosInnerAngle = Mathf.Cos((2.0f * Mathf.Atan(Mathf.Tan(spotAngle * 0.5f * Mathf.Deg2Rad) * (64.0f - 18.0f) / 64.0f)) * 0.5f);
            float smoothAngleRange = Mathf.Max(0.001f, cosInnerAngle - cosOuterAngle);
            float invAngleRange = 1.0f / smoothAngleRange;
            float add = -cosOuterAngle * invAngleRange;

            lightAttenuation.z = invAngleRange;
            lightAttenuation.w = add;
        }

        public static void GetSpotDirection(ref Matrix4x4 lightLocalToWorldMatrix, out Vector4 lightSpotDir)
        {
            Vector4 dir = lightLocalToWorldMatrix.GetColumn(2);
            lightSpotDir = new Vector4(-dir.x, -dir.y, -dir.z, 0.0f);
        }
    }
}
