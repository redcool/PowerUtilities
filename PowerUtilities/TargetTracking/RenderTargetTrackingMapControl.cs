using PowerUtilities.RenderFeatures;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static PowerUtilities.TargetTrackingTools;

namespace PowerUtilities
{
    /// <summary>
    /// Render target track to RT
    /// </summary>
    [ExecuteAlways]
    public class RenderTargetTrackingMapControl : MonoBehaviour
    {
        [Tooltip("update renderer's sceneFogMap")]
        public Renderer boxSceneFogRender;

        [Header("TargetTrackTrace")]

        [Tooltip("cs calc fog map")]
        [LoadAsset("TargetTrackingCS.compute")]
        public ComputeShader trackTraceCS;

        [Header("Target ")]
        [Tooltip("track all targets with tag")]
        public string targetTag = "TrackTarget";

        [Tooltip("target radius")]
        public float radius = 2;

        [Header("World Borders")]
        public Transform minPosTr;
        public Transform maxPosTr;

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
        [Range(0.1f,1)]
        public float renderScale=1;
        public RenderTexture trackRT;
        [Tooltip("shader set global texture")]
        public string trackTextureName = "_TrackTexture";
        [Tooltip("texture channel mask(1:on,0:off)")]
        public Vector4 trackTextureChannelMask = new Vector4(1, 0, 0, 0);
        public Color clearColor;
        public bool isClearRT;

        [Header("Debug")]
        [EditorDisableGroup]
        public Material boxSceneFogMat;


        RenderTextureDescriptor desc;

        public void OnEnable()
        {
            if (!boxSceneFogRender)
            {
                boxSceneFogRender = GetComponent<Renderer>();
            }

            desc = new RenderTextureDescriptor(1, 1, RenderTextureFormat.ARGB32, 0, 0);
            desc.SetupColorDescriptor(Camera.main, renderScale);
        }

        private void Update()
        {
            var trackTargets = GameObject.FindGameObjectsWithTag(targetTag);
            if (trackTargets.Length == 0)
                return;
            TrySetupRT(desc, trackTextureName, ref trackRT);

            FindBorderObjectsInChildren(transform, minPosTrName, ref minPosTr, ref minPos, maxPosTrName, ref minPosTr, ref minPos);

            var targetPosArray = trackTargets.Select(target => (Vector4)target.transform.position).ToArray();
            DispatchTrackingCS(trackTraceCS,targetPosArray);

            UpdateBoxSceneFogMaterial(boxSceneFogRender, ref boxSceneFogMat, minPos, maxPos, trackRT);
            Shader.SetGlobalTexture(trackTextureName, trackRT);
        }

        private void DispatchTrackingCS(ComputeShader cs,Vector4[] trackTargetPositions)
        {
            //-------- clear kernel
            if (CompareTools.IsTiggerOnce(ref isClearRT))
            {
                cs.DispatchKernel_CSClear("_TrackTexture", trackRT, clearColor: clearColor);
            }

            //------ main
            var kernel = cs.FindKernel("CSMain");
            cs.SetTexture(kernel, "_TrackTexture", trackRT);
            cs.SetVectorArray("_TrackTargets", trackTargetPositions);
            cs.SetFloat("_TargetCount", trackTargetPositions.Length);
            cs.SetVector("_MinPos", minPos);
            cs.SetVector("_MaxPos", maxPos);
            cs.SetFloat("_Radius", radius);
            cs.SetFloat("_ResumeSpeed", resumeSpeed);
            trackTextureChannelMask = math.clamp(trackTextureChannelMask, 0, 1);
            cs.SetVector("_TrackTextureChannelMask", trackTextureChannelMask);

            cs.DispatchKernel(kernel, desc.width, desc.height, 1);
        }
    }
}
