#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// For UnityEditor method 
    /// </summary>
    public abstract class BaseEditorEx : Editor
    {
        public Editor defaultEditor;
        public Type defaultEditorType;

        //MethodInfo onEnable, onDisable, onInspector, onSceneGUI, onDestroy, onGetFrameBounds, onValidate,
        //    drawPreview,
        //    onPreviewGUI, onPreviewSettings, renderStaticPreview, onInteractivePreviewGUI;

        
        public Dictionary<string,MethodInfo> methodInfoDict = new Dictionary<string,MethodInfo>();
        public string[] methodNames = new[]
        {
            "OnEnable",
            "OnDisable",
            "OnInspectorGUI",
            "DrawPreview",
            "OnPreviewSettings",

            //"OnSceneGUI",
            "RenderStaticPreview",
            "OnInteractivePreviewGUI",
            "OnPreviewGUI",
            "OnDestroy",

            "OnGetFrameBounds",
            "OnValidate",
        };

        /// <summary>
        /// className : like "UnityEditor.Texture2DArrayInspector"
        /// </summary>
        public abstract string GetDefaultInspectorTypeName();

        public virtual void OnEnable()
        {
            // get texture2d array inspector
            EditorTools.GetOrCreateUnityEditor(ref defaultEditor, targets, ref defaultEditorType, GetDefaultInspectorTypeName());

            methodNames.ForEach(name =>
            {
                MethodInfo methodInfo = null;
                defaultEditorType.GetMethod(ref methodInfo, name);

                methodInfoDict[name] = methodInfo;
            });


            defaultEditor.InvokeDelegate<Action>(methodInfoDict[nameof(OnEnable)]);
        }
        public virtual void OnDisable()
        {
            defaultEditor.InvokeDelegate<Action>(methodInfoDict[nameof(OnDisable)]);
        }
        //public virtual void OnSceneGUI()
        //{
        //    //defaultEditor.InvokeDelegate<Action>(methodInfoDict[nameof(OnSceneGUI)]);
        //}

        private void OnDestroy()
        {
            defaultEditor.InvokeDelegate<Action>(methodInfoDict[nameof(OnDestroy)]);
        }
        public virtual Bounds OnGetFrameBounds()
        {
            return defaultEditor.InvokeDelegate<Func<Bounds>,Bounds>(methodInfoDict[nameof(OnGetFrameBounds)]);
        }
        public virtual void OnValidate()
        {
            defaultEditor.InvokeDelegate<Action>(methodInfoDict[nameof(OnValidate)]);
        }

        public override void OnInspectorGUI()
        {
            defaultEditor.InvokeDelegate<Action>(methodInfoDict[nameof(OnInspectorGUI)]);

            EditorGUITools.DrawColorLine(1);
        }

        public override bool HasPreviewGUI()
        {
            return target;
        }

        public override void DrawPreview(Rect previewArea)
        {
            defaultEditor.InvokeDelegate<Action<Rect>>(methodInfoDict[nameof(DrawPreview)], previewArea);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            defaultEditor.InvokeDelegate<Action<Rect, GUIStyle>>(methodInfoDict[nameof(OnPreviewGUI)], r, background);
        }
        public override void OnPreviewSettings()
        {
            defaultEditor.InvokeDelegate<Action>(methodInfoDict[nameof(OnPreviewSettings)]);
        }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            return (Texture2D)defaultEditor.InvokeDelegate<Func<string, Object[], int, int, Texture2D>>(methodInfoDict[nameof(RenderStaticPreview)], assetPath, subAssets, width, height);
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            defaultEditor.InvokeDelegate<Action<Rect, GUIStyle>>(methodInfoDict[nameof(OnInteractivePreviewGUI)], r, background);
        }

    }
}
#endif