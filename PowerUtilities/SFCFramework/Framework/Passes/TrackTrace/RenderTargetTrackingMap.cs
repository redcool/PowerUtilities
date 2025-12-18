using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static PowerUtilities.TargetTrackingTools;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// Trace targets tracking map
    /// </summary>
    public class RenderTargetTrackingMap : SRPFeature
    {

        [Header("TargetTracking")]

        [Tooltip("cs calc fog map")]
        [LoadAsset("TargetTrackingCS.compute")]
        public ComputeShader trackCS;

        [Header("Target ")]
        [Tooltip("track all targets with tag")]
#if UNITY_EDITOR
        [StringListSearchable(type = typeof(TagManager), staticMemberName = nameof(TagManager.GetTags))]
#endif
        public string targetTag = "TrackTarget";

        [Tooltip("target radius")]
        public float radius = 2;

        [Header("BoxSceneFog")]
        [Tooltip("update boxSceneFog object")]
        public bool isFindBoxSceneFog;

        [Tooltip("boxSceneFog object tag")]
#if UNITY_EDITOR
        [StringListSearchable(type = typeof(TagManager), staticMemberName = nameof(TagManager.GetTags))]
#endif
        [EditorDisableGroup(targetPropName = nameof(isFindBoxSceneFog))]
        public string boxSceneFogTag = "BoxSceneFog";


        [Tooltip("boxSceneFog's child named : MinPosTr")]
        public string minPosTrName = "MinPosTr";
        [Tooltip("boxSceneFog's child named : MaxPosTr")]
        public string maxPosTrName = "MaxPosTr";
        public Vector3 minPos, maxPos = new Vector3(100, 100, 100);

        [Header("Time")]
        [Tooltip("track disappear speed")]
        [Range(0, 1)]
        public float resumeSpeed;

        [Header("Texture")]
        [Range(0.1f, 1)]
        public float renderScale = 1;
        [NonSerialized]public float lastRenderScale=1;

        public RenderTexture trackRT;
        [Tooltip("shader set global texture")]
        public string trackTextureName = "_TrackTexture";
        [Tooltip("texture channel mask(1:on,0:off)")]
        public Vector4 trackTextureChannelMask = new Vector4(1, 0, 0, 0);
        public Color clearColor;
        public bool isClearRT;

        [Header("Debug")]
        [EditorDisableGroup]
        [Tooltip("renderer's material(boxSceneFog shader)")]
        public Material boxSceneFogMat;
        //[EditorDisableGroup]
        public Transform minPosTr, maxPosTr;

        public override ScriptableRenderPass GetPass()
        => new RenderTargetTrackingMapPass(this);
    }

    public class RenderTargetTrackingMapPass : SRPPass<RenderTargetTrackingMap>
    {
        GameObject[] trackTargets;

        public RenderTargetTrackingMapPass(RenderTargetTrackingMap feature) : base(feature)
        {
            //feature.cameraType = CameraType.Game;
        }

        public override bool CanExecute()
        {
            var isValid = base.CanExecute() && Feature.trackCS;

            trackTargets = GameObject.FindGameObjectsWithTag(Feature.targetTag);
            isValid = isValid && (trackTargets != null && trackTargets.Length > 0);
            return isValid;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;

            if (CompareTools.CompareAndSet(ref Feature.lastRenderScale, Feature.renderScale))
            {
                desc.SetupColorDescriptor(camera, Feature.renderScale);
            }

            TrySetupRT(desc, Feature.trackTextureName, ref Feature.trackRT);

            TrySetupBoxSceneFog();

            var targetPosArray = trackTargets.Select(target => (Vector4)target.transform.position).ToArray();
            DispatchTrackingCS(Feature.trackCS, targetPosArray);

            UpdateBoxSceneFogMaterial(Feature.boxSceneFogMat, Feature.minPos, Feature.maxPos, Feature.trackRT);
            // global
            Shader.SetGlobalTexture(Feature.trackTextureName, Feature.trackRT);
        }

        private void DispatchTrackingCS(ComputeShader cs, Vector4[] targetPositions)
        {
            if (CompareTools.IsTriggerOnce(ref Feature.isClearRT))
            {
                ComputeShaderEx.DispatchKernel_CSClear(Feature.trackRT, clearColor: Feature.clearColor);
            }

            //------ call main
            var kernel = cs.FindKernel("CSMain");
            cs.SetTexture(kernel, "_TrackTexture", Feature.trackRT);

            cs.SetVectorArray("_TrackTargets", targetPositions);
            cs.SetFloat("_TargetCount", targetPositions.Length);
            cs.SetVector("_MinPos", Feature.minPos);
            cs.SetVector("_MaxPos", Feature.maxPos);
            cs.SetFloat("_Radius", Feature.radius);
            cs.SetFloat("_ResumeSpeed", Feature.resumeSpeed);
            Feature.trackTextureChannelMask = math.clamp(Feature.trackTextureChannelMask, 0, 1);
            cs.SetVector("_TrackTextureChannelMask", Feature.trackTextureChannelMask);

            cs.DispatchKernel(kernel, Feature.trackRT.width, Feature.trackRT.height, 1);
        }

        void TrySetupBoxSceneFog()
        {
            var isFound = FindBoxSceneFogObj(out var boxSceneFogGo);
            if (!isFound)
                return;

            FindBorderObjectsInChildren(boxSceneFogGo.transform,Feature.minPosTrName,ref Feature.minPosTr,ref Feature.minPos,Feature.maxPosTrName,ref Feature.maxPosTr,ref Feature.maxPos);

            var render = boxSceneFogGo.GetComponent<Renderer>();
            if (!render)
                return;
            Feature.boxSceneFogMat = Application.isPlaying ? render.material : render.sharedMaterial;
        }


        public bool FindBoxSceneFogObj(out GameObject boxSceneFogGo)
        {
            boxSceneFogGo = default;
            if (string.IsNullOrEmpty(Feature.boxSceneFogTag) || !Feature.isFindBoxSceneFog)
                return false;

            boxSceneFogGo = GameObject.FindGameObjectWithTag(Feature.boxSceneFogTag);
            return boxSceneFogGo;
        }

    }
}
