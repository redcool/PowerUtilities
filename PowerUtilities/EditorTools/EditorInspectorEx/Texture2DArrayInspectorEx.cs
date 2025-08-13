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

            SetupSlicesFromFolder();

            SetupSlicesRList();
            lastFormatId = formatProp.intValue;
        }
        public override void OnDisable()
        {
            slices.Clear();
        }

        private void SetupSlicesRList()
        {
            slicesRList = new ReorderableList(slices, typeof(Texture2D));
            slicesRList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Slices");
            };
            slicesRList.drawNoneElementCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "none");
            };
            slicesRList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var tex = (Texture2D)slicesRList.list[index];
                var texName = tex ? tex.name : "none";

                var pos = new Rect(rect.x, rect.y, rect.width, 18);
                slicesRList.list[index] = EditorGUI.ObjectField(pos, (Object)slicesRList.list[index], typeof(Texture2D), false);
            };
            slicesRList.onAddCallback = (ReorderableList rlist) =>
            {
                rlist.list.Add(null);
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // draw 
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();
            EditorGUITools.EnumPropertyFieldSearchable(formatProp, typeof(GraphicsFormat), (SerializedProperty prop, int formatId) =>
            {
                ChangeFormat();
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
                    DumpSlicesToDisk();
                }

                if (GUILayout.Button(GUI_COMBINE))
                {
                    CombineSlices();
                }
            });

        }

        public void CombineSlices()
        {
            var sourceTexs = slices.Where(t => t).ToArray();
            if (sourceTexs.Length <= 0)
                return;

            const int MIN_SIZE = 32;
            var w = Mathf.Max(MIN_SIZE, sourceTexs[0].width);
            var h = Mathf.Max(MIN_SIZE, sourceTexs[0].height);
            var count = sourceTexs.Length;

            var desc = new RenderTextureDescriptor(w, h, RenderTextureFormat.Default);
            desc.enableRandomWrite = true;
            var resultRT = RenderTexture.GetTemporary(desc);

            var texArr = new Texture2DArray(w, h, count, TextureFormat.ASTC_6x6, true);
            var texList = new Texture2D[count];
            for (int i = 0; i < count; i++) {
                var sourceTex = sourceTexs[i];

                ComputeShaderEx.DispatchKernel_CopyTexture(sourceTex, resultRT);

                var tex = texList[i] = new Texture2D(w, h,TextureFormat.RGBA32,true,true);
                resultRT.ReadRenderTexture(ref tex, true);

                // fill texArr
                tex.Compress(true,TextureFormat.ASTC_6x6);
                Graphics.CopyTexture(tex, 0, texArr, i);
            }
            
            RenderTexture.ReleaseTemporary(resultRT);

            var path = AssetDatabase.GetAssetPath(inst);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(texArr,path);

            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2DArray));
        }
        public string GetSlicesFolderAssetPath()
        {
            var path = AssetDatabase.GetAssetPath(inst);
            var folderPath = $"{Path.GetDirectoryName(path)}/{inst.name}_Slices";
            return folderPath;
        }

        public void DumpSlicesToDisk()
        {
            var folderPath = GetSlicesFolderAssetPath();
            PathTools.CreateAbsFolderPath(folderPath);
            
            var tex = new Texture2D(inst.width, inst.height, TextureFormat.ARGB32, true);
            var resultRT = RenderTextureTools.GetTemporaryUAV(inst.width, inst.height, RenderTextureFormat.Default);
            for (int i = 0; i < inst.depth; i++)
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
            resultRT.Release();
        }


        public void SetupSlicesFromFolder()
        {
            var folderPath = GetSlicesFolderAssetPath();
            if(!AssetDatabase.IsValidFolder(folderPath))
                return;

            var texs = AssetDatabaseTools.LoadAssets<Texture2D>(folderPath);
            foreach (var tex in texs) {
                slices.Add(tex);
            }
        }


        public static List<Texture2D> GetSlices(Texture2DArray inst)
        {
            var list = new List<Texture2D>();
            var resultRT = RenderTextureTools.GetTemporaryUAV(inst.width, inst.height, RenderTextureFormat.Default);
            for (int i = 0; i < inst.depth; i++)
            {
                var tex = new Texture2D(inst.width, inst.height, TextureFormat.ARGB32, true);
                tex.name = i.ToString();
                ComputeShaderEx.DispatchKernel_CopyTexture(inst, resultRT, i, 0);
                resultRT.ReadRenderTexture(ref tex);
                list.Add(tex);
            }
            resultRT.Release();

            return list;
        }

        public void ChangeFormat()
        {
            var gf = (GraphicsFormat)formatProp.intValue;
            Debug.Log(gf);

            return;
            var list = GetSlices(inst);

            foreach (var tex in list)
            {
            }
        }
    }
}
#endif