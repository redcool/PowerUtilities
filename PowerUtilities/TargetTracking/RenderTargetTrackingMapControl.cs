using PowerUtilities.RenderFeatures;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        [LoadAsset("TrackTraceCS.compute")]
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

            var cam = Camera.main;
            desc = new RenderTextureDescriptor(1, 1, RenderTextureFormat.ARGB32, 0, 0);
            desc.SetupColorDescriptor(Camera.main, renderScale);
        }

        private void Update()
        {
            var trackTargets = GameObject.FindGameObjectsWithTag(targetTag);
            if (trackTargets.Length == 0)
                return;
            
            TrySetupRT(desc);

            FindBorderObjects();

            var traceCS = trackTraceCS;

            if (CompareTools.IsTiggerOnce(ref isClearRT))
            {
                traceCS.ClearRT("_TrackTexture", trackRT, clearColor: clearColor);
            }

            //------ main
            var kernel = traceCS.FindKernel("CSMain");
            traceCS.SetTexture(kernel, "_TrackTexture", trackRT);

            var targetPosArray = trackTargets.Select(target => (Vector4)target.transform.position).ToArray();
            traceCS.SetVectorArray("_TrackTargets", targetPosArray);
            traceCS.SetFloat("_TargetCount", targetPosArray.Length);
            traceCS.SetVector("_MinPos", minPos);
            traceCS.SetVector("_MaxPos", maxPos);
            traceCS.SetFloat("_Radius", radius);
            traceCS.SetFloat("_ResumeSpeed", resumeSpeed);

            traceCS.DispatchKernel(kernel, desc.width, desc.height, 1);

            UpdateBoxSceneFog();
            Shader.SetGlobalTexture(trackTextureName, trackRT);
        }

        void TrySetupRT(RenderTextureDescriptor desc)
        {
            var isRealloc = !trackRT && !RenderTextureTools.TryGetRT(trackTextureName, out trackRT);
            if (!isRealloc)
                isRealloc = trackRT.IsNeedAlloc(desc);

            if (isRealloc)
            {
                desc.sRGB = false;
                desc.enableRandomWrite = true;
                trackRT = new RenderTexture(desc);
            }
        }

        void UpdateBoxSceneFog()
        {
            if (!boxSceneFogRender)
                return;

            var mat = boxSceneFogMat = Application.isPlaying ? boxSceneFogRender.material : boxSceneFogRender.sharedMaterial;
            mat.SetVector("_MinWorldPos", minPos);
            mat.SetVector("_MaxWorldPos", maxPos);
            mat.SetTexture("_SceneFogMap", trackRT);
        }

        private void FindBorderObjects()
        {
            if (!boxSceneFogRender || boxSceneFogRender.transform.childCount < 2)
                return;
            RenderTargetTrackingMapPass.FindChildObj(boxSceneFogRender.transform, minPosTrName, ref minPosTr, ref minPos);
            RenderTargetTrackingMapPass.FindChildObj(boxSceneFogRender.transform, maxPosTrName, ref maxPosTr, ref maxPos);
        }
    }
}
