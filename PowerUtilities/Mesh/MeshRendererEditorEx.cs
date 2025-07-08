#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace PowerUtilities
{
    [CustomEditor(typeof(MeshRenderer))]
    [CanEditMultipleObjects]
    public class MeshRendererEditorEx : Editor
    {
        public Type meshRendererType;
        public Editor meshRendererEditor;

        MethodInfo onEnable, onInspectorGUI;

        // SortingLayerEditorUtility
        public Type sortingLayerEditorUtilityType;
        MethodInfo renderSortingLayerFields;

        public bool isShowSortingLayerSetting;
        GUIContent sortingLayerSettingGUI = new GUIContent("SortingLayer", "SortingLayer,order");

        private void OnEnable()
        {
            // get MeshRendererEditor
            EditorTools.GetOrCreateUnityEditor(ref meshRendererEditor, targets, ref meshRendererType, "UnityEditor.MeshRendererEditor");

            meshRendererType.GetMethod(ref onEnable,nameof(OnEnable),ReflectionTools.callBindings);
            meshRendererType.GetMethod(ref onInspectorGUI, nameof(OnInspectorGUI), ReflectionTools.callBindings);

            DelegateEx.GetOrCreate<Action>(meshRendererEditor, onEnable).Invoke();

            // get SortingLayerEditorUtility
            EditorTools.GetTypeFromEditorAssembly(ref sortingLayerEditorUtilityType, "UnityEditor.SortingLayerEditorUtility");
            var argTypes = new Type[] { typeof(SerializedProperty), typeof(SerializedProperty) };
            sortingLayerEditorUtilityType.GetMethod(ref renderSortingLayerFields, "RenderSortingLayerFields", ReflectionTools.callBindings,null,argTypes,null);

            DelegateEx.GetOrCreate<Action<SerializedProperty, SerializedProperty>>(null, renderSortingLayerFields);
        }

        public override void OnInspectorGUI()
        {
            if (onInspectorGUI == null)
                OnEnable();

            // call default
            DelegateEx.GetOrCreate<Action>(meshRendererEditor, onInspectorGUI).Invoke();

            DrawInspectorGUI();
        }

        private void DrawInspectorGUI()
        {
            EditorGUITools.DrawColorLine(1);
            var mr = (MeshRenderer)target;

            var sortingLayerIdProp = serializedObject.FindProperty("m_SortingLayerID");
            var sorintgOrderProp = serializedObject.FindProperty("m_SortingOrder");
            var sortingLayerName = SortingLayer.IDToName(sortingLayerIdProp.intValue);

            serializedObject.Update();
            if (isShowSortingLayerSetting = EditorGUILayout.BeginFoldoutHeaderGroup(isShowSortingLayerSetting, sortingLayerSettingGUI))
            {
                renderSortingLayerFields.Invoke(null, new[] { sorintgOrderProp, sortingLayerIdProp });
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif