namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.SceneManagement;

#if UNITY_EDITOR
    [CustomEditor(typeof(DrawShadow))]
    public class DrawShadowEditor : SettingSOEditor
    {
        public override Type SettingSOType => typeof(DrawShadowSettingSO);
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //
            var inst = target as DrawShadow;

            //Debug
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lightObj"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bigShadowRenderCount"));

            // global settings

            EditorGUILayout.PropertyField(serializedObject.FindProperty("globalSettingSO"));
            if (!inst.settingSO)
                return;

            if (GUILayout.Button($"Set {inst.settingSO?.name} as globalSettingSO"))
            {
                inst.globalSettingSO = inst.settingSO;
            }

        }

    }
#endif

    public partial class DrawShadow : ScriptableRendererFeature
    {

        DrawShadowPass drawShadowPass;
        /// <summary>
        /// current used
        /// </summary>
        [Tooltip("Current use settings")]
        public DrawShadowSettingSO settingSO;

        [Header("Global Setting")]
        [Tooltip("1 when BigShadowLightControl disabled,settingSo will use,2 when Create will save settingSO if globalSettingSO is empty")]
        public DrawShadowSettingSO globalSettingSO;

        [Header("Debug")]
        [EditorDisableGroup]
        public GameObject lightObj;
        public BigShadowLightControl bigShadowLightControl;

        [EditorDisableGroup]
        public float currentDistance;

        /// <summary>
        /// step render mode's counter
        /// </summary>
        [Tooltip("StepMode'counter,set 0 will cause drawBigShadow once")]
        public int bigShadowRenderCount = 0;

        public static DrawShadow Instance { private set; get; }

        // for check so changed
        DrawShadowSettingSO lastSettingSO;
        public event Action OnSettingSOChanged;

        /// <inheritdoc/>
        public override void Create()
        {
            // save current setting first
            if (!globalSettingSO)
                globalSettingSO = settingSO;

            //keep a instance
            Instance = this;

            if(drawShadowPass == null)
                drawShadowPass = new DrawShadowPass();
            // Configures where the render pass should be injected.
            drawShadowPass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;

            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            OnSettingSOChanged -= DrawShadow_OnSettingSOChanged;
            OnSettingSOChanged += DrawShadow_OnSettingSOChanged;

            // editor dont need also,SceneManager_sceneLoaded will work,
            //CheckEditorSceneLoaded();
        }

        private void DrawShadow_OnSettingSOChanged()
        {
            StepDrawShadow();
        }

        partial void CheckEditorSceneLoaded();

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StepDrawShadow();
        }

        public void StepDrawShadow()
        {
            Clear();
            if (Instance && Instance.settingSO)
                Instance.settingSO.isStepRender = true;
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
        bool IsNeedDrawShadowOnce(CameraData cameraData)
        {
            var isMainCamera = cameraData.camera.IsMainCamera();
            var isSceneCamera = cameraData.camera.IsSceneViewCamera();
            var camera = cameraData.camera;

            if (!isMainCamera && !isSceneCamera)
                return false;

            // mainCamera follow it,sceneViewCamera dont do follow
            var isExceedMaxDistance = false;
            if(isMainCamera)
                isExceedMaxDistance = IsExceedMaxDistanceAndSaveFinalLightPos(camera, out currentDistance, ref settingSO.finalLightPos);

            var isStepRender = settingSO.isStepRender || isExceedMaxDistance || (bigShadowRenderCount ==0);
            if (isStepRender)
            {
                settingSO.isStepRender = false;
                bigShadowRenderCount++;
            }

            return settingSO.isAutoRendering || isStepRender;

            ///
            //============== inner methods
            ///

            Vector3 GetLightCameraBlendPos(Vector3 camPos)
            {
                var lightPos = (lightObj != null && settingSO.isUsePos) ? lightObj.transform.position : settingSO.pos;
                return Vector3.Lerp(camPos, lightPos, settingSO.lightCameraPosBlend);
            }
            
            bool IsExceedMaxDistanceAndSaveFinalLightPos(Camera camera, out float curDistance,ref Vector3 finalLightPos)
            {
                // dont follow camera
                if (settingSO.maxDistance <= 0)
                {
                    finalLightPos = settingSO.pos;
                    curDistance = -1;
                    return false;
                }

                // follow camera append lightPosOffset
                var lightCamPos = GetLightCameraBlendPos(camera.transform.position);
                lightCamPos += settingSO.lightPosOffset; 

                var dir = lightCamPos - finalLightPos; // include lightPosOffset
                curDistance = dir.magnitude;

                var isExceedMaxDistance = curDistance > settingSO.maxDistance;
                if (isExceedMaxDistance)
                {
                    finalLightPos = lightCamPos;
                }

                return isExceedMaxDistance;
            }
        }
        /// <summary>
        /// Clear current big shadowMap and stop
        /// </summary>
        /// <returns></returns>
        private bool IsNeedStopShadow()
        {
            var isUseLightObjButNotExists = settingSO.isUseLightTransform && !string.IsNullOrEmpty(settingSO.lightTag) & !lightObj;
            var isDontNeedDrawShadow = settingSO.layers == 0;
            return isUseLightObjButNotExists || isDontNeedDrawShadow;
        }

        void DrawLightGizmos(ref CameraData cameraData)
        {
#if UNITY_EDITOR
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
            DebugTools.DrawLineCube(vertices,settingSO.boundsLineColor);
#endif
        }

        /// <summary>
        /// check settingSO is changed
        /// </summary>
        private void CheckSettingSOChange()
        {
            if (CompareTools.CompareAndSet(ref lastSettingSO, ref settingSO))
            {
                OnSettingSOChanged?.Invoke();
            }
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settingSO)
                return;

            CheckSettingSOChange();

            drawShadowPass.settingSO = settingSO;

            drawShadowPass.UpdateShaderVariables();

            // clear shadowMap only
            if (settingSO.isClearShadowMap)
            {
                settingSO.isClearShadowMap = false;
                drawShadowPass.Clear();
            }

            ref var cameraData = ref renderingData.cameraData;
            if(cameraData.isSceneViewCamera)
                DrawLightGizmos(ref cameraData);

            // find Light and setup
            var isNewLight = TrySetupLightCameraInfo();
            if (isNewLight)
                settingSO.isStepRender = true;

            // clear shadowmap
            if (IsNeedStopShadow())
            {
                Clear();
                return;
            }

            // dont need draw shadow again
            if (!IsNeedDrawShadowOnce(cameraData) && drawShadowPass.IsBigShadowMapValid())
            {
                return;
            }

            renderer.EnqueuePass(drawShadowPass);
        }

        public void Clear()
        {
            drawShadowPass?.Clear();
            bigShadowRenderCount = 1;
            lightObj = null;
        }

        private bool TrySetupLightCameraInfo()
        {
            var isNewLight = false;
            FindBigShadowLight(ref isNewLight);

            if (!lightObj)
                return isNewLight;


            SetupLightCamFromBigShadowLightTransform();
            if (settingSO.isUseBigShadowLightBoxCollider)
                SetupLightCamFromBigShadowLightBounds();

            SetupBigShadowLightControl(lightObj);
            return isNewLight;
        }
        /// <summary>
        /// Get BigShadowLightTransform collider, orthoSize,near,far
        /// </summary>
        void SetupLightCamFromBigShadowLightBounds()
        {
            var c = lightObj.GetComponent<BoxCollider>();
            if (!c)
                return;
            var halfSize = c.size * 0.5f;
            settingSO.near = -halfSize.z;
            settingSO.far = halfSize.z;
            settingSO.orthoSize = halfSize.y;
        }

        /// <summary>
        /// Get BigShadowLightTransform's pos,rot,up
        /// </summary>
        void SetupLightCamFromBigShadowLightTransform()
        {
            if (settingSO.isUsePos)
                settingSO.pos = lightObj.transform.position + settingSO.lightPosOffset;
            if (settingSO.isUseRot)
                settingSO.rot = lightObj.transform.eulerAngles;
            if (settingSO.isUseUp)
                settingSO.up = lightObj.transform.up;
        }

        /// <summary>
        /// Find BigShadowLight 
        /// </summary>
        /// <param name="isNewLight">when find it first time</param>
        void FindBigShadowLight(ref bool isNewLight)
        {
            if (settingSO.isUseLightTransform)
            {
                if (!lightObj && !string.IsNullOrEmpty(settingSO.lightTag))
                {
                    try
                    {
                        // find first actived Light
                        lightObj = GameObject.FindGameObjectsWithTag(settingSO.lightTag)
                            .Where(go => go.activeInHierarchy)
                            .FirstOrDefault();
                        //lightObj = GameObject.FindGameObjectWithTag(settingSO.lightTag);

                        isNewLight = lightObj;
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                lightObj = null;
            }
        }

        private void SetupBigShadowLightControl(GameObject lightObj)
        {
            if (!lightObj)
                return;

            bigShadowLightControl = lightObj.GetComponent<BigShadowLightControl>();
            if (bigShadowLightControl)
            {
                bigShadowLightControl.OnShadowEnableChanged -= BigShadowLightControl_OnShadowEnableChanged;
                bigShadowLightControl.OnShadowEnableChanged += BigShadowLightControl_OnShadowEnableChanged;

                bigShadowLightControl.OnOverrideSettingSO -= BigShadowLightControl_OnOverrideSettingSO;
                bigShadowLightControl.OnOverrideSettingSO += BigShadowLightControl_OnOverrideSettingSO;
            }
        }

        private void BigShadowLightControl_OnOverrideSettingSO(bool isOn, DrawShadowSettingSO overrideSettingSO)
        {
            if (!overrideSettingSO)
                overrideSettingSO = globalSettingSO;

            settingSO.isStepRender = isOn;
            settingSO = isOn ? overrideSettingSO : globalSettingSO;
        }

        private void BigShadowLightControl_OnShadowEnableChanged(bool isOn)
        {
            if (isOn)
                StepDrawShadow();
            else
                Clear();
        }
    }


}