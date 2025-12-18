#if UNITY_EDITOR
using Microsoft.CodeAnalysis.Text;
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
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEngine.Networking.UnityWebRequest;
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

        public List<Texture2D> slices = new() ;
        ReorderableList slicesRList;

        SerializedProperty formatProp,filterModeProp,wrapUProp, wrapVProp, wrapWProp,colorSpaceProp;

        static GUIContent
            GUI_DUMP = new GUIContent("Dump","dump slices to disk"),
            GUI_COMBINE = new GUIContent("Combine", "combine slices generate new texArr");

        Texture2DArray inst;
        int lastFormatId;
        public override void OnEnable()
        {
            base.OnEnable();
            inst = (Texture2DArray)target;
            formatProp = serializedObject.FindProperty("m_Format");
            wrapUProp = serializedObject.FindProperty("m_TextureSettings.m_WrapU");
            wrapVProp = serializedObject.FindProperty("m_TextureSettings.m_WrapV");
            wrapWProp = serializedObject.FindProperty("m_TextureSettings.m_WrapW");
            colorSpaceProp = serializedObject.FindProperty("m_ColorSpace");

            SetupSlicesFromFolder(inst,slices);
            slicesRList = EditorGUITools.SetupSlicesRList(slices);

            lastFormatId = formatProp.intValue;
        }
        public override void OnDisable()
        {
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
                ChangeTextureFormat(inst, formatId);
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
                    DumpSlicesToDisk(inst,slices);
                }

                if (GUILayout.Button(GUI_COMBINE))
                {
                    CombineSlices(inst,slices);
                }
            });

        }

        public static void CombineSlices(Texture inst,List<Texture2D> slices)
        {
            var isTexArr = inst.dimension == TextureDimension.Tex2DArray;


            var sourceTexs = slices.Where(t => t).ToArray();
            if (sourceTexs.Length == 0)
                return;

            const int MIN_SIZE = 32;
            var w = Mathf.Max(MIN_SIZE, sourceTexs[0].width);
            var h = Mathf.Max(MIN_SIZE, sourceTexs[0].height);
            var count = sourceTexs.Length;

            var resultRT = RenderTextureTools.GetTemporaryUAV(w, h, RenderTextureFormat.Default);

            Texture texArrOr3D = isTexArr ?
                new Texture2DArray(w, h, count, TextureFormat.ASTC_6x6, true, true) :
                new Texture3D(w, h, count, TextureFormat.ASTC_6x6, true);
            

            var texList = new Texture2D[count];
            for (int i = 0; i < count; i++) {
                var sourceTex = sourceTexs[i];

                ComputeShaderEx.DispatchKernel_CopyTexture(sourceTex, resultRT);

                var tex = texList[i] = new Texture2D(w, h,TextureFormat.RGBA32,true,true);
                resultRT.ReadRenderTexture(ref tex, true);

                // fill texArrOr3D
                tex.Compress(true,TextureFormat.ASTC_6x6);
                Graphics.CopyTexture(tex, 0, texArrOr3D, i);
            }
            
            resultRT.ReleaseSafe();

            Selection.activeObject = AssetDatabaseTools.SaveNewAsset(inst, texArrOr3D);
        }


        public static string GetTextureSlicesFolder(Texture inst)
        {
            var path = AssetDatabase.GetAssetPath(inst);
            var folderPath = $"{Path.GetDirectoryName(path)}/{inst.name}_Slices";
            return folderPath;
        }

        public static void DumpSlicesToDisk(Texture inst,List<Texture2D> slices)
        {
            slices.Clear();

            var textureDepth = inst.GetDepth();

            var folderPath = GetTextureSlicesFolder(inst);
            PathTools.CreateAbsFolderPath(folderPath);
            
            var resultRT = RenderTextureTools.GetTemporaryUAV(inst.width, inst.height, RenderTextureFormat.Default);
            var tex = new Texture2D(inst.width, inst.height, TextureFormat.ARGB32, true,false);
            for (int i = 0; i < textureDepth; i++)
            {
                ComputeShaderEx.DispatchKernel_CopyTexture(inst, resultRT, i, 0);
                resultRT.ReadRenderTexture(ref tex);

                var assetPath = $"{folderPath}/{i}.png";
                var filePath = PathTools.GetAssetAbsPath(assetPath);
                var bytes = tex.GetEncodeBytes(TextureEncodeType.PNG);
                File.WriteAllBytes(filePath, bytes);

                AssetDatabase.Refresh();
                slices.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath));
            }
            resultRT.ReleaseSafe();
        }

        public static void SetupSlicesFromFolder(Texture inst,List<Texture2D> slices)
        {
            var folderPath = GetTextureSlicesFolder(inst);
            if(!AssetDatabase.IsValidFolder(folderPath))
                return;

            var texs = AssetDatabaseTools.LoadAssets<Texture2D>(folderPath);
            foreach (var tex in texs) {
                slices.Add(tex);
            }
        }


        public static List<Texture2D> GetSlices(Texture inst,TextureFormat sliceFormat,float gammaValue = 1)
        {
            var textureDepth = inst.GetDepth();

            var list = new List<Texture2D>();
            var resultRT = RenderTextureTools.GetTemporaryUAV(inst.width, inst.height, RenderTextureFormat.Default);
            var isCompressFormat = GraphicsFormatUtility.IsCompressedFormat(sliceFormat);

            for (int i = 0; i < textureDepth; i++)
            {
                var tex = new Texture2D(inst.width, inst.height, TextureFormat.RGBA32, true, true);

                tex.name = i.ToString();
                ComputeShaderEx.DispatchKernel_CopyTexture(inst, resultRT, i, 0, gammaValue);
                resultRT.ReadRenderTexture(ref tex);
                list.Add(tex);

                if (isCompressFormat)
                {
                    tex.Compress(true, sliceFormat);
                }
                tex.Apply();

            }
            resultRT.Release();

            return list;
        }
        /// <summary>
        /// Change Format of Texture2DArray or Texture3D
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="texDepth"></param>
        /// <param name="newFormatId"></param>
        public static void ChangeTextureFormat(Texture inst, int newFormatId)
        {
            var isTexArr = inst.dimension == TextureDimension.Tex2DArray;
            if (inst.dimension != TextureDimension.Tex3D && !isTexArr)
                return;

            var gf = (GraphicsFormat)newFormatId;
            var tf = GraphicsFormatUtility.GetTextureFormat(gf);

            if (tf == 0)
            {
                Debug.Log("error format");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(inst);
            if (!EditorTools.DisplayDialog_Ok_Cancel($"Override {inst}?"))
            {
                assetPath = $"{Path.GetDirectoryName(assetPath)}/{inst}_.asset";
            }

            var list = GetSlices(inst, tf);

            if (isTexArr)
                EditorTextureTools.Create2DArray(list, assetPath, true);
            else
                EditorTextureTools.Create3D(list, assetPath, true);
        }


    }
}
#endif