#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// Extends UnityEditor.Texture3D
    /// </summary>
    [CustomEditor(typeof(Texture3D))]
    [CanEditMultipleObjects]
    public class Texture3DInspectorEx : BaseEditorEx
    {

        public override string GetDefaultInspectorTypeName() => "UnityEditor.Texture3DInspector";
        [EnumSearchable(typeof(TextureFormat))]
        public TextureFormat format;

        public List<Texture2D> slices = new();
        ReorderableList slicesRList;

        SerializedProperty formatProp, filterModeProp, wrapUProp, wrapVProp, wrapWProp, colorSpaceProp;
        static GUIContent
            GUI_DUMP = new GUIContent("Dump", "dump slices to disk"),
            GUI_COMBINE = new GUIContent("Combine", "combine slices generate new texArr");

        Texture3D inst;
        public override void OnEnable()
        {
            base.OnEnable();
            formatProp = serializedObject.FindProperty("m_Format");
            wrapUProp = serializedObject.FindProperty("m_TextureSettings.m_WrapU");
            wrapVProp = serializedObject.FindProperty("m_TextureSettings.m_WrapV");
            wrapWProp = serializedObject.FindProperty("m_TextureSettings.m_WrapW");
            colorSpaceProp = serializedObject.FindProperty("m_ColorSpace");


            inst = target as Texture3D;
            slicesRList = EditorGUITools.SetupSlicesRList(slices);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            slices.Clear();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // draw 
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();
            EditorGUITools.EnumPropertyFieldSearchable(formatProp, typeof(GraphicsFormat), (SerializedProperty prop, int formatId) =>
            {
                if (formatProp.intValue == formatId)
                    return;
                Texture2DArrayInspectorEx.ChangeTextureFormat(inst, formatId);
            });

            wrapUProp.intValue = (int)(WrapMode)EditorGUILayout.EnumPopup(wrapUProp.displayName, (WrapMode)wrapUProp.intValue);
            wrapVProp.intValue = (int)(WrapMode)EditorGUILayout.EnumPopup(wrapVProp.displayName, (WrapMode)wrapVProp.intValue);
            wrapWProp.intValue = (int)(WrapMode)EditorGUILayout.EnumPopup(wrapWProp.displayName, (WrapMode)wrapWProp.intValue);


            slicesRList.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUITools.BeginHorizontalBox(() =>
            {
                if (GUILayout.Button(GUI_DUMP))
                {
                    Texture2DArrayInspectorEx.DumpSlicesToDisk(inst,slices);
                }

                if (GUILayout.Button(GUI_COMBINE))
                {
                    Texture2DArrayInspectorEx.CombineSlices(inst,slices);
                }
            });

        }
    }
}
#endif