namespace PowerUtilities
{
    using System;
    using Unity.Mathematics;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class DrawShadow : ScriptableRendererFeature
    {
        static RenderTexture emptyShadowMap;
        /// <summary>
        /// Defulat empty shadowmap,
        /// Texture2D.whiteTexture,some device will crash.
        /// </summary>
        public static RenderTexture EmptyShadowMap
        {
            get
            {
                if(emptyShadowMap == null)
                    emptyShadowMap = ShadowUtils.GetTemporaryShadowTexture(1, 1, 16);
                return emptyShadowMap;
            }
        }

        class DrawShadowPass : ScriptableRenderPass
        {
            Settings settings;
            public int
                _BigShadowMap = Shader.PropertyToID(nameof(_BigShadowMap)),
                _BigShadowVP = Shader.PropertyToID(nameof(_BigShadowVP)),
                _BigShadowParams = Shader.PropertyToID(nameof(_BigShadowParams)),
                _BigShadowOn = Shader.PropertyToID(nameof(_BigShadowOn))
                ;

            RenderTexture bigShadowMap;

            public DrawShadowPass(Settings settings)
            {
                this.settings = settings;
            }


            public void SetupBigShadowMap(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var desc = new RenderTextureDescriptor((int)settings.res, (int)settings.res, GraphicsFormat.None, GraphicsFormatUtility.GetDepthStencilFormat(24, 0));
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
                ref var cameraData = ref renderingData.cameraData;
                var cmd = CommandBufferPool.Get();
                cmd.BeginSampleExecute(nameof(DrawShadow), ref context);

                SetupBigShadowMap(cmd, ref renderingData);
                SetBigShadowMapTarget(cmd);

                float4x4 view, proj;
                SetupVp(cmd, out view, out proj);


                // --- setup vp,viewport
                cmd.SetViewport(new Rect(0, 0, (int)settings.res, (int)settings.res));
                cmd.SetViewProjectionMatrices(view, proj);
                cmd.Execute(ref context);

                //----------- draw objects
                DrawingSettings drawSettings = SetupDrawSettings(ref renderingData);

                var renderQueueRange = settings.drawTransparents ? RenderQueueRange.all : RenderQueueRange.opaque;
                var filterSettings = new FilteringSettings(renderQueueRange, settings.layers);
                context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSettings);


                cmd.DisableScissorRect();
                cmd.SetViewProjectionMatrices(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());

                cmd.EndSampleExecute(nameof(DrawShadow), ref context);
                CommandBufferPool.Release(cmd);

                //====================== methods

                DrawingSettings SetupDrawSettings(ref RenderingData renderingData)
                {
                    DrawingSettings drawSettings = default;
                    if (!settings.isCallShadowCaster && settings.shadowMat)
                    {
                        drawSettings = CreateDrawingSettings(RenderingTools.urpForwardShaderPassNames, ref renderingData, SortingCriteria.CommonOpaque);
                        drawSettings.overrideMaterial = settings.shadowMat;
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
                var rot = Quaternion.Euler(settings.rot);
                var forward = rot * Vector3.forward;

                view = float4x4.LookAt(settings.finalLightPos, settings.finalLightPos + forward, settings.up);
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
                proj = float4x4.Ortho(settings.orthoSize * aspect * 2, settings.orthoSize * 2, settings.near, settings.far);
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
                var texelSize = 2f / proj[0][0] / (int)settings.res;
                var bias = new float4(-settings.shadowDepthBias, -settings.shadowNormalBias, 0, 0);
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
                cmd.SetGlobalVector(ShaderPropertyIds._ShadowBias,shadowBias);
                cmd.SetGlobalTexture(_BigShadowMap, bigShadowMap);
                cmd.SetGlobalVector("_CustomShadowBias",shadowBias);
            }

            /// <summary>
            /// send variables per frame
            /// </summary>
            public void UpdateShaderVariables()
            {
                Shader.SetGlobalVector(_BigShadowParams, new Vector4(settings.shadowIntensity, 0));
                Shader.SetGlobalFloat(_BigShadowOn, settings.shadowIntensity);
            }

            public void Clear()
            {
                Shader.SetGlobalTexture(_BigShadowMap, DrawShadow.EmptyShadowMap);
            }
        }

        DrawShadowPass drawShadowPass;

        [Serializable]
        public class Settings
        {
            [EditorGroup("ShadowMapOptions",true)]
            public TextureResolution res = TextureResolution.x512;

            [EditorGroup("ShadowMapOptions")]
            [Tooltip("call renderer's ShadowCaster pass, more batch than use override material")]
            public bool isCallShadowCaster;

            [EditorGroup("ShadowMapOptions")]
            [Tooltip("use override material,dont use ShadowCaster,will cause more srp batches!")]
            [LoadAsset("BigShadowCasterMat.mat")]
            public Material shadowMat;

            [EditorGroup("ShadowMapOptions")]
            public LayerMask layers = 0;

            [EditorGroup("ShadowMapOptions")]
            public bool drawTransparents;

            [EditorGroup("Light Camera",true)]
            [Tooltip("Find by tag,disable DrawShadow when lightTransform not found")]
            public bool isUseLightTransform = true;

            [EditorGroup("Light Camera")]
            public string lightTag = "BigShadowLight";

            [EditorGroup("Light Camera")]
            public Vector3 pos, rot, up = Vector3.up;

            [EditorGroup("Light Camera")]
            [Tooltip("light position'sy in world space")]
            public float lightHeight = 10;


            [EditorGroup("Light Camera")]
            [Tooltip("half of height")]
            public float orthoSize = 20;

            [EditorGroup("Light Camera")]
            [Tooltip("near clip plane ")]
            public float near = 0.3f;

            [EditorGroup("Light Camera")]
            [Tooltip("far clip plane ")]
            public float far = 100;

            [EditorGroup("Shadow", true)]
            [Min(0)] public float shadowDepthBias = 1;

            [EditorGroup("Shadow")]
            [Min(0)] public float shadowNormalBias = 1;

            [EditorGroup("Shadow")]
            [Range(0, 1)] public float shadowIntensity = 1;

            [Header("Render control")]
            [Tooltip("draw shadow frame then stop,when isAutoRendering = false")]
            [EditorButton]public bool isStepRender = true;

            [Tooltip("draw shadow per frame")]
            public bool isAutoRendering;
            [Tooltip("Follow camera, when distance > maxDistance, draw shadow once,<=0 disable Follow Camera")]
            public float maxDistance = -1;

            // light position finally.
            [HideInInspector] public Vector3 finalLightPos;

            [Tooltip("false will set _BigShadowMap white tex")]
            [EditorButton] public bool isClearShadowMap;
        }
        public Settings settings = new Settings();

        [Header("Debug")]
        [EditorReadonly]
        public GameObject lightObj;
        [EditorReadonly]
        public float currentDistance;
        

        int renderCount = 0;

        /// <inheritdoc/>
        public override void Create()
        {
            drawShadowPass = new DrawShadowPass(settings);

            // Configures where the render pass should be injected.
            drawShadowPass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
        }

        /// <summary>
        /// Can call DrawShadowPass?
        /// 
        /// Camera : mainCamera or sceneViewCamera
        /// stepRender is true,
        /// autoRendering is true
        /// 
        /// </summary>
        /// <param name="cameraData"></param>
        /// <returns></returns>
        bool CanExecute(CameraData cameraData)
        {
            var isMainCamera = cameraData.camera.IsMainCamera();
            var isSceneCamera = cameraData.camera.IsSceneViewCamera();

            if (!isMainCamera && !isSceneCamera)
                return false;

            // mainCamera follow it,sceneViewCamera dont do follow
            var isExceedMaxDistance = false;
            if (isMainCamera)
                isExceedMaxDistance = IsExceedMaxDistanceAndSaveLightPos(cameraData, out currentDistance, ref settings.finalLightPos);

            var isStepRender = settings.isStepRender || isExceedMaxDistance;
            if (isStepRender)
            {
                settings.isStepRender = false;
                renderCount++;
            }

            return settings.isAutoRendering || isStepRender;

            //============== methods
            bool IsExceedMaxDistanceAndSaveLightPos(CameraData cameraData,out float curDistance,ref Vector3 finalLightPos)
            {
                // dont follow camera
                if (settings.maxDistance <= 0)
                {
                    finalLightPos = settings.pos;
                    curDistance = -1;
                    return false;
                }

                // follow camera

                var cameraPos = cameraData.camera.transform.position;
                cameraPos.y = finalLightPos.y = settings.lightHeight;

                var dir = cameraPos - finalLightPos;
                curDistance = dir.magnitude;

                var isExceedMaxDistance = curDistance > settings.maxDistance;
                if (isExceedMaxDistance)
                {
                    finalLightPos = cameraPos;
                }

                return isExceedMaxDistance;
            }
        }

        void DrawLightGizmos(ref CameraData cameraData)
        {
            var h = settings.orthoSize;
            var w = h;
            var n = settings.near;
            var f = settings.far;

            var p0 = new Vector3(-w, -h, n);
            var p1 = new Vector3(-w, h, n);
            var p2 = new Vector3(w, h, n);
            var p3 = new Vector3(w, -h, n);

            var p4 = new Vector3(-w, -h, f);
            var p5 = new Vector3(-w, h, f);
            var p6 = new Vector3(w, h, f);
            var p7 = new Vector3(w, -h, f);

            var rot = Quaternion.Euler(settings.rot);

            var vertices = new[] { p0, p1, p2, p3, p4, p5, p6, p7 };
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = (settings.finalLightPos + rot * vertices[i]);
            }
            // view frustum
            DebugTools.DrawLineCube(vertices);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            drawShadowPass.UpdateShaderVariables();
            if(settings.isClearShadowMap)
            {
                settings.isClearShadowMap = false;
                drawShadowPass.Clear();
            }

            ref var cameraData = ref renderingData.cameraData;
            TrySetupLightCameraInfo();

            DrawLightGizmos(ref cameraData);

            var isUseLightObjButNotExists = settings.isUseLightTransform && !string.IsNullOrEmpty(settings.lightTag) & !lightObj;

            if (!CanExecute(cameraData) && isUseLightObjButNotExists)
            {
                drawShadowPass.Clear();
                return;
            }

            renderer.EnqueuePass(drawShadowPass);
        }


        private void TrySetupLightCameraInfo()
        {
            if (settings.isUseLightTransform)
            {
                if (!lightObj && !string.IsNullOrEmpty(settings.lightTag))
                {
                    try
                    {
                        lightObj = GameObject.FindGameObjectWithTag(settings.lightTag);
                    }catch(Exception ex) { }
                }
            }
            else
            {
                lightObj = null;
            }

            if (!lightObj)
                return;
            settings.pos = lightObj.transform.position;
            settings.rot = lightObj.transform.eulerAngles;
            settings.up = lightObj.transform.up;

            // use lightHeight
            settings.pos.Set(settings.pos.x, settings.lightHeight, settings.pos.z);
        }
    }


}