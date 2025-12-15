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
    /// <summary>
    /// Extends Unity MeshRenderer Editor
    /// </summary>
    [CustomEditor(typeof(MeshRenderer))]
    [CanEditMultipleObjects]
    public class MeshRendererEditorEx : BaseEditorEx
    {
        readonly GUIContent
            sortingLayerSettingGUI = new GUIContent("SortingLayer", "SortingLayer,order"),
            lightmapIndexGUI = new GUIContent("LightmapIndex", "lightmap Index"),
            lightmapScaleOffsetGUI = new GUIContent("LightmapScaleOffset", "Lightmap Scale Offset"),
            showLightmapSettingGUI = new GUIContent("LightmapSetting","lightmap setting")
            ;
        

        // SortingLayerEditorUtility
        public Type sortingLayerEditorUtilityType;
        MethodInfo renderSortingLayerFields;

        public bool isShowSortingLayerSetting;

        // lightmap
        public bool isShowLightmapSetting;
        public bool isShowBounds;

        public override string GetDefaultInspectorTypeName() => "UnityEditor.MeshRendererEditor";

        public override void OnEnable()
        {
            base.OnEnable();

            // get SortingLayerEditorUtility
            RendererEditorTools.GetSortingLayerEditorUtility(ref sortingLayerEditorUtilityType, ref renderSortingLayerFields);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var mr = (MeshRenderer)target;

            serializedObject.Update();
            RendererEditorTools.DrawRenderSortingLayerFields(serializedObject, renderSortingLayerFields, ref isShowSortingLayerSetting, sortingLayerSettingGUI);
            DrawLightmapSetting();
            DrawBounds(mr);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBounds(MeshRenderer mr)
        {
            //var boundsProp = serializedObject.FindProperty("m_Bounds");
            var bounds = mr.bounds;
            EditorGUITools.BeginFoldoutHeaderGroupBox(ref isShowBounds, new GUIContent("Bounds"), () =>
            {
                //EditorGUILayout.PropertyField(boundsProp);
                bounds.center = EditorGUILayout.Vector3Field("Center", bounds.center);
                bounds.size = EditorGUILayout.Vector3Field("Size", bounds.size);
                DebugTools.DrawLineCube(bounds, Color.green,5);
            });
        }

        public void DrawLightmapSetting()
        {
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