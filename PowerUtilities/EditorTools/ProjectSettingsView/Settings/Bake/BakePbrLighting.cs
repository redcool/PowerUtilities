#if UNITY_EDITOR
using PowerUtilities.Coroutine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;
using WaitForEndOfFrame = PowerUtilities.Coroutine.WaitForEndOfFrame;

namespace PowerUtilities
{

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

        [EditorDisableGroup(targetPropName = nameof(isUseSceneCamPos),isRevertMode =true)]
        [Tooltip("bake camera's position")]
        public float camDistance = 10;

        [Tooltip("bake camera's position use sceneView camera position")]
        public bool isUseSceneCamPos = false;

        [EditorButton(onClickCall = "BakeLighting")]
        public bool isBake;

        [EditorGroup("Debug",true)]
        public RenderTexture rt;
        [EditorGroup("Debug")]
        public Texture2D tex;

        public void BakeLighting()
        {
            if (!target)
            {
                target = Selection.activeGameObject;
                if(!target)
                {
                    //EditorTools.DisplayDialog_Ok_Cancel("select a gameobject");
                    Debug.LogError("select a gameobject");
                    return;
                }
            }


            TryCreateRT(ref rt);

            var allObjs = Object.FindObjectsByType<Renderer>(sortMode: FindObjectsSortMode.None);

            //======================= begin render
            Camera cam;
            BeforeDraw(allObjs, out cam, out var lastTarget,out var lastPos,out var lastClearFlags);

            cam.Render();

            //======================= after render
            AfterDraw(allObjs);
            cam.targetTexture = lastTarget;
            cam.transform.position = lastPos;
            cam.clearFlags = lastClearFlags;

            SaveTex();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnGizmos;
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnGizmos;
        }

        void OnGizmos(SceneView sceneView)
        {
            if (!target)
                return;
            var cam = Camera.main;
            var camBakePos = target.transform.position - cam.transform.forward * camDistance;
            Handles.color = Color.magenta;
            Handles.DrawLine(camBakePos, target.transform.position);

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
            rt.ReadRenderTexture(ref tex,true);
            //tex.Apply();

            PathTools.CreateAbsFolderPath(outputPath);
            File.WriteAllBytes($"{outputPath}/{target.name}.png", tex.EncodeToPNG());

            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(outputPath);
        }

        private void BeforeDraw(Renderer[] allObjs, out Camera cam, out RenderTexture lastTarget, out Vector3 lastPos, out CameraClearFlags lastClearFlags)
        {
            foreach (var obj in allObjs)
            {
                obj.enabled = false;
            }

            var renders = target.GetComponentsInChildren<Renderer>(isIncludeInvisible);
            foreach (var render in renders)
                render.enabled = true;

            Shader.SetGlobalFloat("_FullScreenOn", 1);

            cam = Camera.main;
            // keep
            lastTarget = cam.targetTexture;
            lastPos = cam.transform.position;
            lastClearFlags = cam.clearFlags;

            cam.targetTexture = rt;
            cam.clearFlags = cameraClearFlags;
            cam.backgroundColor = backgroundColor;

            cam.transform.position = target.transform.position - cam.transform.forward * camDistance;
            if (isUseSceneCamPos)
            {

                var sceneCam = Camera.allCameras.Where(cam => cam.cameraType == CameraType.SceneView).FirstOrDefault();
                if (sceneCam)
                {
                    cam.transform.position = sceneCam.transform.position;
                    cam.transform.forward = sceneCam.transform.forward;
                }
            }
        }

        private void TryCreateRT(ref RenderTexture rt)
        {
            var w = (int)resolution;
            if (!rt)
            {
                rt = new RenderTexture(w, w, 32, GraphicsFormatTools.GetColorTextureFormat(true,false));
            }

            if (!tex)
            {
                tex = new Texture2D(w, w, TextureFormat.ARGB32,false,true);
            }
        }


    }
}
#endif