using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace PowerUtilities
{
    public class ComputeShaderDispatcher : MonoBehaviour
    {
        [HelpBox]
        public string helpBox = "Dispath compute shader with partial rect update";

        [LoadAsset("CS_RectUpdate.compute")]
        public ComputeShader cs;
        [Tooltip("Run kernel name")]
        public string kernelName;

        [Tooltip("Size for create rt")]
        public Vector3Int textureSize = new Vector3Int(512, 512, 1);

        [Tooltip("Update rect {xy:start,zw:size}")]
        public Vector4 UpdateRect = new Vector4(0,0,1,1);
        [Tooltip("Use double rt ,swap them per call")]
        public bool isDoubleRT;

        [Tooltip("buffers")]
        public RenderTexture rt0, rt1;


        [EditorButton(onClickCall = nameof(StartClear))]
        public bool lisClear;

        [EditorButton(onClickCall = nameof(StartDispatch))]
        public bool isStartDispatch;


        [EditorDisableGroup]
        public RenderTexture curRT;
        [EditorDisableGroup]
        public int frameCount;
        private void OnEnable()
        {
            StartClear();
        }
        private void OnDisable()
        {
            
        }

        private void Update()
        {
            StartDispatch();
        }
        public void TryCreateRTs()
        {
            if (!rt0)
            {
                rt0 = new RenderTexture(textureSize.x, textureSize.y, 0);
                rt0.enableRandomWrite = true;
            }

            if (isDoubleRT && !rt1)
            {
                rt1 = new RenderTexture(textureSize.x, textureSize.y, 0);
                rt1.enableRandomWrite = true;
            }
        }
        public RenderTexture GetSwappedResultRT(RenderTexture rt0, RenderTexture rt1)
        {
            if (rt0 && rt1)
                return frameCount++ % 2 == 0 ? rt0 : rt1;
            return rt0 ? rt0 : rt1;
        }

        public void StartClear()
        {
            if (!cs.CanExecute())
                return;
            TryCreateRTs();

            var clearId = cs.FindKernel("CSClear");
            cs.SetTexture(clearId, "_ResultTex", rt0);
            if (isDoubleRT)
            {
                cs.EnableKeyword("DOUBLE_RT");
                cs.SetTexture(clearId, "_ResultTex1", rt1);
            }
            cs.DispatchKernel(clearId, rt0.width, rt0.height, rt0.depth);
        }

        public void StartDispatch()
        {
            if (!cs.CanExecute() || string.IsNullOrEmpty(kernelName))
                return;

            TryCreateRTs();

            var kernelId = cs.FindKernel(kernelName);
            var resultRT = GetSwappedResultRT(rt0, rt1);
            var texSize = resultRT? resultRT.GetSize() : new Vector3Int(textureSize.x, textureSize.y, textureSize.z);
            
            cs.SetTextureWithSize(kernelId, "_ResultTex", resultRT);

            UpdateRect = math.clamp(UpdateRect, 0.0f, 1.0f);
            cs.DispatchKernel(kernelId, textureSize.x, textureSize.y, textureSize.z, UpdateRect);

            curRT = resultRT;
        }

    }
}
