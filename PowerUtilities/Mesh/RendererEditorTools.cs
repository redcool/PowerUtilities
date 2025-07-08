# if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class RendererEditorTools
    {

        /// <summary>
        /// Get sortingLayerEditorUtilityType type
        /// Get renderSortingLayerFields method
        /// </summary>
        /// <param name="sortingLayerEditorUtilityType"></param>
        /// <param name="renderSortingLayerFields"></param>
        public static void GetSortingLayerEditorUtility(ref Type sortingLayerEditorUtilityType, ref MethodInfo renderSortingLayerFields)
        {
            EditorTools.GetTypeFromEditorAssembly(ref sortingLayerEditorUtilityType, "UnityEditor.SortingLayerEditorUtility");
            var argTypes = new Type[] { typeof(SerializedProperty), typeof(SerializedProperty) };
            sortingLayerEditorUtilityType.GetMethod(ref renderSortingLayerFields, "RenderSortingLayerFields", ReflectionTools.callBindings, null, argTypes, null);
        }

        /// <summary>
        /// Gui method Call renderSortingLayerFields, 
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="renderSortingLayerFields"></param>
        /// <param name="isShowSortingLayerSetting"></param>
        /// <param name="sortingLayerSettingGUI"></param>
        public static void DrawRenderSortingLayerFields(SerializedObject serializedObject, MethodInfo renderSortingLayerFields, ref bool isShowSortingLayerSetting, GUIContent sortingLayerSettingGUI)
        {
            //var r = (Renderer)serializedObject.targetObject;

            var sortingLayerIdProp = serializedObject.FindProperty("m_SortingLayerID");
            var sorintgOrderProp = serializedObject.FindProperty("m_SortingOrder");
            //var sortingLayerName = SortingLayer.IDToName(sortingLayerIdProp.intValue);

            serializedObject.Update();
            if (isShowSortingLayerSetting = EditorGUILayout.BeginFoldoutHeaderGroup(isShowSortingLayerSetting, sortingLayerSettingGUI))
            {
                DelegateEx.GetOrCreate<Action<SerializedProperty, SerializedProperty>>(null, renderSortingLayerFields)
                    .Invoke(sorintgOrderProp, sortingLayerIdProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif