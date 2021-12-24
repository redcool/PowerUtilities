namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

#if UNITY_EDITOR
    using UnityEditor;
    using System.Linq;
    [CustomEditor(typeof(LightmapInfoRecorder))]
    public class LightmapInfosEidotr : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("Box");
            base.OnInspectorGUI();
            GUILayout.EndVertical();

            var inst = target as LightmapInfoRecorder;

            if (!inst.rootGo)
                return;

            if (GUILayout.Button("Record LightmapInfos"))
            {
                inst.RecordLightmapInfos();
                EditorUtility.SetDirty(inst);
            }

            if (GUILayout.Button("Apply LightmapInfos"))
            {
                inst.ApplyLightmapInfos();
            }

            if (GUILayout.Button("Clear LightmapInfo"))
            {
                foreach (var item in inst.renderers)
                {
                    item.lightmapIndex = -1;
                    item.lightmapScaleOffset = Vector4.zero;
                }
            }
        }


    }
    public class LightmapInfoDetector
    {
        [InitializeOnLoadMethod]
        public static void AddlightmapCallback()
        {
            Lightmapping.bakeCompleted -= OnLightmapBakedDone;
            Lightmapping.bakeCompleted += OnLightmapBakedDone;
        }

        static void OnLightmapBakedDone()
        {
            var recorder = GameObject.FindObjectOfType<LightmapInfoRecorder>();
            if (!recorder)
                return;

            recorder.RecordLightmapInfos();
            EditorUtility.SetDirty(recorder);
        }
    }
#endif

    /// <summary>
    /// save rootGo children Renderer lightmapInfo 
    /// </summary>
    public class LightmapInfoRecorder : MonoBehaviour
    {

        [Header("LightmapInfo")]
        public GameObject rootGo;

        public MeshRenderer[] renderers;
        public Vector4[] lightmapUVs;
        public int[] lightmapIds;

        public bool isAutoLoad = true;

        void Start()
        {
            if (isAutoLoad)
                ApplyLightmapInfos(renderers, lightmapUVs, lightmapIds);
        }

#if UNITY_EDITOR

        public static void RecordLightmapInfos(MeshRenderer[] renderers, out Vector4[] lightmapUVs, out int[] lightmapIds)
        {
            if (renderers == null)
                throw new Exception("Renderers is Null !");

            lightmapUVs = new Vector4[renderers.Length];
            lightmapIds = new int[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                var item = renderers[i];
                lightmapUVs[i] = item.lightmapScaleOffset;
                lightmapIds[i] = item.lightmapIndex;
            }
        }

#endif

        public static void ApplyLightmapInfos(MeshRenderer[] renderers, Vector4[] lightmapUVs, int[] lightmapIds)
        {
            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                var item = renderers[i];
                if (!item)
                    continue;

                item.lightmapIndex = lightmapIds[i];
                item.lightmapScaleOffset = lightmapUVs[i];
            }
        }

#if UNITY_EDITOR
        public void RecordLightmapInfos()
        {
            if (!rootGo)
                rootGo = gameObject;

            renderers = rootGo.GetComponentsInChildren<MeshRenderer>();
            if (renderers != null && renderers.Length > 0)
            {
                renderers = renderers.Where
                    (item => GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject,
                        StaticEditorFlags.ContributeGI))
                    .ToArray();
                RecordLightmapInfos(renderers, out lightmapUVs, out lightmapIds);
            }
        }
#endif

        public void ApplyLightmapInfos()
        {
            ApplyLightmapInfos(renderers, lightmapUVs, lightmapIds);
        }

    }
}