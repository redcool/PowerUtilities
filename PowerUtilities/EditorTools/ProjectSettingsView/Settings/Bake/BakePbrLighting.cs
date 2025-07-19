#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.IO;
    using System.Linq;

    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [CustomEditor(typeof(BakePbrLighting))]
    public class BakePbrLightingEditor : PowerEditor<BakePbrLighting>
    {
        public override bool NeedDrawDefaultUI()
        => true;


    }


    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Bake/BakePbrLighting")]
    [SOAssetPath("Assets/PowerUtilities/BakePbrLighting.asset")]

    public class BakePbrLighting : ScriptableObject
    {
        [HelpBox]
        public string helpBox = "Bake pbr lighting to texture";

        [Tooltip("select target for bake")]
        [EditorObjectField]
        public GameObject target;

        [Tooltip("baked texture path for save")]
        public string outputPath = "Assets/PowerUtilities/BakeLighting";

        [Tooltip("bake texture's resolution")]
        public TextureResolution resolution = TextureResolution.x2048;

        [Tooltip("target children renderer include  invisible ")]
        public bool isIncludeInvisible;

        [Tooltip("bake camera clear color")]
        public Color backgroundColor = Color.clear;
        public CameraClearFlags cameraClearFlags = CameraClearFlags.SolidColor;

        //[EditorDisableGroup(targetPropName = nameof(isUseSceneCamPos), isRevertMode = true)]
        [Tooltip("bake camera's position")]
        public float camDistance = 10;

        [Tooltip("bake lighting use sceneView camera or main camera(Tag:MainCamera)")]
        public bool isUseSceneCamPos = false;

        [EditorButton(onClickCall = "BakeLighting")]
        public bool isBake;

        [EditorGroup("Debug", true)]
        public RenderTexture rt;
        [EditorGroup("Debug")]
        public Texture2D tex;

        Camera sceneCam;
        Camera bakeCam;

        public void BakeLighting()
        {
            if (!target)
            {
                target = Selection.activeGameObject;
                if (!target)
                {
                    //EditorTools.DisplayDialog_Ok_Cancel("select a gameobject");
                    Debug.LogError("select a gameobject");
                    return;
                }
            }

            if (isUseSceneCamPos && sceneCam)
                bakeCam = sceneCam;


            TryCreateRT(ref rt);

            var allObjs = Object.FindObjectsByType<Renderer>(sortMode: FindObjectsSortMode.None);

            //======================= begin render
            BeforeDraw(allObjs, out var lastTarget, out var lastPos, out var lastClearFlags);

            bakeCam.Render();

            //======================= after render
            AfterDraw(allObjs);
            bakeCam.targetTexture = lastTarget;
            bakeCam.transform.position = lastPos;
            bakeCam.clearFlags = lastClearFlags;

            SaveTex();
        }

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

        public static void TryGetFirstSceneView(ref SceneView sv)
        {
            if (sv || SceneView.sceneViews.Count == 0)
                return;

            sv = (SceneView)SceneView.sceneViews[0];
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnGizmos;
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

        private void AfterDraw(Renderer[] allObjs)
        {
            //cam.clearFlags = CameraClearFlags.Skybox;
            Shader.SetGlobalFloat("_FullScreenOn", 0);

            foreach (var obj in allObjs)
                obj.enabled = true;
        }

        private void SaveTex()
        {
            rt.ReadRenderTexture(ref tex, true);
            //tex.Apply();

            PathTools.CreateAbsFolderPath(outputPath);
            File.WriteAllBytes($"{outputPath}/{target.name}.png", tex.EncodeToPNG());

            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(outputPath);
        }

        void CalcBakeCamPos(out Vector3 camPos, out Vector3 camForward)
        {
            camPos = target.transform.position - bakeCam.transform.forward * camDistance;
            camForward = bakeCam.transform.forward;

            if (isUseSceneCamPos && sceneCam)
            {
                camPos = sceneCam.transform.position + sceneCam.transform.forward*0.1f;
                camForward = sceneCam.transform.forward;
            }
        }

        private void BeforeDraw(Renderer[] allObjs, out RenderTexture lastTarget, out Vector3 lastPos, out CameraClearFlags lastClearFlags)
        {
            foreach (var obj in allObjs)
            {
                obj.enabled = false;
            }

            var renders = target.GetComponentsInChildren<Renderer>(isIncludeInvisible);
            foreach (var render in renders)
                render.enabled = true;

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
#endif