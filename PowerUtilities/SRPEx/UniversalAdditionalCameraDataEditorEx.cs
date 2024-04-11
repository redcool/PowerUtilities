#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    [CanEditMultipleObjects]
    // open some ui for convenient
    [CustomEditor(typeof(UniversalAdditionalCameraData))]
    class UniversalAdditionalCameraDataEditorEx : Editor
    {

        readonly GUIContent CamerasContent = new GUIContent("Cameras", "setup overlay cameras,will sync CameraEditor");

        TransparencySortMode transparentSortMode;
        Vector3 transparentSortAxis = Vector3.forward;

        OpaqueSortMode opaqueSortMode;

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.indentLevel++;
            DrawCameras();


            DrawCameraSortMode();

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCameraSortMode()
        {
            var data = target as UniversalAdditionalCameraData;
            var cam = data.GetComponent<Camera>();

            EditorGUILayout.LabelField("Sort Mode", EditorStyles.boldLabel);
            transparentSortMode = (TransparencySortMode)EditorGUILayout.EnumPopup("TransparentSortMode", transparentSortMode);
            cam.transparencySortMode = transparentSortMode;

            if (transparentSortMode == TransparencySortMode.CustomAxis)
            {
                transparentSortAxis = EditorGUILayout.Vector3Field("transparentSortAxis", transparentSortAxis);
                cam.transparencySortAxis = transparentSortAxis;
            }


            opaqueSortMode = (OpaqueSortMode)EditorGUILayout.EnumPopup("opaqueSortMode", opaqueSortMode);
            cam.opaqueSortMode = opaqueSortMode;
        }

        private void DrawCameras()
        {
            var m_Cameras = serializedObject.FindProperty("m_Cameras");
            EditorGUILayout.PropertyField(m_Cameras, CamerasContent);

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