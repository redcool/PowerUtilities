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
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// BaseInspector for call default UnityEditor method 
    /// </summary>
    public abstract class BaseEditorEx : Editor
    {
        public Editor defaultEditor;
        public Type defaultEditorType;

        public Dictionary<string, MethodInfo> methodInfoDict = new Dictionary<string, MethodInfo>();
        public string[] methodNames = new[]
        {
            "OnEnable",
            "OnDisable",
            "OnInspectorGUI",
            "DrawPreview",
            "OnPreviewSettings",

            "OnSceneGUI",
            "RenderStaticPreview",
            "OnInteractivePreviewGUI",
            "OnPreviewGUI",
            "OnDestroy",

            "OnGetFrameBounds",
            "OnValidate",
            "HasPreviewGUI",
            "CreateInspectorGUI",
        };
        /// <summary>
        /// Set true will use CreateInspectorGUI ,false use OnInspectorGUI
        /// new UnityEditor use UIToolkit,like PresetEditor
        /// </summary>
        public virtual bool IsUIToolkitGUI => false;

        /// <summary>
        /// className : like "UnityEditor.Texture2DArrayInspector"
        /// </summary>
        public abstract string GetDefaultInspectorTypeName();
        public Object scriptObj;

        public virtual void OnEnable()
        {
            // get texture2d array inspector
            EditorTools.GetOrCreateUnityEditor(ref defaultEditor, targets, ref defaultEditorType, GetDefaultInspectorTypeName());

            // save unity editor methodinfo
            methodNames.ForEach(name =>
            {
                methodInfoDict[name] = defaultEditorType.GetMethod(name, ReflectionTools.callBindings);
            });

            defaultEditor.InvokeDelegate<Action>(ref methodInfoDict, nameof(OnEnable));

            scriptObj = AssetDatabaseTools.FindAssetPathAndLoad<Object>(out _, GetType().Name);
        }
        public virtual void OnDisable()
        {
            defaultEditor.InvokeDelegate<Action>(ref methodInfoDict, nameof(OnDisable));
        }
        public virtual void OnDestroy()
        {
            defaultEditor.InvokeDelegate<Action>(ref methodInfoDict, nameof(OnDestroy));
        }
        public virtual void OnSceneGUI()
        {
            defaultEditor.InvokeDelegate<Action>(ref methodInfoDict, nameof(OnSceneGUI));
        }
        public override void OnInspectorGUI()
        {
            defaultEditor.InvokeDelegate<Action>(ref methodInfoDict, nameof(OnInspectorGUI));
            EditorGUITools.DrawColorLine(1);

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField(GUIContentEx.TempContent("Script : ", "Script draw gui "),scriptObj, typeof(Object), false);
            }
        }

        public override bool HasPreviewGUI()
        {
            return (bool)defaultEditor.InvokeDelegate<Func<bool>>(ref methodInfoDict, nameof(HasPreviewGUI));
            //return target;
        }

        public override void DrawPreview(Rect previewArea)
        {
            defaultEditor.InvokeDelegate<Action<Rect>>(ref methodInfoDict, nameof(DrawPreview), previewArea);
        }

        public virtual Bounds OnGetFrameBounds()
        {
            return (Bounds)defaultEditor.InvokeDelegate<Func<Bounds>>(ref methodInfoDict, nameof(OnGetFrameBounds));
        }
        public virtual void OnValidate()
        {
            defaultEditor.InvokeDelegate<Action>(ref methodInfoDict, nameof(OnValidate));
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            defaultEditor.InvokeDelegate<Action<Rect, GUIStyle>>(ref methodInfoDict, nameof(OnPreviewGUI), r, background);
        }
        public override void OnPreviewSettings()
        {
            defaultEditor.InvokeDelegate<Action>(ref methodInfoDict, nameof(OnPreviewSettings));
        }
        
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            return (Texture2D)defaultEditor.InvokeDelegate<Func<string, Object[], int, int, Texture2D>>(ref methodInfoDict, nameof(RenderStaticPreview), assetPath, subAssets, width, height);
        }
        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            defaultEditor.InvokeDelegate<Action<Rect, GUIStyle>>(ref methodInfoDict, nameof(OnInteractivePreviewGUI), r, background);
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (!IsUIToolkitGUI)
                return default;

            var rootUI = (VisualElement)defaultEditor.InvokeDelegate<Func<VisualElement>>(ref methodInfoDict, nameof(CreateInspectorGUI));
            //rootUI.style.marginLeft = -30;

            var rootContainer = new VisualElement();
            rootContainer.style.flexGrow = 1;
            rootContainer.Add(rootUI);

            var line = new VisualElement();
            line.style.backgroundColor = ColorTools.Parse("#749C75");
            line.style.height = 1;
            line.style.flexGrow = 0;
            rootContainer.Add(line);

            var scriptField = new ObjectField("Script");
            scriptField.tooltip = "Script draw gui";
            scriptField.SetEnabled(false);
            scriptField.value = scriptObj;
            rootContainer.Add(scriptField);

            return rootContainer;
        }
    }
}
#endif