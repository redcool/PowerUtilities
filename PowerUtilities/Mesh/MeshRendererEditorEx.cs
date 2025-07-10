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
        readonly GUIContent
            sortingLayerSettingGUI = new GUIContent("SortingLayer", "SortingLayer,order"),
            lightmapIndexGUI = new GUIContent("LightmapIndex", "lightmap Index"),
            lightmapScaleOffsetGUI = new GUIContent("LightmapScaleOffset", "Lightmap Scale Offset"),
            showLightmapSettingGUI = new GUIContent("LightmapSetting","lightmap setting")
            ;
        

        public Type meshRendererType;
        public Editor meshRendererEditor;

        MethodInfo onEnable, onInspectorGUI;

        // SortingLayerEditorUtility
        public Type sortingLayerEditorUtilityType;
        MethodInfo renderSortingLayerFields;

        public bool isShowSortingLayerSetting;

        // lightmap
        public bool isShowLightmapSetting;

        private void OnEnable()
        {
            // get MeshRendererEditor
            EditorTools.GetOrCreateUnityEditor(ref meshRendererEditor, targets, ref meshRendererType, "UnityEditor.MeshRendererEditor");

            meshRendererType.GetMethod(ref onEnable, nameof(OnEnable), ReflectionTools.callBindings);
            meshRendererType.GetMethod(ref onInspectorGUI, nameof(OnInspectorGUI), ReflectionTools.callBindings);

            DelegateEx.GetOrCreate<Action>(meshRendererEditor, onEnable).Invoke();
            // get SortingLayerEditorUtility
            RendererEditorTools.GetSortingLayerEditorUtility(ref sortingLayerEditorUtilityType, ref renderSortingLayerFields);
        }

        public override void OnInspectorGUI()
        {
            if (onInspectorGUI == null)
                OnEnable();
            
            // call default
            DelegateEx.GetOrCreate<Action>(meshRendererEditor, onInspectorGUI).Invoke();

            EditorGUITools.DrawColorLine(1);
            serializedObject.Update();
            RendererEditorTools.DrawRenderSortingLayerFields(serializedObject, renderSortingLayerFields, ref isShowSortingLayerSetting, sortingLayerSettingGUI);
            DrawLightmapSetting();
            serializedObject.ApplyModifiedProperties();
        }

        public void DrawLightmapSetting()
        {
            var mr = (MeshRenderer)target;
            var lightmapIndexProp = serializedObject.FindProperty("m_LightmapIndex");
            var lightmapScaleOffsetProp = serializedObject.FindProperty("m_LightmapTilingOffset");
            EditorGUITools.BeginFoldoutHeaderGroupBox(ref isShowLightmapSetting, showLightmapSettingGUI, () =>
            {
                EditorGUILayout.PropertyField(lightmapIndexProp);
                EditorGUILayout.PropertyField(lightmapScaleOffsetProp);
            });
        }
    }
}
#endif