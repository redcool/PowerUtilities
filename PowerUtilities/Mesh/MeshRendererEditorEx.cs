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
            RendererEditorTools.DrawRenderSortingLayerFields(serializedObject, renderSortingLayerFields, ref isShowSortingLayerSetting, sortingLayerSettingGUI);
        }


    }
}
#endif