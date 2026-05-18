#if UNITY_6000_3_OR_NEWER
namespace PowerUtilities.RenderFeatures
{
#if UNITY_EDITOR
    using UnityEditor.SceneManagement;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Rendering.Universal.Internal;
    using UnityEngine.SceneManagement;
    using PowerUtilities;
    using Object = UnityEngine.Object;
    using Unity.Mathematics;
    using RenderQueueType = PowerUtilities.RenderQueueType;
    using UnityEngine.Rendering.RenderGraphModule;

    /// <summary>
    /// mrt 6000.4 + renderPassEvent need >= After Skybox, otherwise may cause issue
    /// </summary>
    public class DrawObjectsPassControl : SRPPass<DrawObjects>
    {
        FullDrawObjectsPass drawObjectsPass;
        
        public DrawObjectsPassControl(DrawObjects feature) : base(feature)
        {
            drawObjectsPass = new FullDrawObjectsPass(feature);
        }

        public override bool IsTryRestoreLastTargets(Camera c) => c.IsGameCamera();

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            //if (renderPassEvent <= RenderPassEvent.BeforeRenderingOpaques)
            //    renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

            // sync data
            drawObjectsPass.defaultPassData = defaultPassData;

            drawObjectsPass.OnCameraSetup(cmd, ref renderingData);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref CameraData cameraData = ref renderingData.cameraData;

            var renderer = (UniversalRenderer)renderingData.cameraData.renderer;

            // setup depthTextureMode
            if (camera.IsGameCamera())
                camera.depthTextureMode = Feature.cameraDepthTextureMode;

            // draw scene use reflection camera(view,proj)
            var viewMat = cameraData.GetViewMatrix();
            var projMat = cameraData.GetProjectionMatrix();
            if (Feature.isUseReflectionCamera)
                SetupRenderReflectionCamera(ref cameraData,cmd,viewMat,projMat);
            // draw scene
            drawObjectsPass.OnExecute(context, ref renderingData, cmd);

            // restore camera (view,proj)
            if (Feature.overrideCamera)
            {
                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMat, projMat, false);
            }
            if (Feature.isUseReflectionCamera)
                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMat, projMat, false);
        }

        private void SetupRenderReflectionCamera(ref CameraData cameraData, CommandBuffer cmd, Matrix4x4 viewMat, Matrix4x4 projMat)
        {
            var camTr = cameraData.camera.transform;
            camTr.GetReflection(Feature.reflectionPlaneTr, Feature.planeYOffset, out var camForward, out var camUp, out var camPos);

            var v = Float4x4Ex.LookAtInverse(camPos, camPos + camForward, camUp);
            DebugTools.DrawAxis(camPos, v.c0.xyz, v.c1.xyz, v.c2.xyz);

            RenderingUtils.SetViewAndProjectionMatrices(cmd, v, projMat, false);
        }
    }


    public class FullDrawObjectsPass : SRPPass<DrawObjects>
    {
        static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");
        public List<ShaderTagId> shaderTagList = new List<ShaderTagId>();

        public FilteringSettings filteringSettings;
        public RenderStateBlock renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

        public static event RefAction<DrawingSettings> OnSetupDrawSettings;

        Light sun;
        bool lastSRPBatchEnabled;


        RendererListHandle rendererListHandle;
        RendererListHandle errorRendererListHandle;

        public FullDrawObjectsPass(DrawObjects feature) : base(feature)
        {
            Init();
        }
        /// <summary>
        /// Apply Feature data
        /// when change feature's params call this
        /// </summary>
        public void Init()
        {
            SetupShaderTagList(Feature.shaderTags);
            SetupFilterSettings(Feature);
            SetupRenderStateBlock(Feature);
        }

        public void SetupShaderTagList(string[] shaderTags)
        {
            shaderTagList.Clear();
            foreach (string shaderTag in shaderTags)
                shaderTagList.Add(new ShaderTagId(shaderTag));
        }

        public void SetupRenderStateBlock(DrawObjects feature)
        {
            // setup render stateBlock(depth,stencil)
            var stencilData = feature.stencilData;
            var stencilState = StencilState.defaultValue;
            stencilState.enabled = stencilData.overrideStencilState;
            stencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            stencilState.SetFailOperation(stencilData.failOperation);
            stencilState.SetPassOperation(stencilData.passOperation);
            stencilState.SetZFailOperation(stencilData.zFailOperation);

            if (feature.stencilData.overrideStencilState)
            {
                renderStateBlock.stencilState = stencilState;
                renderStateBlock.mask = RenderStateMask.Stencil;
                renderStateBlock.stencilReference = stencilData.stencilReference;
            }

            if (feature.depthState.isOverrideDepthState)
            {
                renderStateBlock.mask |= RenderStateMask.Depth;
                renderStateBlock.depthState = new DepthState(feature.depthState.isWriteDepth, feature.depthState.compareFunc);
            }
        }

        public void SetupFilterSettings(DrawObjects feature)
        {
            // setup filterSettings
            var renderQueueRange = RenderQueueTools.ToRenderQueueRange(feature.renderQueueType);
            filteringSettings = new FilteringSettings(renderQueueRange, feature.layers);

            if (feature.isOverrideFilterSetting)
                filteringSettings = feature.filterSetting;
        }

        private FilteringSettings GetFilterSettings()
        {
            if (filteringSettings.layerMask != Feature.layers)
                filteringSettings.layerMask = Feature.layers;

            return filteringSettings;
        }

        void SwitchCheckOverdraw()
        {
            if (Feature.isSwitchOverdrawMode)
            {
                Feature.isSwitchOverdrawMode = false;

                // switch overdraw mode
                Feature.isEnterCheckOverdraw = !Feature.isEnterCheckOverdraw;

                if (Feature.isEnterCheckOverdraw)
                {
                    EnterCheckOverdrawMode();
                }
                else
                {
                    ExistCheckOverdrawMode();
                }
            }
            
            // inner methods
            void ExistCheckOverdrawMode()
            {
                Feature.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
                Feature.overrideMaterial = null;
            }

            void EnterCheckOverdrawMode()
            {
                Feature.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
                Feature.overrideMaterial = Feature.overdrawMat;
            }
        }


        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd,ref renderingData);

            sun = RenderSettings.sun;

            lastSRPBatchEnabled = UniversalRenderPipeline.asset.useSRPBatcher;

            var rendererListParams = SetupRendererListParams(ref context, ref renderingData, cmd);
            var errorRendererListParams = RenderingTools.GetErrorRendererListParams(ref context, ref renderingData.cullResults, renderingData.cameraData.camera, filteringSettings, SortingCriteria.None);

            rendererListHandle = defaultPassData.renderGraph.CreateRendererList(rendererListParams);
            errorRendererListHandle = defaultPassData.renderGraph.CreateRendererList(errorRendererListParams);

            defaultPassData.rasterBuilder.UseRendererList(rendererListHandle);
            defaultPassData.rasterBuilder.UseRendererList(errorRendererListHandle);

            SwitchCheckOverdraw();
        }

        static NativeArray<RenderStateBlock> renderStateBlockArr;

        //[ApplicationExit]
        //[CompileStarted]
        static void DisposeNative()
        {
            if (renderStateBlockArr.IsCreated)
                renderStateBlockArr.Dispose();
        }

        static FullDrawObjectsPass()
        {
            ApplicationTools.OnDomainUnload += DisposeNative;
        }

        void DrawScene(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var drawObjectPassData = new Vector4(0, 0, 0, Feature.renderQueueType == RenderQueueType.opaque ? 1 : 0);
            cmd.SetGlobalVector(s_DrawObjectPassDataPropID, drawObjectPassData);

            // scaleBias.x = flipSign
            // scaleBias.y = scale
            // scaleBias.z = bias
            // scaleBias.w = unused

            var flipSign = renderingData.cameraData.IsCameraProjectionMatrixFlipped() ? -1f : 1f;
            var scaleBias = flipSign < 0 ? new Vector4(flipSign, 1, -1, 1) : new Vector4(flipSign, 0, 1, 1);
            cmd.SetGlobalVector(ShaderPropertyIds.scaleBias, scaleBias);
            ColorSpaceTransform.SetColorSpace(cmd, Feature.colorSpaceMode);
            cmd.Execute(ref context);

            //------
            //context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSetting, null, renderStateBlockArr);
            //RenderingTools.DrawErrorObjects(cmd, ref context, ref renderingData.cullResults, camera, filterSetting, SortingCriteria.None);

            // 6000 draw renderer 
            if(rendererListHandle.IsValid())
                context.GetRasterContext().cmd.DrawRendererList(rendererListHandle);
            if(errorRendererListHandle.IsValid())
                context.GetRasterContext().cmd.DrawRendererList(errorRendererListHandle);

            RestoreDrawSettings(ref renderingData, cmd);
        }

        public RendererListParams SetupRendererListParams(ref ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;

            var filterSetting = GetFilterSettings();
#if UNITY_EDITOR
            var scene = SceneManager.GetActiveScene();
            var stage = StageUtility.GetCurrentStage();

            var isPrefabStage = !string.IsNullOrEmpty(stage.assetPath);
            if (camera.cameraType == CameraType.Preview || (isPrefabStage && Feature.isShowAllInPrefabStage))
                filterSetting.layerMask = -1;

#endif
            OverrideCamera(ref context, cmd, ref renderingData);
            var drawSettings = GetDrawSettings(context, cmd, ref renderingData, ref cameraData);

            NativeArrayTools.CreateIfNull(ref renderStateBlockArr, 1);
            renderStateBlockArr[0] = renderStateBlock;
            //-----
            var rendererListParams = new RendererListParams(renderingData.cullResults, drawSettings, filterSetting)
            {
                stateBlocks = renderStateBlockArr,
            };
            rendererListParams.tagValues = rendererListParams.GetDefaultTagArr();

            return rendererListParams;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            DrawScene(context, ref renderingData, cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (Feature.overrideSRPBatch)
            {
                UniversalRenderPipeline.asset.useSRPBatcher = lastSRPBatchEnabled;
            }
        }

        private void RestoreDrawSettings(ref RenderingData renderingData, CommandBuffer cmd)
        {
            var cameraData = renderingData.cameraData;

            if (Feature.overrideMainLightIndex && Feature.isRestoreMainLightIndexFinish)
            {
                OverrideMainLight(context, ref renderingData, sun);
            }
        }

        private DrawingSettings GetDrawSettings(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData, ref CameraData cameraData)
        {
            var cam = renderingData.cameraData.camera;

            var sortFlags = Feature.renderQueueType == RenderQueueType.opaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
            var drawSettings = CreateDrawingSettings(shaderTagList, ref renderingData, sortFlags);
            drawSettings.overrideMaterial = Feature.overrideMaterial;
            drawSettings.overrideMaterialPassIndex = Feature.overrideMaterialPassIndex;
#if UNITY_2021_1_OR_NEWER
            drawSettings.fallbackMaterial = Feature.fallbackMaterial;
#endif
            if (Feature.overrideMainLightIndex)
            {
                // show current vl name
                Feature.visibleLightNames.Clear();
                Feature.visibleLightNames.AddRange(
                    renderingData.lightData.visibleLights
                    .Where(vl => vl.lightType == LightType.Directional)
                    .Select(vl => vl.light.name)
                    );

                Feature.mainLightIndex = Mathf.Clamp(Feature.mainLightIndex, 0, renderingData.lightData.visibleLights.Length-1);
                var mainLightIndex = Feature.mainLightIndex;
                //// find by lightName
                if (!string.IsNullOrEmpty(Feature.lightName))
                {
                    mainLightIndex = FindLightIndex(ref renderingData, Feature.lightName);
                }

                OverrideMainLight(context, ref renderingData, renderingData.lightData.visibleLights[mainLightIndex].light);
            }
            if (Feature.overridePerObjectData)
            {
                drawSettings.perObjectData = Feature.perObjectData;
            }

            if (Feature.overrideDynamicBatching)
                drawSettings.enableDynamicBatching = Feature.enableDynamicBatching;

            if (Feature.overrideGPUInstancing)
                drawSettings.enableInstancing = Feature.enableGPUInstancing;

            if (Feature.overrideSRPBatch && UniversalRenderPipeline.asset)
            {

                UniversalRenderPipeline.asset.useSRPBatcher = Feature.enableSRPBatch;
                GraphicsSettings.useScriptableRenderPipelineBatching = Feature.enableSRPBatch;
            }

            if (OnSetupDrawSettings != null)
            {
                OnSetupDrawSettings(ref drawSettings);
            }

            // disable instancing
            if (cameraData.isPreviewCamera)
                drawSettings.enableInstancing = false;

            return drawSettings;
        }

        private void OverrideCamera(ref ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (Feature.overrideCamera)
            {
                var cameraData = renderingData.cameraData;
                var cam = cameraData.camera;

                var aspect = cam.pixelWidth / (float)cam.pixelHeight;
                var projMat = Matrix4x4.Perspective(Feature.cameraFOV, aspect, cam.nearClipPlane, cam.farClipPlane);
                projMat = GL.GetGPUProjectionMatrix(projMat, renderingData.cameraData.IsCameraProjectionMatrixFlipped());

                var viewMat = cameraData.GetViewMatrix();
                viewMat.SetColumn(3, viewMat.GetColumn(3) + Feature.cameraOffset);

                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMat, projMat, false);

                cmd.Execute(ref context);
            }
        }

        public int FindLightIndex(ref RenderingData renderingData, string lightGOName)
        {
            var id = 0;
            foreach (var vl in renderingData.lightData.visibleLights)
            {
                if (vl.light.name == lightGOName)
                    return id;
                id++;
            }
            return -1;
        }

        private void OverrideMainLight(ScriptableRenderContext context, ref RenderingData renderingData, Light mainLight)
        {
            RenderSettings.sun = mainLight;
        }
    }

}
#endif // #if UNITY_6000_3_OR_NEWER