namespace PowerUtilities.RenderFeatures
{
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
    using UnityEngine.SceneManagement;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(SRPRenderFeatureControl))]
    public class SRPRenderFeatureControlEditor : Editor
    {
        Editor featureListEditor;
        private void OnEnable()
        {
            var featureListSO = serializedObject.FindProperty(nameof(SRPRenderFeatureControl.featureListSO));
            featureListEditor = CreateEditor(featureListSO.objectReferenceValue);
        }

        public override void OnInspectorGUI()
        {
            var inst = serializedObject.targetObject as SRPRenderFeatureControl;

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                OnEnable();
            }

            serializedObject.UpdateIfRequiredOrScript();

            if (inst.featureListSO == null)
            {
                if (GUILayout.Button("Create SRPFeatureListSO"))
                {
                    CreateNewFeatureListAsset(inst);
                }
            }
            else
            {
                var isFeatureListFoldout = serializedObject.FindProperty("isFeatureListFoldout");
                isFeatureListFoldout.boolValue = EditorGUILayout.Foldout(isFeatureListFoldout.boolValue, EditorGUITools.TempContent("featureList Details"), true, EditorStylesEx.FoldoutHeader);
                if (isFeatureListFoldout.boolValue)
                {
                    EditorGUI.indentLevel++;
                    featureListEditor?.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void CreateNewFeatureListAsset(SRPRenderFeatureControl inst)
        {
            var listSO = ScriptableObject.CreateInstance<SRPFeatureListSO>();
            var scene = SceneManager.GetActiveScene();
            var path = $"Assets/{scene.name}_FeatureList.asset";

            var listAsset = AssetDatabaseTools.CreateAssetThenLoad<SRPFeatureListSO>(listSO, path);
            EditorGUIUtility.PingObject(listAsset);
            inst.featureListSO = listAsset;
        }
    }
#endif

    public class SRPRenderFeatureControl : ScriptableRendererFeature
    {
        public SRPFeatureListSO featureListSO;

        [SerializeField]
        [HideInInspector]
        bool isFeatureListFoldout;


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if ( featureListSO == null || featureListSO.featureList.Count == 0 || !featureListSO.enabled)
                return;

            // cached last use instance
            SRPFeatureListSO.instance = featureListSO;
            ref var cameraData = ref renderingData.cameraData;

            foreach (var feature in featureListSO.featureList)
            {
                if (feature == null)
                    continue;

                var pass = feature.PassInstance;
                if (pass == null || !feature.enabled)
                    continue;

                if (feature.isSceneCameraOnly && cameraData.camera.cameraType != CameraType.SceneView)
                    continue;

                pass.renderPassEvent = feature.renderPassEvent + feature.renderPassEventOffset;
                renderer.EnqueuePass(pass);

                if (feature.interrupt)
                    break;
            }
            
        }

        public override void Create()
        {
        }
    }
}
