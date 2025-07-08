#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    //[CustomEditor(typeof(SpriteRenderer))]
    public class SpriteRendererEditorEx : Editor
    {
        Type spriteRendererEditorType;
        Editor spriteRendererEditor;

        GUIContent ExSettingText = new GUIContent("Ex Settings");
        bool isFold;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorTools. GetOrCreateUnityEditor(ref spriteRendererEditor,targets,ref spriteRendererEditorType, "SpriteRendererEditor");
            spriteRendererEditor?.OnInspectorGUI();


            isFold = EditorGUILayout.BeginFoldoutHeaderGroup(isFold, ExSettingText);
            if (isFold)
            {
                EditorGUI.indentLevel++;
                if (GUILayout.Button("Test"))
                {

                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif