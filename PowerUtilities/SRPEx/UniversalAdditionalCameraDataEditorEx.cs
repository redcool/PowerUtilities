#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    [CanEditMultipleObjects]
    // open some ui for convenient
    [CustomEditor(typeof(UniversalAdditionalCameraData))]
    class UniversalAdditionalCameraDataEditorEx : Editor
    {

        readonly GUIContent CamerasContent = new GUIContent("Camera Stack", "setup overlay cameras,will sync CameraEditor");

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.indentLevel++;
            DrawCameras();

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCameras()
        {
            var m_Cameras = serializedObject.FindProperty("m_Cameras");
            EditorGUILayout.PropertyField(m_Cameras, CamerasContent);

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