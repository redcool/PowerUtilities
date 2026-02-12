using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace PowerUtilities
{
    public class ComputeShaderDispatcher : MonoBehaviour
    {
        public enum ResultMode
        {
            Texture,
            Buffer,
        }

        [HelpBox]
        public string helpBox = "Dispath compute shader with partial rect update";

        [LoadAsset("CS_RectUpdate.compute")]
        public ComputeShader cs;

        [Tooltip("Kernel name,StartDispatch run it")]
        [EditorNotNull]
        public string kernelName;

        [Header("Result Mode")]
        public ResultMode resultMode = ResultMode.Texture;

        [Header("Result Texutre")]
        [Tooltip("Size for create rt or buffer")]
        public Vector3Int textureSize = new Vector3Int(512, 512, 1);

        [Tooltip("Update rect {xy:start,zw:size}")]
        public Vector4 updateRect = new Vector4(0,0,1,1);
        [Tooltip("Use double rt ,swap them per call")]
        public bool isDoubleRT;

        [Tooltip("buffers")]
        public RenderTexture rt0, rt1;
        [Tooltip("texture name in shader")]
        public string resultTextureName = "_ResultTex";

        [Header("Result Buffer")]
        public int bufferSize = 10;
        GraphicsBuffer buffer = null;
        [Tooltip("buffer name in shader")]
        public string resultBufferName = "_ResultBuffer";

        [Header("Shader values")]
        public List<ShaderValue<float>> floatValues = new();
        public List<ShaderValue<float4>> float4Values = new();
        public List<ShaderValue<float4x4>> float4x4Values = new();
        public List<ShaderValue<Texture>> textureValues = new();

        [EditorBox("Buttons", "lisClear,isStartDispatch,isShowBuffer",isShowFoldout = true)]
        [EditorButton(onClickCall = nameof(StartClear))]
        public bool lisClear;

        [EditorButton(onClickCall = nameof(StartDispatch))]
        [HideInInspector]
        public bool isStartDispatch;

        [EditorButton(onClickCall = nameof(ShowBuffer))]
        [HideInInspector]
        public bool isShowBuffer;


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
            cs.DispatchKernel(clearId, rt0.width, rt0.height, rt0.GetDepth());
        }

        public void StartDispatch()
        {
            if (!cs.CanExecute() || string.IsNullOrEmpty(kernelName))
            {
                Debug.Log("can't execute compute shader or kernel name is empty");
                return;
            }

            var kernelId = cs.FindKernel(kernelName);

            UpdateStates(kernelId);

            if (resultMode == ResultMode.Texture)
            {
                DispatchKernelTextureResult(kernelId);
            }
            else
            {
                DispatchKernelBufferResult(kernelId);
            }
        }

        void ShowBuffer()
        {
            if (!buffer.IsValidSafe())
            {
                Debug.Log("buffer not exist");
                return;
            }
            var arr = new float4[buffer.count];
            buffer.GetData(arr);
            Debug.Log(string.Join("\n", arr));
        }

        private void DispatchKernelBufferResult(int kernelId)
        {
            cs.EnableKeyword("RESULT_BUFFER");
            // create buffer when result mode is buffer, and set buffer
            GraphicsBufferTools.TryCreateBuffer(ref buffer, GraphicsBuffer.Target.Structured, bufferSize, Marshal.SizeOf<float4>());
            cs.SetBuffer(kernelId, "_ResultBuffer", buffer);
            cs.DispatchKernel(kernelId, bufferSize, 1, 1);

            Shader.SetGlobalBuffer(resultBufferName, buffer);
        }

        private void DispatchKernelTextureResult(int kernelId)
        {
            TryCreateRTs();

            var resultRT = GetSwappedResultRT(rt0, rt1);
            var texSize = resultRT ? resultRT.GetSize() : new Vector3Int(textureSize.x, textureSize.y, textureSize.z);
            cs.SetTextureWithSize(kernelId, "_ResultTex", resultRT);

            curRT = resultRT;

            cs.DisableKeyword("RESULT_BUFFER");
            updateRect = math.clamp(updateRect, 0.0f, 1.0f);
            cs.DispatchKernel(kernelId, textureSize.x, textureSize.y, textureSize.z, updateRect);

            Shader.SetGlobalTexture(resultTextureName, resultRT);
        }

        private void UpdateStates(int kernelId)
        {
            foreach (var value in floatValues)
            {
                if (value.IsValid)
                    cs.SetFloat(value.name, value.value);
            }
            foreach (var value in float4Values)
            {
                if (value.IsValid)
                    cs.SetVector(value.name, value.value);
            }
            foreach (var value in float4x4Values)
            {
                if (value.IsValid)
                    cs.SetMatrix(value.name, value.value);
            }
            foreach (var value in textureValues)
            {
                if (value.IsValid)
                    cs.SetTexture(kernelId, value.name, value.value);
            }
        }
    }
}
