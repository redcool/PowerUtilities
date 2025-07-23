#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// Extends UnityEditor.Texture2DArrayInspector
    /// </summary>
    [CustomEditor(typeof(Texture2DArray))]
    [CanEditMultipleObjects]
    public class Texture2DArrayInspectorEx : BaseEditorEx
    {

        public override string GetDefaultInspectorTypeName() => "UnityEditor.Texture2DArrayInspector";

        [EnumSearchable(typeof(TextureFormat))]
        public TextureFormat format;

        SerializedProperty formatProp,filterModeProp,wrapUProp, wrapVProp, wrapWProp,colorSpaceProp;
        public override void OnEnable()
        {
            base.OnEnable();

            formatProp = serializedObject.FindProperty("m_Format");
            wrapUProp = serializedObject.FindProperty("m_TextureSettings.m_WrapU");
            wrapVProp = serializedObject.FindProperty("m_TextureSettings.m_WrapV");
            wrapWProp = serializedObject.FindProperty("m_TextureSettings.m_WrapW");
            colorSpaceProp = serializedObject.FindProperty("m_ColorSpace");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // draw 
            var inst = (Texture2DArray)target;
            
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUITools.EnumPropertyFieldSearchable(formatProp, typeof(GraphicsFormat));

            //colorSpaceProp.intValue =  (int)(ColorSpace)EditorGUILayout.EnumPopup(colorSpaceProp.displayName,(ColorSpace)colorSpaceProp.intValue);

            wrapUProp.intValue = (int)(WrapMode)EditorGUILayout.EnumPopup(wrapUProp.displayName, (WrapMode)wrapUProp.intValue);
            wrapVProp.intValue = (int)(WrapMode)EditorGUILayout.EnumPopup(wrapVProp.displayName, (WrapMode)wrapVProp.intValue);
            wrapWProp.intValue = (int)(WrapMode)EditorGUILayout.EnumPopup(wrapWProp.displayName, (WrapMode)wrapWProp.intValue);
            //EditorGUITools.EnumPropertyFieldSearchable(wrapUProp, typeof(WrapMode));
            //EditorGUITools.EnumPropertyFieldSearchable(wrapVProp, typeof(WrapMode));
            //EditorGUITools.EnumPropertyFieldSearchable(wrapWProp, typeof(WrapMode));


            serializedObject.ApplyModifiedProperties();
        }

        //public static void DoFilterModePopup<T>(SerializedProperty filterModeProp,T enumType) where T : Enum
        //{
        //    EditorGUI.BeginChangeCheck();
        //    EditorGUI.showMixedValue = filterModeProp.hasMultipleDifferentValues;
        //    var intValue = filterModeProp.intValue;
        //    intValue = (T)EditorGUILayout.EnumPopup(EditorGUIUtility.TrTempContent("Filter Mode"), (T)intValue);
        //    EditorGUI.showMixedValue = false;
        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        filterModeProp.intValue = (int)intValue;
        //    }
        //}
    }
}
#endif