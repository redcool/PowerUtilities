using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace PowerUtilities
{

    public static class CommandBufferEx
    {
        public static readonly int
            _SourceTex = Shader.PropertyToID(nameof(_SourceTex)),
            _FinalSrcMode = Shader.PropertyToID(nameof(_FinalSrcMode)),
            _FinalDstMode = Shader.PropertyToID(nameof(_FinalDstMode))
        ;

        public readonly static RenderTextureDescriptor defaultDescriptor = new RenderTextureDescriptor(1, 1, RenderTextureFormat.Default, 0, 0);

        public static void ClearRenderTarget(this CommandBuffer cmd, Camera camera, float depth = 1, uint stencil = 0)
        {
            var isClearDepth = camera.clearFlags <= CameraClearFlags.Depth;
            var isClearColor = camera.clearFlags <= CameraClearFlags.Color;// ** if condition set equals , mrt color will not clear

            var backColor = camera.clearFlags == CameraClearFlags.Color || camera.cameraType == CameraType.SceneView?
                camera.backgroundColor.linear : Color.clear;

            var flags = RTClearFlags.None;
            if (isClearColor)
                flags |= RTClearFlags.Color;
            if (isClearDepth)
                flags |= RTClearFlags.DepthStencil;
            cmd.ClearRenderTarget(flags, backColor, depth, stencil);

            //cmd.ClearRenderTarget(camera.clearFlags <= CameraClearFlags.Depth,
            //camera.clearFlags == CameraClearFlags.color,
            //camera.clearFlags == CameraClearFlags.color ? camera.backgroundColor : color.clear
            //);
        }

        public static void ClearRenderTarget(this CommandBuffer cmd,bool isClearColor,Color backColor, bool isClearDepth,float depth,bool isClearStencil, uint stencil)
        {
            var flags = RTClearFlags.None;
            if (isClearColor)
                flags |= RTClearFlags.Color;
            if (isClearDepth)
                flags |= RTClearFlags.Depth;
            if (isClearStencil)
                flags |= RTClearFlags.Stencil;

            cmd.ClearRenderTarget(flags, backColor, depth, stencil);
        }

        public static void CreateTargets(this CommandBuffer Cmd, Camera camera, int[] targetIds, float renderScale = 1, bool hasDepth = false, bool isHdr = false,int samples=1)
        {
            if (targetIds == null || targetIds.Length == 0)
                return;

            var desc = defaultDescriptor;
            desc.SetupColorDescriptor(camera, renderScale, isHdr, samples);

            if (hasDepth)
                desc.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;

            targetIds.ForEach((item, id) =>
            {

                Cmd.GetTemporaryRT(item, desc);
            });

        }

        public static void CreateTargets(this CommandBuffer cmd, Camera camera, List<RenderTargetInfo> targetInfos, float renderScale = 1, int samples = 1)
        {
            if (targetInfos == null || targetInfos.Count == 0)
                return;

            var desc = defaultDescriptor;
            desc.SetupColorDescriptor(camera, renderScale, false, samples);

            targetInfos.ForEach((info, id) =>
            {
                if (! info.IsValid())
                    return;

                desc.colorFormat = info.isHdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
                desc.graphicsFormat = info.format;

                if (info.hasDepthBuffer)
                    desc.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;

                cmd.GetTemporaryRT(info.GetHash(), desc);
            });
        }

        public static void CreateDepthTargets(this CommandBuffer Cmd, Camera camera, int[] targetIds, float renderScale = 1, int samples = 1)
        {
            if (targetIds == null || targetIds.Length == 0)
                return;

            var desc = defaultDescriptor;
            desc.SetupDepthDescriptor(camera, renderScale);
            desc.msaaSamples = samples;

            foreach (var item in targetIds)
            {
                Cmd.GetTemporaryRT(item, desc);
            }
        }
        public static void CreateDepthTarget(this CommandBuffer cmd, Camera camera, int targetId, float renderScale = 1, int samples = 1)
        {
            var desc = defaultDescriptor;
            desc.SetupDepthDescriptor(camera, renderScale);
            desc.msaaSamples = samples;
            cmd.GetTemporaryRT(targetId, desc);
        }

        /// <summary>
        /// Execute cmd then Clear it
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="context"></param>
        public static void Execute(this CommandBuffer cmd, ref ScriptableRenderContext context)
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        public static void BeginSampleExecute(this CommandBuffer cmd, string sampleName, ref ScriptableRenderContext context)
        {
            cmd.name = sampleName;
            cmd.BeginSample(sampleName);
            cmd.Execute(ref context);
        }
        public static void EndSampleExecute(this CommandBuffer cmd, string sampleName, ref ScriptableRenderContext context)
        {
            cmd.name = sampleName;
            cmd.EndSample(sampleName);
            cmd.Execute(ref context);
        }

        public static void SetShaderKeywords(this CommandBuffer cmd, bool isOn, params string[] keywords)
        {
            foreach (var item in keywords)
            {
                if (Shader.IsKeywordEnabled(item) == isOn)
                    continue;

                if (isOn)
                    cmd.EnableShaderKeyword(item);
                else
                    cmd.DisableShaderKeyword(item);
            }
        }

        public static void SetComputeShaderKeywords(this CommandBuffer cmd,ComputeShader cs,bool isOn,params string[] keywords)
        {
            foreach (var item in keywords)
            {
                if (cs.IsKeywordEnabled(item) == isOn)
                    continue;

                if (isOn)
                    cs.EnableKeyword(item);
                else
                    cs.DisableKeyword(item);
            }
        }

        public static void BlitTriangle(this CommandBuffer cmd, RenderTargetIdentifier sourceId, RenderTargetIdentifier targetId, Material mat, int pass, Camera camera = null, BlendMode finalSrcMode = BlendMode.One, BlendMode finalDstMode = BlendMode.Zero)
        {
            cmd.SetGlobalTexture(_SourceTex, sourceId);

            cmd.SetGlobalFloat(_FinalSrcMode, (float)finalSrcMode);
            cmd.SetGlobalFloat(_FinalDstMode, (float)finalDstMode);

            var loadAction = finalDstMode == BlendMode.Zero ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;
            cmd.SetRenderTarget(targetId, loadAction, RenderBufferStoreAction.Store);

            if (camera)
            {
                cmd.SetViewport(camera.pixelRect);
            }
            cmd.DrawProcedural(Matrix4x4.identity, mat, pass, MeshTopology.Triangles, 3);
        }

    }
}