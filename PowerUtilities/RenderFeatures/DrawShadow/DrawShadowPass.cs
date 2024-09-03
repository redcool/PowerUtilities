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
            _BigShadowOn = Shader.PropertyToID(nameof(_BigShadowOn)),
            _CustomShadowBias = Shader.PropertyToID(nameof(_CustomShadowBias))
            ;

        RenderTexture bigShadowMap;

        CommandBuffer cmd;

        /// <summary>
        /// when stop play in Editor, bigShadowMap will be destroy
        /// need call Execute again
        /// </summary>
        /// <returns></returns>
        public bool IsBigShadowMapValid()
        {
            return bigShadowMap;
        }

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
#if UNITY_2020
            var desc = new RenderTextureDescriptor(res, res, RenderTextureFormat.Depth, 24);
#else
            var desc = new RenderTextureDescriptor(res,res, GraphicsFormat.None, GraphicsFormatUtility.GetDepthStencilFormat(24, 0));
#endif
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
            
            if (cmd == null)
                cmd = new CommandBuffer { name = nameof(DrawShadow) };


            cmd.BeginSampleExecute(nameof(DrawShadow), ref context);

            SetupBigShadowMap(cmd, ref renderingData);
            SetBigShadowMapTarget(cmd);
            DrawShadows(context,ref renderingData);

            cmd.EndSampleExecute(nameof(DrawShadow), ref context);
        }

        public void DrawShadows(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            /*** 
             * rowCount {1,2,4,8}, tileCount = rowCount * rowCount
             * 
             * rowId,colId
             *  2 3
             *  0 1
             * */
            var rowCount = 1;
            var tileRes = (int)settingSO.res / rowCount;
            var tileStart = new Vector2Int(0, 0);
            var rowId = tileStart.x / tileRes;
            var colId = tileStart.y / tileRes;

            /**
             Setup view,proj,

             */
            float4x4 view, proj;
            SetupVp(cmd, out view, out proj, out var lightForward);


            DrawShadowRenderers(ref renderingData);

            /**
             transform (proj,view) to shadowMapTexture' space[0,tileRes]
             */
            var atlasMat = TransformVPToShadowTextureSpace(ref view, ref proj, rowCount, rowId, colId);
            SendBigShadowVariables(cmd, atlasMat, proj, lightForward);

            //====================== inner methods

            void DrawShadowRenderers(ref RenderingData renderingData)
            {
                ref var cameraData = ref renderingData.cameraData;

                // --- setup vp,viewport
                cmd.SetViewport(new Rect(tileStart.x, tileStart.y, tileRes, tileRes));
                cmd.SetViewProjectionMatrices(view, proj);
                cmd.Execute(ref context);

                //----------- draw objects
                DrawingSettings drawSettings = SetupDrawSettings(ref renderingData);

                var renderQueueRange = settingSO.drawTransparents ? RenderQueueRange.all : RenderQueueRange.opaque;
                var filterSettings = new FilteringSettings(renderQueueRange, settingSO.layers,settingSO.renderingLayerMask);
                context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSettings);

                cmd.DisableScissorRect();
                cmd.SetViewProjectionMatrices(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());

            }

            DrawingSettings SetupDrawSettings(ref RenderingData renderingData)
            {
                DrawingSettings drawSettings = default;
                if (!settingSO.isCallShadowCaster && settingSO.shadowMat)
                {
                    drawSettings = CreateDrawingSettings(ShaderTagIdEx.urpForwardShaderPassNames, ref renderingData, SortingCriteria.CommonOpaque);
                    drawSettings.overrideMaterial = settingSO.shadowMat;
                    drawSettings.overrideMaterialPassIndex = 0;
                }
                else
                {
                    drawSettings = CreateDrawingSettings(ShaderTagIdEx.shadowCaster, ref renderingData, SortingCriteria.CommonOpaque);
                }

                return drawSettings;
            }
        }

        private void SetupVp(CommandBuffer cmd, out float4x4 view, out float4x4 proj,out Vector3 lightForward)
        {
            var rot = Quaternion.Euler(settingSO.rot);
            lightForward = rot * Vector3.forward;

            view = float4x4.LookAt(settingSO.finalLightPos, settingSO.finalLightPos + lightForward, settingSO.up);

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
            {
                //view = math.mul(float4x4.Scale(1, 1, -1), view);
                view.c2 *= -1;
            }
            view = math.fastinverse(view);


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


           
        }

        float4 CalcShadowBias(float4x4 proj)
        {
            var texelSize = 2f / proj[0][0] / (int)settingSO.res;
            var bias = new float4(-settingSO.shadowDepthBias, -settingSO.shadowNormalBias, 0, 0);
            bias *= texelSize;
            return bias;
        }

        float4x4 TransformVPToShadowTextureSpace(ref float4x4 view, ref float4x4 proj, int rowCount = 1, int rowId = 0, int colId = 0)
        {
            // reverse row2 again, dont need reverse  in shader
            if (SystemInfo.usesReversedZBuffer)
            {
                //proj[0][2] *= -1;
                //proj[1][2] *= -1;
                proj[2][2] *= -1;
                proj[3][2] *= -1;
            }
            /*
             
             [-w,w] -> [0,1] or tile tileRes in atlasTextureSpace
             */
            var tileUVSize = 0.5f;
            var uvScale = Mathf.Pow(tileUVSize, rowCount);
            var m = new float4x4(
                uvScale, 0, 0, uvScale + tileUVSize * rowId,
                0, uvScale, 0, uvScale + tileUVSize * colId,
                0, 0, .5f, 0.5f,
                0, 0, 0, 1
                );

            return math.mul(m, math.mul(proj, view));
        }

        /// <summary>
        /// send shader variables once.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="view"></param>
        /// <param name="proj"></param>
        /// <param name="forward"></param>
        private void SendBigShadowVariables(CommandBuffer cmd, float4x4 atlasVPMat, float4x4 proj, float3 forward)
        {
            cmd.SetGlobalMatrix(_BigShadowVP, atlasVPMat);

            var shadowBias = CalcShadowBias(proj);
            cmd.SetGlobalVector(ShaderPropertyIds._ShadowBias, shadowBias);
            cmd.SetGlobalVector(ShaderPropertyIds._LightDirection, new float4(-forward, 0));
            cmd.SetGlobalTexture(_BigShadowMap, bigShadowMap);
            cmd.SetGlobalInt(_BigShadowOn, 1);
        }

        /// <summary>
        /// send variables per frame
        /// </summary>
        public void UpdateShaderVariables()
        {
            Shader.SetGlobalVector(_BigShadowParams, new Vector4(settingSO.shadowIntensity, 0));
        }

        /// <summary>
        /// clear bigShadow
        /// </summary>
        public void Clear()
        {
            Shader.SetGlobalTexture(_BigShadowMap, RenderingTools.EmptyShadowMap);
            Shader.SetGlobalVector(_BigShadowParams, Vector4.zero);
            Shader.SetGlobalInt(_BigShadowOn, 0);
        }
    }
}