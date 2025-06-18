using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    public class TargetTrackTrace : SRPFeature
    {
        [Header("TargetTrackTrace")]

        [Tooltip("cs calc fog map")]
        [LoadAsset("TrackTraceCS.compute")]
        public ComputeShader trackTraceCS;

        [Header("Target ")]
        [Tooltip("track target with tag")]
        public string targetTag = "TrackTarget";

        [Tooltip("target radius")]
        public float radius=2;

        [Header("BoxSceneFog")]
        [Tooltip("update boxSceneFog object")]
        public bool isFindBoxSceneFog;

        [EditorDisableGroup(targetPropName = nameof(isFindBoxSceneFog))]
        [Tooltip("boxSceneFog object tag")]
        public string boxSceneFogTag = "BoxSceneFog";


        [Tooltip("boxSceneFog's child named : MinPosTr")]
        public string minPosTrName = "MinPosTr";
        [Tooltip("boxSceneFog's child named : MaxPosTr")]
        public string maxPosTrName = "MaxPosTr";


        public Vector3 minPos, maxPos = new Vector3(100,100,100);

        [Header("Texture")]
        public RenderTexture trackRT;
        [Tooltip("shader set global texture")]
        public string trackTextureName = "_TrackTexture";
        public bool isClear;

        [Header("Debug")]
        [EditorDisableGroup]
        public Material boxSceneFogMat;
        [EditorDisableGroup]
        public Transform minPosTr, maxPosTr;


        public override ScriptableRenderPass GetPass()
        => new TargetTrackTracePass(this);
    }

    public class TargetTrackTracePass : SRPPass<TargetTrackTrace>
    {
        GameObject[] trackTargets;

        public TargetTrackTracePass(TargetTrackTrace feature) : base(feature)
        {
            //feature.cameraType = CameraType.Game;
        }

        public void TrySetupTrackRT(RenderTextureDescriptor desc)
        {
            var isRealloc = !Feature.trackRT && !RenderTextureTools.TryGetRT(Feature.trackTextureName, out Feature.trackRT) ;
            if (!isRealloc)
                isRealloc = Feature.trackRT.IsNeedAlloc(desc);

            if(isRealloc)
            {
                desc.sRGB = false;
                desc.enableRandomWrite = true;
                Feature.trackRT = new RenderTexture(desc);
            }
        }

        public override bool CanExecute()
        {
            var isValid = base.CanExecute() && Feature.trackTraceCS;

            trackTargets = GameObject.FindGameObjectsWithTag(Feature.targetTag);
            isValid = isValid && (trackTargets != null && trackTargets.Length > 0);
            return isValid;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            TrySetupBoxSceneFog();

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            TrySetupTrackRT(desc);

            var traceCS = Feature.trackTraceCS;

            if (CompareTools.IsTiggerOnce(ref Feature.isClear))
            {
                ClearRT(desc, traceCS,Feature.trackRT);
            }

            //------ main
            var kernel = traceCS.FindKernel("CSMain");
            traceCS.SetTexture(kernel, "_TrackTexture", Feature.trackRT);

            var targetPosArray = trackTargets.Select(target => (Vector4)target.transform.position).ToArray();
            traceCS.SetVectorArray("_TrackTargets", targetPosArray);
            traceCS.SetFloat("_TargetCount", targetPosArray.Length);
            traceCS.SetVector("_MinPos", Feature.minPos);
            traceCS.SetVector("_MaxPos", Feature.maxPos);
            traceCS.SetFloat("_Radius", Feature.radius);

            traceCS.DispatchKernel(kernel, desc.width, desc.height, 1);

            UpdateBoxSceneFog();

            Shader.SetGlobalTexture(Feature.trackTextureName, Feature.trackRT);
        }


        void TrySetupBoxSceneFog()
        {
            if (string.IsNullOrEmpty(Feature.boxSceneFogTag) || !Feature.isFindBoxSceneFog)
                return;

            var boxSceneFogGo = GameObject.FindGameObjectWithTag(Feature.boxSceneFogTag);
            if (!boxSceneFogGo)
                return;

            if (boxSceneFogGo.transform.childCount > 1)
            {
                var minPosTr = Feature.minPosTr = boxSceneFogGo.transform.Find(Feature.minPosTrName);
                var maxPosTr = Feature.maxPosTr = boxSceneFogGo.transform.Find(Feature.maxPosTrName);
                if (minPosTr)
                    Feature.minPos = minPosTr.position;
                if (maxPosTr)
                    Feature.maxPos = maxPosTr.position;
            }

            var r = boxSceneFogGo.GetComponent<Renderer>();
            if (!r)
                return;

            var mat = Feature.boxSceneFogMat = Application.isPlaying ? r.material : r.sharedMaterial;
            mat.SetVector("_MinWorldPos", Feature.minPos);
            mat.SetVector("_MaxWorldPos", Feature.maxPos);
        }

        void UpdateBoxSceneFog()
        {
            if (!Feature.boxSceneFogMat)
                return;

            Feature.boxSceneFogMat.SetTexture("_SceneFogMap", Feature.trackRT);
        }
        /// <summary>
        /// Clear rt
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="traceCS"></param>
        private static void ClearRT(RenderTextureDescriptor desc, ComputeShader traceCS,RenderTexture rt)
        {
            var clearKernel = traceCS.FindKernel("CSClear");
            traceCS.SetVector("_ClearColor", Color.white);
            traceCS.SetTexture(clearKernel, "_TrackTexture", rt);
            traceCS.DispatchKernel(clearKernel, desc.width, desc.height, 1);
        }
    }
}
