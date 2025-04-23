#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    [CanEditMultipleObjects]
    // open some ui for convenient
    [CustomEditor(typeof(UniversalAdditionalCameraData))]
    class UniversalAdditionalCameraDataEditorEx : Editor
    {
        readonly GUIContent
            rendererGUI = new GUIContent("Renderer","srp renderer current used"),
            transparentSortModeContent = new GUIContent("TransparentSortMode", "transparent items sorting mode"),
            transparentSortAxis = new GUIContent("transparentSortAxis", "transparentSort Axis"),
            opaqueSortModeContent = new GUIContent("opaqueSortMode", "opaque items sorting mode"),
            cameraDepthContent = new GUIContent("Depth", "camera 's rendering order"),
            camerasContent = new GUIContent("Cameras", "setup overlay cameras,will sync CameraEditor"),
            opaqueTextureContent = new GUIContent("Opaque Texture", "setup opaque Texture override"),
            showDefaultSettings = new GUIContent("Default Camera Settings", "show all AdditionalCameraData properties");

        bool isFoldDefaultSettings;

        public override void OnInspectorGUI()
        {
            var data = target as UniversalAdditionalCameraData;
            var cam = data.GetComponent<Camera>();


            EditorGUI.indentLevel++;
            EditorGUITools.DrawFoldContent(showDefaultSettings, ref isFoldDefaultSettings, () =>
            {
                DrawDefaultInspector();
            });

            EditorGUITools.DrawColorLine(1);

            serializedObject.UpdateIfRequiredOrScript();

            DrawCameras();

            DrawCameraSortMode(data,cam);

            // draw depth
            cam.depth = EditorGUILayout.FloatField(cameraDepthContent, cam.depth);

            // show opaque texture
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RequiresOpaqueTextureOption"), opaqueTextureContent);


            serializedObject.ApplyModifiedProperties();
            /**
             dont modify serializedObject
             */
            // srp renderer
            var rendererId = serializedObject.FindProperty("m_RendererData");
            var rendererData = data.GetRendererData<UniversalRendererData>();
            EditorGUILayout.ObjectField(rendererGUI, rendererData, typeof(UniversalRendererData), true);


            EditorGUI.indentLevel--;
        }

        private void DrawCameraSortMode(UniversalAdditionalCameraData data, Camera cam)
        {
            EditorGUILayout.LabelField("Sort Mode", EditorStyles.boldLabel);
            cam.transparencySortMode = (TransparencySortMode)EditorGUILayout.EnumPopup(transparentSortModeContent, cam.transparencySortMode);

            if (cam.transparencySortMode == TransparencySortMode.CustomAxis)
            {
                cam.transparencySortAxis = EditorGUILayout.Vector3Field(transparentSortAxis, cam.transparencySortAxis);
            }

            cam.opaqueSortMode = (OpaqueSortMode)EditorGUILayout.EnumPopup(opaqueSortModeContent, cam.opaqueSortMode);
        }

        private void DrawCameras()
        {
            var m_Cameras = serializedObject.FindProperty("m_Cameras");
            EditorGUILayout.PropertyField(m_Cameras, camerasContent);

            m_Cameras.RemoveDuplicateItems();

            //change Overlay
            for (int i = 0; i < m_Cameras.arraySize; i++)
            {
                var prop = m_Cameras.GetArrayElementAtIndex(i);
                if(prop.objectReferenceValue is Camera cam)
                {
                    var data = cam.GetComponent<UniversalAdditionalCameraData>();
                    data.renderType = CameraRenderType.Overlay;
                }
            }
        }
    }
}
#endif