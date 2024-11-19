namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Linq;
#if UNITY_EDITOR
    //using Unity.EditorCoroutines.Editor;
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

            // swap script
            var isOverrideSO = inst.settingSO.GetType() == typeof(DrawShadowOverrideSettingSO);
            var newScriptName = !isOverrideSO ? "DrawShadowOverrideSettingSO" : "DrawShadowSettingSO";
            if (GUILayout.Button($"Update script to {newScriptName}"))
            {
                //EditorCoroutineUtility.StartCoroutine(WaitForChangeSettingSO(inst,newScriptName),this);
                EditorApplicationTools.coroutineList.Add(WaitForChangeSettingSO(inst, newScriptName));
            }
        }

        IEnumerator WaitForChangeSettingSO(DrawShadow inst,string newScriptName)
        {
            var path = AssetDatabase.GetAssetPath(inst.settingSO);
            inst.settingSO.ChangeRefScript(newScriptName);
            inst.settingSO = ScriptableObject.CreateInstance< DrawShadowOverrideSettingSO>();
            yield return 0;
            inst.settingSO = AssetDatabase.LoadAssetAtPath<DrawShadowSettingSO>(path);
        }
    }
#endif


    public class DrawShadow : ScriptableRendererFeature
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
        
        /// <inheritdoc/>
        public override void Create()
        {
            // save current setting first
            if(!globalSettingSO)
                globalSettingSO = settingSO;

            //keep a instance
            Instance = this;

            drawShadowPass = new DrawShadowPass();
            // Configures where the render pass should be injected.
            drawShadowPass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;

            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

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

            if (!isMainCamera && !isSceneCamera)
                return false;

            // mainCamera follow it,sceneViewCamera dont do follow
            var isExceedMaxDistance = false;
            if (isMainCamera)
                isExceedMaxDistance = IsExceedMaxDistanceAndSaveFinalLightPos(cameraData, out currentDistance, ref settingSO.finalLightPos);

            var isStepRender = settingSO.isStepRender || isExceedMaxDistance || (bigShadowRenderCount ==0);
            if (isStepRender)
            {
                settingSO.isStepRender = false;
                bigShadowRenderCount++;
            }

            return settingSO.isAutoRendering || isStepRender;

            //============== methods
            bool IsExceedMaxDistanceAndSaveFinalLightPos(CameraData cameraData,out float curDistance,ref Vector3 finalLightPos)
            {
                // dont follow camera
                if (settingSO.maxDistance <= 0)
                {
                    finalLightPos = settingSO.pos;
                    curDistance = -1;
                    return false;
                }

                // follow camera append lightPosOffset
                var finalCameraPos = cameraData.camera.transform.position + settingSO.lightPosOffset; 

                var dir = finalCameraPos - finalLightPos; // include lightPosOffset
                curDistance = dir.magnitude;

                var isExceedMaxDistance = curDistance > settingSO.maxDistance;
                if (isExceedMaxDistance)
                {
                    finalLightPos = finalCameraPos;
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
            DebugTools.DrawLineCube(vertices);
#endif
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settingSO)
                return;

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
            drawShadowPass.Clear();
            lightObj = null;
            bigShadowRenderCount = 1;
        }

        private bool TrySetupLightCameraInfo()
        {
            var isNewLight = false;
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

            if (!lightObj)
                return isNewLight;

            settingSO.pos = lightObj.transform.position + settingSO.lightPosOffset;
            settingSO.rot = lightObj.transform.eulerAngles;
            settingSO.up = lightObj.transform.up;

            SetupBigShadowLightControl(lightObj);

            return isNewLight;
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