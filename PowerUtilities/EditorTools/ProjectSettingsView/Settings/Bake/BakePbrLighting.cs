namespace PowerUtilities
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

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
            TexArr, TexAtlas,PerObject
        }

        public const string
            _FullScreenOn = "_FullScreenOn",
            _FullScreenUVRange = "_FullScreenUVRange",
            _CullMode = "_CullMode"
            ;

        [HelpBox]
        public string helpBox = "Bake pbr lighting to texture";

        //====================== baked target
        [Header("Baked Target")]
        [Tooltip("select target for bake")]
        [EditorObjectField]
        public GameObject target;

        [Tooltip("Get selected target when bake start")]
        public bool isUseSelectedAlways;

        [Tooltip("bakeCamera align to target pivot")]
        [EditorObjectField]
        public GameObject targetPosPivot;

        //====================== object settings
        [Tooltip("target children renderer include  invisible ")]
        public bool isIncludeInvisible;

        [Tooltip("set object material CullOff")]
        public bool isSetCullOff;

        //====================== baked camera
        [Header("BakeCamera")]
        [Tooltip("bake lighting use sceneView camera or main camera(Tag:MainCamera)")]
        public bool isUseSceneCamPos = false;

        [EditorDisableGroup(targetPropName = nameof(isUseSceneCamPos), isRevertMode = true)]
        [Tooltip("bake camera's position offset alone camera's forward")]
        public float camOffsetDistance = 10;

        //====================== output 
        [Header("Output")]
        [Tooltip("baked texture path for save")]
        public string outputPath = "Assets/PowerUtilities/BakeLighting";

        [Tooltip("bake texture's resolution")]
        public TextureResolution resolution = TextureResolution.x2048;

        [Tooltip("texture batch")]
        public TextureBatchType texBatchType;

        [Tooltip("output texture type (texAtlas)")]
        public TextureEncodeType texEncodeType;
        [Tooltip("combine children meshes and create prefab")]
        public bool isCombineMeshes;

        [Tooltip("create prefab mat use this shader")]
        [LoadAsset("Unlit_ColorMRT.shader")]
        public Shader bakedPbrLgihtShader;

        //====================== camera options
        [Header("Camera Options")]
        [Tooltip("bake camera clear color")]
        public Color backgroundColor = Color.clear;
        public CameraClearFlags cameraClearFlags = CameraClearFlags.SolidColor;

        //======================  bake
        [EditorButton(onClickCall = nameof(BakeLighting))]
        public bool isBake;

        //====================== tools
        [Header("Tools")]
        [Tooltip("_FullScreenOn set on")]
        public bool isShowFullscreen;

        [EditorBox("", "isShowTargetOnly,isShowScene,isSelectOutputFolder", boxType = EditorBoxAttribute.BoxType.HBox)]
        [EditorButton(onClickCall = nameof(ShowTargetOnly))]
        public bool isShowTargetOnly;

        [EditorButton(onClickCall = nameof(ResumeShowScene))]
        [HideInInspector]
        public bool isShowScene;

        [EditorButton(onClickCall = nameof(RefreshAssetDatabase),text ="SelectOutputFolder",nameType = EditorButtonAttribute.NameType.AttributeText)]
        [HideInInspector]
        public bool isSelectOutputFolder;


        //====================== debug
        [EditorGroup("Debug", true)]
        public RenderTexture targetRT;
        [EditorGroup("Debug")]
        public Texture2D outputTex;

        Camera sceneCam;
        Camera bakeCam;
        Renderer[] lastShowRenderers;
        List<Vector4> lastRenderersUVList = new();

        Renderer[] targetRenderers;
        Renderer[] allRenderers;

        public void BakeLighting()
        {
#if UNITY_EDITOR
            if (!target || isUseSelectedAlways)
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

            targetRenderers = target.GetComponentsInChildren<Renderer>(isIncludeInvisible);
            if (targetRenderers.Length == 0)
                return;

            bakeCam = Camera.main;
            if (isUseSceneCamPos && sceneCam)
                bakeCam = sceneCam;

            TryCreateRT(ref targetRT);

            allRenderers = Object.FindObjectsByType<Renderer>(sortMode: FindObjectsSortMode.None);
            //======================= begin render
            BeforeDraw(allRenderers, out var lastTarget, out var lastPos, out var lastClearFlags);

            StartDraw(targetRenderers);

            //======================= after render
            AfterDraw(allRenderers);
            bakeCam.targetTexture = lastTarget;
            bakeCam.transform.position = lastPos;
            bakeCam.clearFlags = lastClearFlags;


            RefreshAssetDatabase();
        }

        private void StartDraw(Renderer[] renders)
        {
            switch (texBatchType)
            {
                case TextureBatchType.TexAtlas:
                    StartTexAtlasFlow(renders);
                    break;
                case TextureBatchType.TexArr:
                    StartTexArrayFlow(renders);
                    break;
                case TextureBatchType.PerObject:
                    StartPerObjectFlow(renders);
                    break;
            }
        }

        private void AfterDraw(Renderer[] allRenderers)
        {
            Shader.SetGlobalFloat(_FullScreenOn, 0);

            foreach (Renderer r in allRenderers)
            {
                if (r)
                    r.enabled = true;
            }
        }

        private void StartPerObjectFlow(Renderer[] renders)
        {
            for (int i = 0; i < renders.Length; i++)
            {
                ShowProgressBar(i, renders.Length);

                var render = renders[i];

                StartRenderObject(ref render, ref outputTex);

                SaveOutputTex(render.name);
            }

            ClearProgressBar();
        }

        private void StartTexArrayFlow(Renderer[] renders)
        {
            var texList = RenderObjects_TexArray(renders);
            SaveTexArray(texList);

            CombineRenderersAndSavePrefab(renders, null);
        }

        private void StartTexAtlasFlow(Renderer[] renders)
        {
            // bake objects 1 by 1
            lastRenderersUVList = RenderObjects_Atlas(renders);
            // readbake rt
            targetRT.ReadRenderTexture(ref outputTex, true);
            SaveOutputTex(target.name);

            CombineRenderersAndSavePrefab(renders, lastRenderersUVList);
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
                ShowProgressBar(i,renders.Length);

                var render = renders[i];

                var x = i % tileCount;
                var y = i / tileCount;

                var tileStart = new Vector2(x * tileSize.x, y * tileSize.y);
                // xy : range start, zw : uv range size
                tileUVOffsetTilingList.Add(new Vector4(tileStart.x, tileStart.y, tileSize.x, tileSize.y));

                //xy : range start, zw : range end
                var tileUV = new Vector4(tileStart.x, tileStart.y, tileSize.x + tileStart.x, tileSize.y + tileStart.y);
                Debug.Log($"id:{x},{y},count: {tileCount},suv :{tileUV}");
                Shader.SetGlobalVector(_FullScreenUVRange, tileUV);

                StartRenderObject(ref render,ref outputTex);
                
                //no need clear, is ok, 
                bakeCam.clearFlags = CameraClearFlags.Nothing;
            }
            Shader.SetGlobalVector(_FullScreenUVRange, new Vector4(0, 0, 1, 1));

            ClearProgressBar();

            return tileUVOffsetTilingList;
        }


        /// <summary>
        /// render 1 object ,save to tex
        /// </summary>
        /// <param name="render"></param>
        /// <param name="tex"></param>
        void StartRenderObject(ref Renderer render,ref Texture2D tex)
        {
            render.enabled = true;
            var lastCullMode = render.sharedMaterial.GetFloat(_CullMode);
            if (isSetCullOff)
                render.sharedMaterial.SetFloat(_CullMode, 0);

            bakeCam.Render();

            targetRT.ReadRenderTexture(ref tex, true);

            render.enabled = false;
            render.sharedMaterial.SetFloat(_CullMode, lastCullMode);
            
        }
        /// <summary>
        /// render objects into texture2d array
        /// </summary>
        List<Texture2D> RenderObjects_TexArray(Renderer[] renders)
        {
            var texList = new List<Texture2D>();
            for (int i = 0; i < renders.Length; i++)
            {
                ShowProgressBar(i, renders.Length);

                Texture2D sliceTex = new Texture2D(targetRT.width,targetRT.height,TextureFormat.RGB24,true,true);
                var render = renders[i];
                StartRenderObject(ref render,ref sliceTex);
#if UNITY_EDITOR
                EditorUtility.CompressTexture(sliceTex, TextureFormat.ASTC_6x6,TextureCompressionQuality.Normal);
#else
                sliceTex.Compress(false);
#endif
                texList.Add(sliceTex);
            }

            ClearProgressBar();

            return texList;
        }

        private void SaveTexArray(List<Texture2D> texList)
        {
            PathTools.CreateAbsFolderPath(outputPath);
#if UNITY_EDITOR
            var path = $"{outputPath}/{target.name}_texArr.asset";
            EditorTextureTools.Create2DArray(texList, path);
#endif
        }

        private void SaveOutputTex(string targetName)
        {
            PathTools.CreateAbsFolderPath(outputPath);

            var extName = texEncodeType.ToString();
            var filePath = $"{outputPath}/{targetName}.{extName}";
            File.WriteAllBytes(filePath, outputTex.GetEncodeBytes(texEncodeType));

#if UNITY_EDITOR
            AssetDatabase.Refresh();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            tex?.Setting(texImp =>
            {
                texImp.sRGBTexture = false;
                texImp.SaveAndReimport();
            });
#endif
        }

        private void ShowProgressBar(int i, int length)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Warning", "Bake PbrLighting", i / (float)length);
#endif
        }
        void ClearProgressBar()
        {
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
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
            {
                render.enabled = true;
            }

            Shader.SetGlobalFloat(_FullScreenOn, isShowFullscreen?1:0);
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
            Shader.SetGlobalFloat(_FullScreenOn, 0);
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
        void CombineRenderersAndSavePrefab(Renderer[] renders, List<Vector4> tileUVList)
        {
            if (!isCombineMeshes || renders.Length <= 1)
            {
                //Debug.Log("dont need combine meshes, renders.Length == 0");
                return;
            }
            var mfList = renders.Select(r => r.GetComponent<MeshFilter>()).ToList();
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            var tex = AssetDatabaseTools.FindAssetPathAndLoad<Texture2D>(out _, target.name, searchInFolders: outputPath);

            var texArr = AssetDatabaseTools.FindAssetPathAndLoad<Texture2DArray>(out _, $"{target.name}_texArr", searchInFolders: outputPath);
            // create combine mesh
            Mesh meshAsset = SaveMesh(tileUVList, mfList);
            // create mat
            Material matPrefab = SaveMat(tex, texArr);
            // create prefab
            SavePrefab(meshAsset, matPrefab);

            //------------ inner methods
            Mesh SaveMesh(List<Vector4> tileUVList, List<MeshFilter> mfList)
            {
                var mesh = MeshTools.CombineMesh(mfList, tileUVList);
                var meshPath = $"{outputPath}/{target.name}.mesh";
                var meshAsset = AssetDatabaseTools.CreateAssetThenLoad<Mesh>(mesh, meshPath);
                return meshAsset;
            }
            Material SaveMat(Texture2D tex, Texture2DArray texArr)
            {
                var matPath = $"{outputPath}/{target.name}.mat";
                var mat = new Material(bakedPbrLgihtShader);
                mat.SetFloat("_UseUV1", 1);
                mat.SetFloat("_UV1ReverseY", 1);

                mat.SetTexture("_MainTex", tex);
                mat.SetTexture("_MainTexArray", texArr);
                var matPrefab = AssetDatabaseTools.CreateAssetThenLoad<Material>(mat, matPath);
                return matPrefab;
            }

            void SavePrefab(Mesh meshAsset, Material matPrefab)
            {
                var go = new GameObject($"{target.name}");
                go.AddComponent<MeshFilter>().sharedMesh = meshAsset;
                go.AddComponent<MeshRenderer>().sharedMaterial = matPrefab;
                var prefab = PrefabTools.CreatePrefab(go, $"{outputPath}/{target.name}.prefab");
                Selection.activeObject = prefab;
            }
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

        private void BeforeDraw(Renderer[] allRenderers, out RenderTexture lastTarget, out Vector3 lastPos, out CameraClearFlags lastClearFlags)
        {
            foreach (var r in allRenderers)
            {
                if (r)
                    r.enabled = false;
            }

            Shader.SetGlobalFloat(_FullScreenOn, 1);

            // keep
            lastTarget = bakeCam.targetTexture;
            lastPos = bakeCam.transform.position;
            lastClearFlags = bakeCam.clearFlags;

            bakeCam.targetTexture = targetRT;
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

            if (!outputTex)
            {
                outputTex = new Texture2D(w, w, TextureFormat.ARGB32, false, true);
            }
        }


    }
}