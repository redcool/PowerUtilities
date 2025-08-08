namespace PowerUtilities
{
    using PowerUtilities.Coroutine;
    using PowerUtilities.RenderFeatures;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Unity.Mathematics;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;
    using WaitForSeconds = Coroutine.WaitForSeconds;

#if UNITY_EDITOR
    [CustomEditor(typeof(BakePbrLighting))]
    public class BakePbrLightingEditor : PowerEditor<BakePbrLighting>
    {
        public override bool NeedDrawDefaultUI() => true;
    }
#endif

    public enum TextureBatchType
    {
        /**objects to texture array*/
        TexArr,
        /** objects to texture atlas*/
        TexAtlas,
        /** objects to texture atlas( uv pre combined)*/
        AllInOne,
        /** objects to textures (uv1 to lightmap uv)*/
        AllInOneLightmapUV,
        /** objects texture one by one*/
        PerObject,

    }

    public enum UVMode
    {
        UV, UV1, UV2, UV3,
    }

    public enum TextureSuffix
    {
        C, N,PM,/*pbrMask*/
        E
    }


    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Bake/BakePbrLighting")]
    [SOAssetPath("Assets/PowerUtilities/BakePbrLighting.asset")]

    public class BakePbrLighting : ScriptableObject
    {


        public const string
            _FullScreenOn = "_FullScreenOn",
            _FullScreenUVRange = "_FullScreenUVRange",
            _CullMode = "_CullMode",
            _UV1TransformToLightmapUV = "_UV1TransformToLightmapUV",
            _FullScreenUVId = "_FullScreenUVId",
            _OutputNormal01 = "_OutputNormal01",
            _OutputPbrMask = "_OutputPbrMask",
            _OutputEmission = "_OutputEmission"
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

        [Tooltip("cameraTarget & outputTex is hdr")]
        public bool isHDR;

        [Header("UV Space")]
        [Tooltip("set object material CullOff,[0-3]")]
        public UVMode uvMode = UVMode.UV1;


        //====================== baked camera
        [Header("BakeCamera")]

        //[Tooltip("use target camera or MainCamera")]
        //[EditorObjectField(objectType = typeof(Camera))]
        //public Camera targetCamera;

        [Tooltip("bake lighting use sceneView camera or main camera(Tag:MainCamera)")]
        public bool isUseSceneCamPos = false;

        [EditorDisableGroup(targetPropName = nameof(isUseSceneCamPos), isRevertMode = true)]
        [Tooltip("bake camera's position offset alone camera's forward")]
        public float camOffsetDistance = 10;

        [Tooltip("wait n frame ")]
        public int waitFramesForSetTargets = 10;
        public int waitFramesForClearTargets = 10;

        //====================== output 
        [Header("Output")]
        [Tooltip("baked texture path for save")]
        public string outputPath = "Assets/PowerUtilities/BakeLighting";

        [Tooltip("bake texture's resolution")]
        public TextureResolution resolution = TextureResolution.x2048;

        [Tooltip("texture batch")]
        public TextureBatchType texBatchType;

        [Tooltip("output texture asset also")]
        public bool isOutputTexAsset;

        [Tooltip("output texture asset format")]
        [EnumSearchable(enumType = typeof(TextureFormat))]
        public TextureFormat outputTexAssetFormat = TextureFormat.ASTC_HDR_6x6;

        [Tooltip("output texture type (texAtlas)")]
        public TextureEncodeType texEncodeType;
        [Tooltip("combine children meshes and create prefab")]
        public bool isCombineMeshes;

        [Tooltip("create prefab mat use this shader")]
        [LoadAsset("BakedPbrLit.shader")]
        public Shader bakedPbrLgihtShader;

        [LoadAsset("ColorConvert.compute")]
        public ComputeShader colorConvertCS;
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

        /**object uv1 scale offset to scene lightmap*/
        [Tooltip("bake object to sceneLightmapUV,for uvMode.uv1,TexBatchType.AllInOneLightmapUV")]
        public bool isUV1TransformToLightmapUV;

        [EditorBox("", "isShowTargetOnly,isShowScene,isSelectOutputFolder", boxType = EditorBoxAttribute.BoxType.HBox)]
        [EditorButton(onClickCall = nameof(ShowTargetOnly))]
        public bool isShowTargetOnly;

        [EditorButton(onClickCall = nameof(ResumeShowScene))]
        [HideInInspector]
        public bool isShowScene;

        [EditorButton(onClickCall = nameof(RefreshAssetDatabase),text ="SelectOutputFolder",nameType = EditorButtonAttribute.NameType.AttributeText)]
        [HideInInspector]
        public bool isSelectOutputFolder;

        [EditorButton(onClickCall = nameof(ClearRTs), text = "SelectOutputFolder", nameType = EditorButtonAttribute.NameType.AttributeText)]
        //[HideInInspector]
        public bool isClearRTs;

        //====================== debug
        [EditorGroup("Debug", true)]
        [Tooltip("intermediate target,double click you change rt params")]
        public RenderTexture targetRT;
        [EditorGroup("Debug")]
        public RenderTexture 
            // normal,pbrMask,emission
            targetRT1 , targetRT2, targetRT3 ;

        [EditorGroup("Debug")]
        public RenderTexture depthRT;

        string[] colorTargetNames;
        string depthTargetName = nameof(depthRT);

        Camera sceneCam;
        Camera bakeCam;
        Renderer[] lastShowRenderers;
        List<(Renderer,float)> lastRendererCullModeList = new();
        List<Vector4> lastRenderersUVList = new();

        Renderer[] targetRenderers;
        Renderer[] allRenderers;

        // sfc pass(urp
        SRPRenderFeatureControl srpControl = null;
        (SRPFeature,bool)[] setTargetFeatureInfos;
        SetRenderTarget setRenderTargetFeature;
        string setRenderTargetFeatureName = "bake pbr lighting(SetTargets)";

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

            //bakeCam = targetCamera ?? Camera.main;
            bakeCam = Camera.main;

            if (!bakeCam)
            {
                Debug.LogError("need target camera or set MainCamera");
                return;
            }

            targetRenderers = target.GetComponentsInChildren<Renderer>(isIncludeInvisible);
            if (targetRenderers.Length == 0)
                return;

            SetupRTs();

            allRenderers = Object.FindObjectsByType<Renderer>(sortMode: FindObjectsSortMode.None);


            //======================= begin render
            CoroutineTool.StartCoroutine(WaitForBaking());
            //StartBaking();
        }
        IEnumerator WaitForBaking()
        {
            SetCameraTargets();

            yield return new Coroutine.WaitForEndOfFrame(waitFramesForSetTargets);
            StartBaking();

            yield return new Coroutine.WaitForEndOfFrame(waitFramesForClearTargets);

            if (setRenderTargetFeature)
            {
                srpControl?.featureListSO?.featureList.Remove(setRenderTargetFeature);
            }
        }
        private void StartBaking()
        {
            Debug.Log("StartBaking");
            var lastForward = bakeCam.transform.forward;
            BeforeDraw(allRenderers, out var lastTarget, out var lastPos, out var lastClearFlags);

            StartDraw(targetRenderers);

            //======================= after render
            AfterDraw(allRenderers);

            //bakeCam.targetTexture = lastTarget;
            bakeCam.targetTexture = null;
            bakeCam.clearFlags = lastClearFlags;
            bakeCam.transform.position = lastPos;
            bakeCam.transform.forward = lastForward;

            RefreshAssetDatabase();
        }

        private void SetupRTs()
        {
            var w = (int)resolution;
            if (!targetRT)
            {
                targetRT = new RenderTexture(w, w, 0, GraphicsFormatTools.GetColorTextureFormat(isHDR, true)) { name = nameof(targetRT) };
            }
            if (!targetRT1)
            {
                targetRT1 = new RenderTexture(w, w, 0, GraphicsFormatTools.GetColorTextureFormat(false, false)) { name = nameof(targetRT1) };
            }
            if (!targetRT2)
            {
                targetRT2 = new RenderTexture(w, w, 0, GraphicsFormatTools.GetColorTextureFormat(false, false)) { name = nameof(targetRT2) };
            }
            if (!targetRT3)
            {
                targetRT3 = new RenderTexture(w, w, 0, GraphicsFormatTools.GetColorTextureFormat(isHDR, true)) { name = nameof(targetRT3) };
            }

            if (!depthRT)
            {
                depthRT = new RenderTexture(w, w, 0, RenderTextureFormat.Depth) { name = nameof(depthRT) };
            }
            RenderTextureTools.AddRT(targetRT, targetRT.name);
            RenderTextureTools.AddRT(targetRT1, targetRT1.name);
            RenderTextureTools.AddRT(targetRT2, targetRT2.name);
            RenderTextureTools.AddRT(targetRT3, targetRT3.name);
            RenderTextureTools.AddRT(depthRT, depthRT.name);

            var targets = new[] { targetRT, targetRT1, targetRT2, targetRT3 };
            colorTargetNames = targets.Select(t => t.name).ToArray();
        }

        private void StartDraw(Renderer[] renders)
        {
            lastRenderersUVList = null;

            switch (texBatchType)
            {
                case TextureBatchType.TexAtlas:
                    // fill  lastRenderersUVList
                    StartTexAtlasFlow(renders);
                    break;
                case TextureBatchType.TexArr:
                    StartTexArrayFlow(renders);
                    break;
                case TextureBatchType.PerObject:
                    StartPerObjectFlow(renders);
                    break;
                case TextureBatchType.AllInOne:
                    StartAllInOneFlow(renders);
                    break;
                case TextureBatchType.AllInOneLightmapUV:
                    StartAllInOneLightmapUVFlow(renders);
                    break;
            }

            // try combines
            CombineRenderersAndSavePrefab(renders, lastRenderersUVList);
        }

        /// <summary>
        /// Render objects ,uv1 to lightmapUV
        /// </summary>
        /// <param name="renders"></param>
        void StartAllInOneLightmapUVFlow(Renderer[] renders)
        {
            //uvMode = UVMode.UV1;

            Shader.SetGlobalFloat(_UV1TransformToLightmapUV, 1);

            var texList = new List<Texture2D>();
            var groups = renders.GroupBy(r => r.lightmapIndex);
            foreach (var group in groups)
            {
                var groupName = $"{target.name}_Lightmap_{group.Key}";
                var groupRenders = group.ToArray();
                StartAllInOneFlow(groupRenders,groupName);

                foreach (var r in groupRenders)
                {
                    r.enabled = false;
                }
            }

        }

        void StartAllInOneFlow(Renderer[] renderers,string texName=null)
        {
            var list = new List<float>();
            foreach (Renderer render in renderers)
            {
                list.Add(render.sharedMaterial.GetFloat(_CullMode));
                if (isSetCullOff)
                    render.sharedMaterial.SetFloat(_CullMode, 0);

                render.enabled = true;
            }

            //Shader.SetGlobalFloat(_UV1TransformToLightmapUV, isUV1TransformToLightmapUV ? 1 : 0);
            bakeCam.Render();

            for (int i = 0; i < list.Count; i++)
            { 
                var lastCullMode = list[i];
                var render = renderers[i];
                render.sharedMaterial.SetFloat(_CullMode, lastCullMode);
            }

            // save
            SaveOutputTex(texName ?? target.name);
        }

        private void StartPerObjectFlow(Renderer[] renders)
        {
            for (int i = 0; i < renders.Length; i++)
            {
                ShowProgressBar(i, renders.Length);

                var render = renders[i];

                StartRenderObject(ref render);

                SaveOutputTex(render.name);
            }

            ClearProgressBar();
        }

        private void StartTexArrayFlow(Renderer[] renders)
        {
            var texList = RenderObjects_TexArray(renders);
            SaveTexArray(texList);
        }

        private void StartTexAtlasFlow(Renderer[] renders)
        {
            // bake objects 1 by 1
            lastRenderersUVList = RenderObjects_Atlas(renders);

            SaveOutputTex(target.name);
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

                StartRenderObject(ref render);
                
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
        void StartRenderObject(ref Renderer render)
        {
            render.enabled = true;
            var lastCullMode = render.sharedMaterial.GetFloat(_CullMode);
            var mat = render.sharedMaterial;
            if (isSetCullOff)
            {
                mat.SetFloat(_CullMode, 0);
            }


            bakeCam.Render();

            render.enabled = false;
            mat.SetFloat(_CullMode, lastCullMode);

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
                StartRenderObject(ref render);

                targetRT.ReadRenderTexture(ref sliceTex);
                sliceTex.Compress(true);
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
            SaveOutputTex(targetRT, targetName, TextureSuffix.C);
            SaveOutputTex(targetRT1, targetName, TextureSuffix.N);
            SaveOutputTex(targetRT2, targetName, TextureSuffix.PM);
            SaveOutputTex(targetRT3, targetName, TextureSuffix.E);
        }

        private void SaveOutputTex(RenderTexture rt,string targetName,TextureSuffix suffixName)
        {
            PathTools.CreateAbsFolderPath(outputPath);

            Texture2D tex = null;
            rt.ReadRenderTexture(ref tex, true);

            if (isHDR && texEncodeType != TextureEncodeType.EXR)
            {
                if (IsColorTexture(suffixName))
                    tex.ConvertColorSpace(colorConvertCS, "ConvertColorSpace");
            }
            //tex.Compress(true, TextureFormat.ASTC_HDR_6x6);

            var extName = texEncodeType.ToString();
            var filePath = $"{outputPath}/{targetName}_{suffixName}.{extName}";
            File.WriteAllBytes(filePath, tex.GetEncodeBytes(texEncodeType));
            tex.Destroy();

            if (isOutputTexAsset)
                SaveTexAsset(rt, targetName, suffixName);

            ReimportSavedTex(suffixName, filePath);

        }
        public static bool IsColorTexture(TextureSuffix suffix)
            => suffix == TextureSuffix.C || suffix == TextureSuffix.E;

        void SaveTexAsset(RenderTexture rt, string targetName, TextureSuffix suffixName)
        {
#if UNITY_EDITOR
            Texture2D tex = null;
            rt.ReadRenderTexture(ref tex);
            tex.Compress(true, outputTexAssetFormat);
            var assetPath = $"{outputPath}/{targetName}_{suffixName}.asset";
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(tex, assetPath);
#endif
        }

        void ReimportSavedTex(TextureSuffix suffixName, string filePath)
        {
#if UNITY_EDITOR
            Texture2D tex=null;

            AssetDatabase.Refresh();
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            tex?.Setting(texImp =>
            {
                if (suffixName == TextureSuffix.PM)
                {
                    texImp.sRGBTexture = false;
                }
                else if (suffixName == TextureSuffix.N)
                {
                    texImp.textureType = TextureImporterType.NormalMap;
                }

                texImp.SaveAndReimport();
            });
#endif
        }

        public static void ShowProgressBar(int i, int length)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Warning", "Bake PbrLighting", i / (float)length);
#endif
        }
        public static void ClearProgressBar()
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
            if (!target || !bakeCam)
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
            if(!target)
            {
                Debug.Log("select a target first");
                return;
            }
            lastShowRenderers = Object.FindObjectsByType<Renderer>(sortMode: FindObjectsSortMode.None);
            lastRendererCullModeList.Clear();

            foreach (var obj in lastShowRenderers)
            {
                obj.enabled = false;
            }

            var renders = target.GetComponentsInChildren<Renderer>(isIncludeInvisible);
            foreach (var render in renders)
            {
                lastRendererCullModeList.Add((render,render.sharedMaterial.GetFloat(_CullMode)));
                render.enabled = true;
                render.sharedMaterial.SetFloat(_CullMode, isSetCullOff ? 0 : 2);
            }

            Shader.SetGlobalFloat(_FullScreenOn, isShowFullscreen?1:0);
            Shader.SetGlobalFloat(_FullScreenUVId, isShowFullscreen ? (int)uvMode: 1);
            Shader.SetGlobalVector(_FullScreenUVRange,new Vector4(0,0,1,1));
            Shader.SetGlobalFloat(_UV1TransformToLightmapUV, isUV1TransformToLightmapUV ? 1 : 0);
        }

        void ResumeShowScene()
        {
            if (lastShowRenderers == null || !target)
                return;

            foreach ((Renderer r,float cullMode) info in lastRendererCullModeList)
            {
                info.r.sharedMaterial.SetFloat(_CullMode, info.cullMode);
            }

            foreach (var obj in lastShowRenderers)
            {
                obj.enabled = true;
            }

            lastShowRenderers = null;
            Shader.SetGlobalFloat(_FullScreenOn, 0);
            Shader.SetGlobalFloat(_UV1TransformToLightmapUV,0);
        }
        void ClearRTs()
        {
            var rts = new[] { targetRT, targetRT1, targetRT2, targetRT3, depthRT };
            foreach (var r in rts)
                r?.Destroy();
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
#if UNITY_EDITOR
            if (!isCombineMeshes || renders.Length <= 1)
            {
                //Debug.Log("dont need combine meshes, renders.Length == 0");
                return;
            }
            var mfList = renders.Select(r => r.GetComponent<MeshFilter>()).ToList();
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
        void SetCameraTargets()
        {
            // find sfc setRenderTarget
            
            setTargetFeatureInfos = GetSetRenderTargetFeatures(ref srpControl,ref setRenderTargetFeature);

            if (!srpControl) // not sfc
            {
                bakeCam.targetTexture = targetRT; // use main rt
                return;
            }

            SetEnableFeatures(setTargetFeatureInfos, false);

            if (!setRenderTargetFeature)
            {
                setRenderTargetFeature = SetRenderTarget.CreateInstance<SetRenderTarget>();
            }
            if(! srpControl.featureListSO.featureList.Contains(setRenderTargetFeature))
                srpControl.featureListSO.featureList.Add(setRenderTargetFeature);

            setRenderTargetFeature.isEditorOnly = true;
            setRenderTargetFeature.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            setRenderTargetFeature.name = setRenderTargetFeatureName;

            setRenderTargetFeature.colorTargetNames = colorTargetNames;
            setRenderTargetFeature.depthTargetName = depthTargetName;
            setRenderTargetFeature.enabled = true;

            setRenderTargetFeature.clearTarget = true;
        }

        private void BeforeDraw(Renderer[] allRenderers, out RenderTexture lastTarget, out Vector3 lastPos, out CameraClearFlags lastClearFlags)
        {
            foreach (var r in allRenderers)
            {
                if (r)
                    r.enabled = false;
            }

            Shader.globalMaximumLOD = int.MaxValue;
            Shader.SetGlobalFloat(_FullScreenOn, 1);
            Shader.SetGlobalFloat(_FullScreenUVId, (int)uvMode);
            Shader.SetGlobalVector(_FullScreenUVRange, new Vector4(0, 0, 1, 1));

            Shader.SetGlobalFloat(_OutputPbrMask, 1);
            Shader.SetGlobalFloat(_OutputNormal01, 1);
            Shader.SetGlobalFloat(_OutputEmission, 1);
            // keep
            lastTarget = bakeCam.targetTexture;
            lastPos = bakeCam.transform.position;
            lastClearFlags = bakeCam.clearFlags;

            bakeCam.clearFlags = cameraClearFlags;
            bakeCam.backgroundColor = backgroundColor;

            CalcBakeCamPos(out var bakePos, out var bakeForward);
            bakeCam.transform.position = bakePos;
            bakeCam.transform.forward = bakeForward;
        }

        private void AfterDraw(Renderer[] allRenderers)
        {
            Shader.SetGlobalFloat(_FullScreenOn, 0);
            Shader.SetGlobalFloat(_FullScreenUVId, 1);
            Shader.SetGlobalFloat(_UV1TransformToLightmapUV, 0);

            Shader.SetGlobalFloat(_OutputPbrMask, 0);
            Shader.SetGlobalFloat(_OutputNormal01, 0);
            Shader.SetGlobalFloat(_OutputEmission, 0);

            foreach (Renderer r in allRenderers)
            {
                if (r)
                    r.enabled = true;
            }
            RestoreFeatures(setTargetFeatureInfos);
        }

        (SRPFeature, bool)[] GetSetRenderTargetFeatures(ref SRPRenderFeatureControl srpControl, ref SetRenderTarget bakePbrLightingSetTarget)
        {
            //SRPPass<SetRenderTarget>.OnCanExecute += OnSetRenderTarget;
            srpControl = bakeCam.GetUniversalAdditionalCameraData()?.GetRendererData<UniversalRendererData>()?.GetFeature<SRPRenderFeatureControl>();
            var setRenderTargets = srpControl?.featureListSO?.featureList
                .Where(feature => feature is SetRenderTarget);

            var setTargetFeature = setRenderTargets.Where(info => info.name == setRenderTargetFeatureName).FirstOrDefault();
            if (setTargetFeature)
                bakePbrLightingSetTarget = (SetRenderTarget)setTargetFeature;

            return setRenderTargets.Select(feature => (feature, feature.enabled))
                .ToArray();
        }

        void SetEnableFeatures((SRPFeature feature,bool enabled)[] setRenderTargetInfos,bool isEnable)
        {
            if (setRenderTargetInfos == null)
                return;

            foreach (var info in setRenderTargetInfos)
            {
                info.feature.enabled = isEnable;
            }
        }
        void RestoreFeatures((SRPFeature feature, bool enabled)[] setRenderTargetInfos)
        {
            foreach (var info in setRenderTargetInfos)
            {
                info.feature.enabled = info.enabled;
            }

        }

    }
}