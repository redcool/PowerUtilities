namespace PowerUtilities
{
    using System;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
    [CustomEditor(typeof(DrawShadow))]
    public class DrawShadowEditor : SettingSOEditor
    {
        public override Type SettingSOType => typeof(DrawShadowSettingSO);
    }
#endif


    public class DrawShadow : ScriptableRendererFeature
    {
        static RenderTexture emptyShadowMap;
        /// <summary>
        /// Defulat empty shadowmap,
        /// Texture2D.whiteTexture,some device will crash.
        /// 
        /// not clear, _BigShadowParams.x is shadowIntensity,
        /// first time need render bigShadow once, otherwist _BigShadowMap is black
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

        DrawShadowPass drawShadowPass;
        public DrawShadowSettingSO settingSO;

        [Header("Debug")]
        [EditorReadonly]
        public GameObject lightObj;
        [EditorReadonly]
        public float currentDistance;

        /// <summary>
        /// step render mode's counter
        /// </summary>
        [Tooltip("StepMode'counter,set 0 will cause drawBigShadow once")]
        public int bigShadowRenderCount = 0;

        /// <inheritdoc/>
        public override void Create()
        {
            drawShadowPass = new DrawShadowPass();

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
        bool IsNeedDrawShadow(CameraData cameraData)
        {
            var isMainCamera = cameraData.camera.IsMainCamera();
            var isSceneCamera = cameraData.camera.IsSceneViewCamera();

            if (!isMainCamera && !isSceneCamera)
                return false;

            // mainCamera follow it,sceneViewCamera dont do follow
            var isExceedMaxDistance = false;
            if (isMainCamera)
                isExceedMaxDistance = IsExceedMaxDistanceAndSaveLightPos(cameraData, out currentDistance, ref settingSO.finalLightPos);

            var isStepRender = settingSO.isStepRender || isExceedMaxDistance || (bigShadowRenderCount ==0);
            if (isStepRender)
            {
                settingSO.isStepRender = false;
                bigShadowRenderCount++;
            }

            return settingSO.isAutoRendering || isStepRender;

            //============== methods
            bool IsExceedMaxDistanceAndSaveLightPos(CameraData cameraData,out float curDistance,ref Vector3 finalLightPos)
            {
                // dont follow camera
                if (settingSO.maxDistance <= 0)
                {
                    finalLightPos = settingSO.pos;
                    curDistance = -1;
                    return false;
                }

                // follow camera

                var cameraPos = cameraData.camera.transform.position;
                cameraPos.y = finalLightPos.y = settingSO.lightHeight;

                var dir = cameraPos - finalLightPos;
                curDistance = dir.magnitude;

                var isExceedMaxDistance = curDistance > settingSO.maxDistance;
                if (isExceedMaxDistance)
                {
                    finalLightPos = cameraPos;
                }

                return isExceedMaxDistance;
            }
        }

        void DrawLightGizmos(ref CameraData cameraData)
        {
            var h = settingSO.orthoSize;
            var w = h;
            var n = settingSO.near;
            var f = settingSO.far;

            var p0 = new Vector3(-w, -h, n);
            var p1 = new Vector3(-w, h, n);
            var p2 = new Vector3(w, h, n);
            var p3 = new Vector3(w, -h, n);

            var p4 = new Vector3(-w, -h, f);
            var p5 = new Vector3(-w, h, f);
            var p6 = new Vector3(w, h, f);
            var p7 = new Vector3(w, -h, f);

            var rot = Quaternion.Euler(settingSO.rot);

            var vertices = new[] { p0, p1, p2, p3, p4, p5, p6, p7 };
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = (settingSO.finalLightPos + rot * vertices[i]);
            }
            // view frustum
            DebugTools.DrawLineCube(vertices);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settingSO)
                return;

            drawShadowPass.settingSO = settingSO;

            drawShadowPass.UpdateShaderVariables();

            if(settingSO.isClearShadowMap)
            {
                settingSO.isClearShadowMap = false;
                drawShadowPass.Clear();
            }

            ref var cameraData = ref renderingData.cameraData;
            TrySetupLightCameraInfo();

            DrawLightGizmos(ref cameraData);

            // dont run draw shadow
            var isUseLightObjButNotExists = settingSO.isUseLightTransform && !string.IsNullOrEmpty(settingSO.lightTag) & !lightObj;
            if (isUseLightObjButNotExists)
            {
                drawShadowPass.Clear();
                return;
            }

            // dont need draw shadow again
            if (!IsNeedDrawShadow(cameraData))
            {
                return;
            }

            renderer.EnqueuePass(drawShadowPass);
        }


        private void TrySetupLightCameraInfo()
        {
            if (settingSO.isUseLightTransform)
            {
                if (!lightObj && !string.IsNullOrEmpty(settingSO.lightTag))
                {
                    try
                    {
                        lightObj = GameObject.FindGameObjectWithTag(settingSO.lightTag);
                    }catch(Exception ex) { }
                }
            }
            else
            {
                lightObj = null;
            }

            if (!lightObj)
                return;
            settingSO.pos = lightObj.transform.position;
            settingSO.rot = lightObj.transform.eulerAngles;
            settingSO.up = lightObj.transform.up;

            // use lightHeight
            settingSO.pos.Set(settingSO.pos.x, settingSO.lightHeight, settingSO.pos.z);
        }
    }


}