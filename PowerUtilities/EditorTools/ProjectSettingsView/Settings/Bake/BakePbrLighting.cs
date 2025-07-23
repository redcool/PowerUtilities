namespace PowerUtilities
{
    using PowerUtilities.Coroutine;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;
    using WaitForEndOfFrame = Coroutine.WaitForEndOfFrame;

#if UNITY_EDITOR
    [CustomEditor(typeof(BakePbrLighting))]
    public class BakePbrLightingEditor : PowerEditor<BakePbrLighting>
    {
        public override bool NeedDrawDefaultUI() => true;
    }
#endif

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Bake/BakePbrLighting")]
    [SOAssetPath("Assets/PowerUtilities/BakePbrLighting.asset")]

    public class BakePbrLighting : ScriptableObject
    {

        public enum TextureBatchType
        {
            TexArr, TexAtlas
        }

        [HelpBox]
        public string helpBox = "Bake pbr lighting to texture";

        [Header("Baked Target")]
        [Tooltip("select target for bake")]
        [EditorObjectField]
        public GameObject target;

        [Tooltip("bakeCamera align to target pivot")]
        [EditorObjectField]
        public GameObject targetPosPivot;

        [EditorDisableGroup(targetPropName = nameof(isUseSceneCamPos), isRevertMode = true)]
        [Tooltip("bake camera's position offset alone camera's forward")]
        public float camOffsetDistance = 10;

        [Tooltip("target children renderer include  invisible ")]
        public bool isIncludeInvisible;

        [Tooltip("bake lighting use sceneView camera or main camera(Tag:MainCamera)")]
        public bool isUseSceneCamPos = false;
        //====================== output 
        [Header("Output")]
        [Tooltip("baked texture path for save")]
        public string outputPath = "Assets/PowerUtilities/BakeLighting";

        [Tooltip("bake texture's resolution")]
        public TextureResolution resolution = TextureResolution.x2048;

        [Tooltip("texture batch")]
        public TextureBatchType texBatchType;

        [Tooltip("combine children meshes ")]
        public bool isCombineMeshes;

        //====================== camera options
        [Header("Camera Options")]
        [Tooltip("bake camera clear color")]
        public Color backgroundColor = Color.clear;
        public CameraClearFlags cameraClearFlags = CameraClearFlags.SolidColor;

        [EditorButton(onClickCall = "BakeLighting")]
        public bool isBake;

        [EditorBox("Tools", "isShowTargetOnly,isShowScene", boxType = EditorBoxAttribute.BoxType.HBox)]
        [EditorButton(onClickCall = "ShowTargetOnly")]
        public bool isShowTargetOnly;

        [EditorButton(onClickCall = "ResumeShowScene")]
        [HideInInspector]
        public bool isShowScene;

        [EditorGroup("Debug", true)]
        public RenderTexture rt;
        [EditorGroup("Debug")]
        public Texture2D tex;

        Camera sceneCam;
        Camera bakeCam;
        Renderer[] lastShowRenderers;
        List<Vector4> lastRenderersUVList = new();

        public void BakeLighting()
        {
#if UNITY_EDITOR
            if (!target)
            {
                target = Selection.activeGameObject;
            }
#endif
            if (!target)
            {
                //EditorTools.DisplayDialog_Ok_Cancel("select a gameobject");
                Debug.LogError("select a gameobject");
                return;
            }

            if (isUseSceneCamPos && sceneCam)
                bakeCam = sceneCam;

            var renders = target.GetComponentsInChildren<Renderer>(isIncludeInvisible);
            if (renders.Length == 0)
                return;

            TryCreateRT(ref rt);

            var allObjs = Object.FindObjectsByType<Renderer>(sortMode: FindObjectsSortMode.None);
            //======================= begin render
            BeforeDraw(allObjs, out var lastTarget, out var lastPos, out var lastClearFlags);

            StartDraw(renders);

            //======================= after render
            AfterDraw(allObjs);
            bakeCam.targetTexture = lastTarget;
            bakeCam.transform.position = lastPos;
            bakeCam.clearFlags = lastClearFlags;


            RefreshAssetDatabase();
        }

        private void StartDraw(Renderer[] renders)
        {
            if (texBatchType == TextureBatchType.TexAtlas)
            {
                // bake objects 1 by 1
                lastRenderersUVList = RenderObjects_Atlas(renders);
                // readbake rt
                rt.ReadRenderTexture(ref tex, true);
                SaveTexAtlas();

                if (isCombineMeshes && renders.Length > 1)
                    CombineRenderers(renders, lastRenderersUVList);
            }
            else
            {
                RenderObjects_TexArray(renders);

                if (isCombineMeshes && renders.Length > 1)
                    CombineRenderers(renders, null);
            }

        }

        void RenderObjects(Renderer[] renders)
        {
            renders.ForEach(r => r.enabled = true);

            bakeCam.Render();
        }
        /// <summary>
        /// render objects 1by1
        /// output objects uv ranges
        /// </summary>
        /// <returns>xy : range start, zw : uv range size</returns>
        public List<Vector4> RenderObjects_Atlas(Renderer[] renders)
        {
            var tileUVOffsetTilingList = new List<Vector4>();

            int tileCount = GetTileCountARow(renders.Length);

            Vector2 tileSize = new Vector2(1f / tileCount, 1f / tileCount);

            for (int i = 0; i < renders.Length; i++)
            {
                var render = renders[i];

                var x = i % tileCount;
                var y = i / tileCount;

                var tileStart = new Vector2(x * tileSize.x, y * tileSize.y);
                // xy : range start, zw : uv range size
                tileUVOffsetTilingList.Add(new Vector4(tileStart.x, tileStart.y, tileSize.x, tileSize.y));

                //xy : range start, zw : range end
                var tileUV = new Vector4(tileStart.x, tileStart.y, tileSize.x + tileStart.x, tileSize.y + tileStart.y);
                Debug.Log($"id:{x},{y},count: {tileCount},suv :{tileUV}");
                Shader.SetGlobalVector("_FullScreenUVRange", tileUV);

                render.enabled = true;
                bakeCam.Render();

                bakeCam.clearFlags = CameraClearFlags.Nothing;
                render.enabled = false;
            }
            Shader.SetGlobalVector("_FullScreenUVRange", new Vector4(0, 0, 1, 1));

            return tileUVOffsetTilingList;
        }
        /// <summary>
        /// render objects into texture2d array
        /// </summary>
        void RenderObjects_TexArray(Renderer[] renders)
        {
            var texList = new List<Texture2D>();
            for (int i = 0; i < renders.Length; i++)
            {
                var render = renders[i];
                render.enabled = true;
                bakeCam.Render();
                
                Texture2D sliceTex = new Texture2D(rt.width,rt.height,TextureFormat.RGB24,true);
                rt.ReadRenderTexture(ref sliceTex,true);
#if UNITY_EDITOR
                EditorUtility.CompressTexture(sliceTex, TextureFormat.ASTC_6x6,TextureCompressionQuality.Normal);
#else
                sliceTex.Compress(false);
#endif
                texList.Add(sliceTex);

                render.enabled = false;
            }
#if UNITY_EDITOR
            var path = $"{outputPath}/{target}_texArr.asset";
            EditorTextureTools.Create2DArray(texList, path);
#endif
        }

        /// <summary>
        /// Get tiles count in a row
        /// </summary>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public static int GetTileCountARow(int totalCount)
        {
            int rowCount = Mathf.NextPowerOfTwo(totalCount);
            if (rowCount > 2)
                rowCount /= 2;
            return rowCount;
        }
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!sceneCam)
            {
                SceneView sceneView = null;
                TryGetFirstSceneView(ref sceneView);
                sceneCam = sceneView?.camera;
            }

            bakeCam = Camera.main;

            SceneView.duringSceneGui -= OnGizmos;
            SceneView.duringSceneGui += OnGizmos;
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnGizmos;
        }

        public static void TryGetFirstSceneView(ref SceneView sv)
        {
            if (sv || SceneView.sceneViews.Count == 0)
                return;

            sv = (SceneView)SceneView.sceneViews[0];
        }

        void OnGizmos(SceneView sceneView)
        {
            if (!target)
                return;

                sceneCam = sceneView.camera;

            CalcBakeCamPos(out var camPos, out var camForward);
            camPos = sceneCam.transform.position + sceneCam.transform.forward;

            Handles.color = Color.magenta;
            Handles.DrawLine(camPos, target.transform.position);
            Handles.color = Color.white;
        }
#endif

        void ShowTargetOnly()
        {
            lastShowRenderers = Object.FindObjectsByType<Renderer>(sortMode: FindObjectsSortMode.None);
            foreach (var obj in lastShowRenderers)
            {
                obj.enabled = false;
            }

            var renders = target.GetComponentsInChildren<Renderer>(isIncludeInvisible);
            foreach (var render in renders)
                render.enabled = true;
        }

        void ResumeShowScene()
        {
            if (lastShowRenderers == null)
                return;

            foreach (var obj in lastShowRenderers)
            {
                obj.enabled = true;
            }

            lastShowRenderers = null;
        }

        private void AfterDraw(Renderer[] allObjs)
        {
            //cam.clearFlags = CameraClearFlags.Skybox;
            Shader.SetGlobalFloat("_FullScreenOn", 0);

            foreach (var obj in allObjs)
                obj.enabled = true;
        }

        private void SaveTexAtlas()
        {
            PathTools.CreateAbsFolderPath(outputPath);
            File.WriteAllBytes($"{outputPath}/{target.name}.png", tex.EncodeToPNG());
        }
        void RefreshAssetDatabase()
        {
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(outputPath);
#endif
        }
        /// <summary>
        /// Combine renders
        /// </summary>
        /// <param name="renders"></param>
        /// <param name="tileUVList">offset uvs[0-8], null : dont offset uv</param>
        void CombineRenderers(Renderer[] renders,List<Vector4> tileUVList)
        {
            var mfList = renders.Select(r => r.GetComponent<MeshFilter>()).ToList();
            //tileUVList = new List<Vector4>() { 
            //new Vector4(0,0,.5f,.5f),
            //new Vector4(0.5f,0.5f,.5f,.5f),
            //};
            var mesh = MeshTools.CombineMesh(mfList, tileUVList);
#if UNITY_EDITOR
            var path = $"{outputPath}/{target.name}.asset";
            AssetDatabase.CreateAsset(mesh, path);
#endif
        }

        void CalcBakeCamPos(out Vector3 camPos, out Vector3 camForward)
        {
            var targetPos = targetPosPivot ? targetPosPivot.transform.position : target.transform.position;
            camPos = targetPos - bakeCam.transform.forward * camOffsetDistance;
            camForward = bakeCam.transform.forward;

            if (isUseSceneCamPos && sceneCam)
            {
                camPos = sceneCam.transform.position - sceneCam.transform.forward * 0.01f;
                camForward = sceneCam.transform.forward;
            }
        }

        private void BeforeDraw(Renderer[] allObjs, out RenderTexture lastTarget, out Vector3 lastPos, out CameraClearFlags lastClearFlags)
        {
            foreach (var obj in allObjs)
            {
                obj.enabled = false;
            }

            Shader.SetGlobalFloat("_FullScreenOn", 1);

            // keep
            lastTarget = bakeCam.targetTexture;
            lastPos = bakeCam.transform.position;
            lastClearFlags = bakeCam.clearFlags;

            bakeCam.targetTexture = rt;
            bakeCam.clearFlags = cameraClearFlags;
            bakeCam.backgroundColor = backgroundColor;

            CalcBakeCamPos(out var bakePos, out var bakeForward);
            bakeCam.transform.position = bakePos;
            bakeCam.transform.forward = bakeForward;
        }

        private void TryCreateRT(ref RenderTexture rt)
        {
            var w = (int)resolution;
            if (!rt)
            {
                rt = new RenderTexture(w, w, 32, GraphicsFormatTools.GetColorTextureFormat(true, false));
            }

            if (!tex)
            {
                tex = new Texture2D(w, w, TextureFormat.ARGB32, false, true);
            }
        }


    }
}