namespace PowerUtilities
{
    using Unity.Mathematics;
#if UNITY_EDITOR
#endif
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    /// <summary>
    /// DrawShadow pass
    /// </summary>
    public class DrawShadowPass : ScriptableRenderPass
    {
        public DrawShadowSettingSO settingSO;
        public int
            _BigShadowMap = Shader.PropertyToID(nameof(_BigShadowMap)),
            _BigShadowVP = Shader.PropertyToID(nameof(_BigShadowVP)),
            _BigShadowParams = Shader.PropertyToID(nameof(_BigShadowParams)),
            _CustomShadowBias = Shader.PropertyToID(nameof(_CustomShadowBias))
            ;

        RenderTexture bigShadowMap;

        public void SetupBigShadowMap(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var res = (int)settingSO.res;
            var isNeedAlloc = bigShadowMap == null || (bigShadowMap != null && bigShadowMap.width != res);
            if (!isNeedAlloc)
                return;

            if(bigShadowMap != null)
            {
                bigShadowMap.Destroy();
            }

            var desc = new RenderTextureDescriptor(res,res, GraphicsFormat.None, GraphicsFormatUtility.GetDepthStencilFormat(24, 0));
            desc.shadowSamplingMode = ShadowSamplingMode.CompareDepths;

            bigShadowMap = new RenderTexture(desc);
        }

        private void SetBigShadowMapTarget(CommandBuffer cmd)
        {
            cmd.SetRenderTarget(bigShadowMap);
            cmd.ClearRenderTarget(true, false, Color.clear);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!settingSO)
                return;

            ref var cameraData = ref renderingData.cameraData;
            var cmd = CommandBufferPool.Get();
            cmd.BeginSampleExecute(nameof(DrawShadow), ref context);

            SetupBigShadowMap(cmd, ref renderingData);
            SetBigShadowMapTarget(cmd);

            float4x4 view, proj;
            SetupVp(cmd, out view, out proj);


            // --- setup vp,viewport
            cmd.SetViewport(new Rect(0, 0, (int)settingSO.res, (int)settingSO.res));
            cmd.SetViewProjectionMatrices(view, proj);
            cmd.Execute(ref context);

            //----------- draw objects
            DrawingSettings drawSettings = SetupDrawSettings(ref renderingData);

            var renderQueueRange = settingSO.drawTransparents ? RenderQueueRange.all : RenderQueueRange.opaque;
            var filterSettings = new FilteringSettings(renderQueueRange, settingSO.layers);
            context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSettings);


            cmd.DisableScissorRect();
            cmd.SetViewProjectionMatrices(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());

            cmd.EndSampleExecute(nameof(DrawShadow), ref context);
            CommandBufferPool.Release(cmd);

            //====================== methods

            DrawingSettings SetupDrawSettings(ref RenderingData renderingData)
            {
                DrawingSettings drawSettings = default;
                if (!settingSO.isCallShadowCaster && settingSO.shadowMat)
                {
                    drawSettings = CreateDrawingSettings(RenderingTools.urpForwardShaderPassNames, ref renderingData, SortingCriteria.CommonOpaque);
                    drawSettings.overrideMaterial = settingSO.shadowMat;
                    drawSettings.overrideMaterialPassIndex = 0;
                }
                else
                {
                    drawSettings = CreateDrawingSettings(RenderingTools.shadowCaster, ref renderingData, SortingCriteria.CommonOpaque);
                }

                return drawSettings;
            }
        }

        private void SetupVp(CommandBuffer cmd, out float4x4 view, out float4x4 proj)
        {
            var rot = Quaternion.Euler(settingSO.rot);
            var forward = rot * Vector3.forward;

            view = float4x4.LookAt(settingSO.finalLightPos, settingSO.finalLightPos + forward, settingSO.up);
            view = math.fastinverse(view);

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
            {
                //view = math.mul(float4x4.Scale(1, 1, -1), view);
                view[0][2] *= -1;
                view[1][2] *= -1;
                view[2][2] *= -1;
                view[3][2] *= -1;
            }

            var aspect = 1;
            proj = float4x4.Ortho(settingSO.orthoSize * aspect * 2, settingSO.orthoSize * 2, settingSO.near, settingSO.far);
            //Debug.Log((Matrix4x4)proj+"\n"+GL.GetGPUProjectionMatrix(proj, false) +"\n"+GL.GetGPUProjectionMatrix(proj, true));
            //proj = GL.GetGPUProjectionMatrix(proj, false); //z from [-1,1] -> [0,1] and reverse z

            // same as GL.GetGPUProjectionMatrix
            if (SystemInfo.usesReversedZBuffer)
            {
                //proj[0][2] *= -1;
                //proj[1][2] *= -1;
                proj[2][2] = (proj[2][2] * -0.5f);
                proj[3][2] = (proj[3][2] + 0.5f);
            }


            CalcShadowTransform(cmd, view, proj, forward);
        }

        float4 CalcShadowBias(float4x4 proj)
        {
            var texelSize = 2f / proj[0][0] / (int)settingSO.res;
            var bias = new float4(-settingSO.shadowDepthBias, -settingSO.shadowNormalBias, 0, 0);
            bias *= texelSize;
            return bias;
        }

        /// <summary>
        /// send shader variables once.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="view"></param>
        /// <param name="proj"></param>
        /// <param name="forward"></param>
        private void CalcShadowTransform(CommandBuffer cmd, float4x4 view, float4x4 proj, float3 forward)
        {
            // reverse row2 again, dont need reverse  in shader
            if (SystemInfo.usesReversedZBuffer)
            {
                proj[0][2] *= -1;
                proj[1][2] *= -1;
                proj[2][2] *= -1;
                proj[3][2] *= -1;
            }
            var halfVec = new float3(.5f, .5f, .5f);

            var m2 = float4x4.Scale(halfVec);
            var m1 = float4x4.Translate(halfVec);
            var m = math.mul(m1, m2);
            var shadowBias = CalcShadowBias(proj);

            cmd.SetGlobalMatrix(_BigShadowVP, math.mul(m, math.mul(proj, view)));
            cmd.SetGlobalVector(ShaderPropertyIds._LightDirection, new float4(-forward, 0));
            cmd.SetGlobalVector(ShaderPropertyIds._ShadowBias, shadowBias);
            cmd.SetGlobalTexture(_BigShadowMap, bigShadowMap);
            cmd.SetGlobalVector(_CustomShadowBias, shadowBias);
        }

        /// <summary>
        /// send variables per frame
        /// </summary>
        public void UpdateShaderVariables()
        {
            var isDrawShadow = settingSO.layers != 0;

            if (!isDrawShadow)
            {
                Clear();
            }
            else
            {
                Shader.SetGlobalVector(_BigShadowParams, new Vector4(settingSO.shadowIntensity, 0));
            }
        }

        /// <summary>
        /// clear bigShadow
        /// </summary>
        public void Clear()
        {
            Shader.SetGlobalTexture(_BigShadowMap, DrawShadow.EmptyShadowMap);
            Shader.SetGlobalVector(_BigShadowParams, Vector4.zero);
        }
    }
}