namespace PowerUtilities.RenderFeatures
{
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
    using Object = UnityEngine.Object;

    [Tooltip("Draw Objects with full urp powers, use SRPBatch or Instanced need multi cameras")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/DrawObjects")]
    public class DrawObjects : SRPFeature
    {
        [Header("Draw Objects Options")]
        public string[] shaderTags = new[] {
            "UniversalForwardOnly",
            "UniversalForward",
            "SRPDefaultUnlit"
        };
        [Tooltip("render opaque(queue<=2000) or transparent(queue>=3000),need match rebderPassEvent")]
        public bool isOpaque = true;
        public LayerMask layers = -1;

        [Header("--- Override stencil")]
        [Tooltip("stencil state control")]
        public StencilStateData stencilData;

        [Header("--- override material")]
        [Tooltip("use this material render objects when not empty")]
        public Material overrideMaterial;

        [Tooltip("overrideMaterial use pass index")]
        public int overrideMaterialPassIndex;

        [Tooltip("lightMode canot match, use this material")]
        public Material fallbackMaterial;


        [Header("--- Per Object Data")]
        [Tooltip("overridePerObjectData,Lightmap : (Lightmaps,LightProbe,LightProbeProxyVolume)" +
            ",ShadowMask:(ShadowMask,OcclusionProbe,OcclusionProbeProxyVolume)")]
        public bool overridePerObjectData;
        public PerObjectData perObjectData;


        [Header("--- override mainLight")]
        public bool overrideMainLightIndex;
        [Tooltip("restore mainLightIndex when draw finish")]
        public bool isRestoreMainLightIndexFinish = true;

        public int mainLightIndex;

        [Tooltip("use this light as mainLight")]
        public string lightName;
        public List<string> visibleLightNames = new List<string>();


        [Header("--- override dynamic batch")]
        [Tooltip("override urp Pipeline Asset")]
        public bool overrideDynamicBatching;
        public bool enableDynamicBatching;


        [Header("--- override instancing")]
        [Tooltip("override instancing")]
        public bool overrideGPUInstancing;
        public bool enableGPUInstancing;


        [Header("--- override srp batch")]
        public bool overrideSRPBatch;
        public bool enableSRPBatch;


        [Header("--- override camera")]
        public bool overrideCamera;
        public float cameraFOV = 60;
        public Vector4 cameraOffset;

        [Header("SkyBox")]
        public bool isDrawSkybox;
        public RenderPassEvent drawSkyboxEvent = RenderPassEvent.BeforeRenderingSkybox;

        [Header("DrawChildrenInstanced")]
        public bool isDrawChildrenInstancedOn;
        public bool forceFindDrawChildrenInstanced;

        public override ScriptableRenderPass GetPass() => new DrawObjectsPassWrapper(this);
    }

    public class DrawObjectsPassWrapper : SRPPass<DrawObjects>
    {
        FullDrawObjectsPass
            drawObjectsPass;

        DrawSkyboxPass drawSkyboxPass;
        DrawChildrenInstancedPass drawChildrenInstancedPass;

        public DrawObjectsPassWrapper(DrawObjects feature) : base(feature)
        {
            drawObjectsPass = new FullDrawObjectsPass(feature);

            if (feature.isDrawSkybox)
                drawSkyboxPass = new DrawSkyboxPass(feature.drawSkyboxEvent);

            drawChildrenInstancedPass = new DrawChildrenInstancedPass(feature);
        }

        #region Get URP DrawObjects
        public static DrawObjectsPass GetUrpDrawObjectsPass(DrawObjects feature)
        {
            UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;

            var stencilData = feature.stencilData;
            var stencilState = StencilState.defaultValue;
            stencilState.enabled = stencilData.overrideStencilState;
            stencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            stencilState.SetFailOperation(stencilData.failOperation);
            stencilState.SetPassOperation(stencilData.passOperation);
            stencilState.SetZFailOperation(stencilData.zFailOperation);

            var shaderTagIds = feature.shaderTags
                .Select(name => new ShaderTagId(name))
                .ToArray();

            var renderQueueRange = feature.isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent;

            return new DrawObjectsPass(feature.name, shaderTagIds, feature.isOpaque, feature.renderPassEvent, renderQueueRange, feature.layers, stencilState, stencilData.stencilReference);
        }
        #endregion

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            drawObjectsPass.OnCameraSetup(cmd, ref renderingData);

            if (drawSkyboxPass != null)
                drawSkyboxPass.OnCameraSetup(cmd, ref renderingData);

            drawChildrenInstancedPass.OnCameraSetup();
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            cmd.BeginSampleExecute(featureName, ref context);

            drawObjectsPass.OnExecute(context, ref renderingData, cmd);

            drawChildrenInstancedPass.OnExecute(context, ref renderingData, cmd);

            if (drawSkyboxPass != null)
                drawSkyboxPass.Execute(context, ref renderingData);

            cmd.EndSampleExecute(featureName, ref context);
        }
    }

    public class DrawChildrenInstancedPass : SRPPass<DrawObjects>
    {
        DrawChildrenInstanced[] drawChildren;
        public int findCount;

        public DrawChildrenInstancedPass(DrawObjects feature) : base(feature)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if (!Feature.isDrawChildrenInstancedOn)
                return;

            if (Feature.forceFindDrawChildrenInstanced)
            {
                Feature.forceFindDrawChildrenInstanced = false;
                findCount = 0;
            }
            //drawChildren.ForEach(dc =>
            for (int i = 0; i < drawChildren.Length; i++)
            {
                var dc = drawChildren[i];
                dc.drawInfoSO.DrawGroupList(cmd);
                cmd.Execute(ref context);
            }
        }

        public void OnCameraSetup()
        {
            if (findCount <1)
            {
                findCount ++;
                drawChildren = Object.FindObjectsOfType<DrawChildrenInstanced>();
            }
        }
    }

    public class FullDrawObjectsPass : SRPPass<DrawObjects>
    {
        static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");
        public List<ShaderTagId> shaderTagList = new List<ShaderTagId>();

        public FilteringSettings filteringSettings;
        public RenderStateBlock renderStateBlock;

        public static event RefAction<DrawingSettings> OnSetupDrawSettings;

        Light sun;
        bool lastSRPBatchEnabled;

        public FullDrawObjectsPass(DrawObjects feature) : base(feature)
        {
            shaderTagList.AddRange(feature.shaderTags.Select(n => new ShaderTagId(n)));
            var renderQueueRange = feature.isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent;
            filteringSettings = new FilteringSettings(renderQueueRange, feature.layers);

            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

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
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            sun = RenderSettings.sun;

            lastSRPBatchEnabled = UniversalRenderPipeline.asset.useSRPBatcher;

            if (renderStateBlock.depthState.compareFunction == CompareFunction.Equal)
            {
                renderStateBlock.depthState = new DepthState(true, CompareFunction.LessEqual);
                renderStateBlock.mask |= RenderStateMask.Depth;
            }
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {

            var drawObjectPassData = new Vector4(0, 0, 0, Feature.isOpaque ? 1 : 0);
            cmd.SetGlobalVector(s_DrawObjectPassDataPropID, drawObjectPassData);

            // scaleBias.x = flipSign
            // scaleBias.y = scale
            // scaleBias.z = bias
            // scaleBias.w = unused
            var flipSign = renderingData.cameraData.IsCameraProjectionMatrixFlipped() ? -1f : 1f;
            var scaleBias = flipSign < 0 ? new Vector4(flipSign, 1, -1, 1) : new Vector4(flipSign, 0, 1, 1);
            cmd.SetGlobalVector(ShaderPropertyIds.scaleBias, scaleBias);
            cmd.Execute(ref context);

            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;

            var filterSetting = GetFilterSettings();
#if UNITY_EDITOR
            if (camera.cameraType == CameraType.Preview)
                filterSetting.layerMask = -1;
#endif
            OverrideCamera(ref context, cmd, ref renderingData);
            var drawSettings = GetDrawSettings(context, cmd, ref renderingData, ref cameraData);

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSetting, ref renderStateBlock);

            RenderingTools.DrawErrorObjects(ref context, ref renderingData.cullResults, camera, filterSetting, SortingCriteria.None);

            RestoreDrawSettings(ref renderingData, cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (Feature.overrideSRPBatch)
            {
                UniversalRenderPipeline.asset.useSRPBatcher = lastSRPBatchEnabled;
            }
        }

        private FilteringSettings GetFilterSettings()
        {
            if(filteringSettings.layerMask != Feature.layers)
                filteringSettings.layerMask = Feature.layers;

            return filteringSettings;
        }

        private void RestoreDrawSettings(ref RenderingData renderingData, CommandBuffer cmd)
        {
            var cameraData = renderingData.cameraData;

            if (Feature.overrideMainLightIndex && Feature.isRestoreMainLightIndexFinish)
            {
                OverrideMainLight(context, ref renderingData, sun);
            }


            if (Feature.overrideCamera)
            {
                RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix(), false);
            }
        }



        private DrawingSettings GetDrawSettings(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData, ref CameraData cameraData)
        {
            var cam = renderingData.cameraData.camera;

            var sortFlags = Feature.isOpaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
            var drawSettings = CreateDrawingSettings(shaderTagList, ref renderingData, sortFlags);
            drawSettings.overrideMaterial = Feature.overrideMaterial;
            drawSettings.overrideMaterialPassIndex = Feature.overrideMaterialPassIndex;
            drawSettings.fallbackMaterial = Feature.fallbackMaterial;

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
