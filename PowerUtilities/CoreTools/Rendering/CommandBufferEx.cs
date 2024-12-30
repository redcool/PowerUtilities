using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{

    public static class CommandBufferEx
    {
        public static readonly int
            _SourceTex = Shader.PropertyToID(nameof(_SourceTex)),
            _FinalSrcMode = Shader.PropertyToID(nameof(_FinalSrcMode)),
            _FinalDstMode = Shader.PropertyToID(nameof(_FinalDstMode))
        ;

        public static Dictionary<CommandBuffer, string> nameDict = new Dictionary<CommandBuffer, string>();

        public readonly static RenderTextureDescriptor defaultDescriptor = new RenderTextureDescriptor(1, 1, RenderTextureFormat.Default, 0, 0);

        public readonly static CommandBuffer defaultCmd = new CommandBuffer();

        static Material defaultBlitTriangleMat;
        public static Material GetDefaultBlitTriangleMat(this CommandBuffer cmd)
        {
            if (!defaultBlitTriangleMat)
                defaultBlitTriangleMat = new Material(Shader.Find("Hidden/Utils/CopyColor"));
            return defaultBlitTriangleMat;
        }

#if UNITY_2020
        public static void ClearRenderTarget(this CommandBuffer cmd,RTClearFlags clearFlags, Color backgroundColor, float depth = 1f, uint stencil = 0u)
        {
            cmd.ClearRenderTarget(clearFlags >= RTClearFlags.Depth, (clearFlags & RTClearFlags.Color) > 0, backgroundColor, depth);
        }
#endif
        /// <summary>
        /// get CommandBuffer name with cache
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string GetName(this CommandBuffer cmd)
            => DictionaryTools.Get(nameDict, cmd, cmd => cmd.name);

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
            {
#if UNITY_2020
                desc.depthBufferBits = 24;
#else
                desc.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
#endif
            }

            //targetIds.ForEach((item, id) =>
            foreach ( var item in targetIds )
            {
                Cmd.GetTemporaryRT(item, desc);
            };

        }

        public static void CreateTargets(this CommandBuffer cmd, Camera camera, List<RenderTargetInfo> targetInfos, float renderScale = 1, int samples = 1)
        {
            if (targetInfos == null || targetInfos.Count == 0)
                return;

            var desc = defaultDescriptor;
            desc.SetupColorDescriptor(camera, renderScale, false, samples);

            //targetInfos.ForEach((info, id) =>
            foreach (var info in targetInfos)
            {
                info.CreateRT(cmd, desc, camera);
            };
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

        /// <summary>
        /// Blit A triangle
        /// set _SourceTex,_FinalSrcMode,_FinalDstMode
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="renderingData"></param>
        /// <param name="sourceId"></param>
        /// <param name="colorTargetId"></param>
        /// <param name="mat"></param>
        /// <param name="pass"></param>
        /// <param name="camera"></param>
        /// <param name="finalSrcMode"></param>
        /// <param name="finalDstMode"></param>
        public static void BlitTriangle(this CommandBuffer cmd, RenderTargetIdentifier sourceId, RenderTargetIdentifier colorTargetId, Material mat, int pass,
            Camera camera = null, BlendMode finalSrcMode = BlendMode.One, BlendMode finalDstMode = BlendMode.Zero,
            ClearFlag clearFlags = ClearFlag.None,Color clearColor=default,RenderTargetIdentifier depthTargetId=default,
            bool isTryReplaceUrpTarget=true)
        {
#if UNITY_2022_1_OR_NEWER
            if (isTryReplaceUrpTarget)
            {
                var render = (UniversalRenderer)UniversalRenderPipeline.asset.scriptableRenderer;
                render.TryReplaceURPRTTarget(ref sourceId);
                render.TryReplaceURPRTTarget(ref colorTargetId);
                render.TryReplaceURPRTTarget(ref depthTargetId);
            }
#endif
            // set source
            cmd.SetGlobalTexture(_SourceTex, sourceId);

            // set blendmode
            cmd.SetGlobalFloat(_FinalSrcMode, (float)finalSrcMode);
            cmd.SetGlobalFloat(_FinalDstMode, (float)finalDstMode);

            // set render target
            var loadAction = finalDstMode == BlendMode.Zero ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;
            if (depthTargetId == default)
            {
                cmd.SetRenderTarget(colorTargetId, loadAction, RenderBufferStoreAction.Store);
            }
            else
            {
                cmd.SetRenderTarget(colorTargetId, loadAction, RenderBufferStoreAction.Store, depthTargetId, loadAction, RenderBufferStoreAction.Store);
            }

            // set clear
            if (clearFlags != ClearFlag.None)
            {
                cmd.ClearRenderTarget((RTClearFlags)clearFlags, clearColor, 1, 0);
            }
            // set viewport
            if (camera)
            {
                cmd.SetViewport(camera.pixelRect);
            }
            // draw trangle
            cmd.DrawProcedural(Matrix4x4.identity, mat, pass, MeshTopology.Triangles, 3);
        }

        public static void SetGlobalBool(this CommandBuffer cmd,int nameId, bool value)
        => cmd.SetGlobalInt(nameId, value ? 1 : 0);

        public static void SetGlobalBool(this CommandBuffer cmd, string name, bool value)
        => cmd.SetGlobalInt(name, value ? 1 : 0);
    }
}