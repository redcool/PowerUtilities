#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Handle MaterialEditor draw [Vector,Texture] cannot have tooltips.
    /// call MaterialEditorEx.ShaderProperty in custom ShaderGUI
    /// 
    /// call VectorProperty ,TextureProperty,DefaultShaderPropertyInternal in MaterialPropertyDrawer
    /// </summary>
    public static class MaterialEditorEx
    {

        #region private methods in MaterialEditor
        /// <summary>
        /// GetPropertyRect
        /// </summary>
        static Lazy<MethodInfo> lazyGetPropertyRect = new Lazy<MethodInfo>(() =>
            typeof(MaterialEditor).GetMethod("GetPropertyRect", BindingFlags.Instance| BindingFlags.NonPublic, null,
                new[] { typeof(MaterialProperty), typeof(string), typeof(bool) }, null)
        );
        public static Rect GetPropertyRect(this MaterialEditor editor, MaterialProperty prop, GUIContent label, bool ignoreDrawer) 
            => (Rect)lazyGetPropertyRect.Value.Invoke(editor, new object[] { prop, label.text, ignoreDrawer });


        /// <summary>
        /// DefaultShaderPropertyInternal
        /// </summary>
        static Lazy<MethodInfo> lazyDefaultShaderPropertyInternal = new Lazy<MethodInfo>(() =>
            typeof(MaterialEditor).GetMethod("DefaultShaderPropertyInternal", BindingFlags.Instance|BindingFlags.NonPublic, null,
                new[] { typeof(MaterialProperty), typeof(GUIContent) }, null)
            );
        static Lazy<MethodInfo> lazyDefaultShaderPropertyInternal3 = new Lazy<MethodInfo>(() =>
            typeof(MaterialEditor).GetMethod("DefaultShaderPropertyInternal", BindingFlags.Instance|BindingFlags.NonPublic, null,
                new[] {typeof(Rect), typeof(MaterialProperty), typeof(GUIContent) }, null)
            );
        public static void DefaultShaderPropertyInternal(this MaterialEditor editor, MaterialProperty prop, GUIContent label)
            => lazyDefaultShaderPropertyInternal.Value.Invoke(editor, new object[] { prop, label });
        public static void DefaultShaderPropertyInternal(this MaterialEditor editor,Rect position, MaterialProperty prop, GUIContent label)
            => lazyDefaultShaderPropertyInternal3.Value.Invoke(editor, new object[] { position,prop, label });

        static Lazy<Assembly> lazyCoreModule = new Lazy<Assembly>(() =>
            AppDomain.CurrentDomain.GetAssemblies().Where(item => item.FullName.Contains("UnityEditor.CoreModule")).FirstOrDefault()
        );

        #endregion


        public static bool DrawMaterialAttribute(this MaterialEditor editor, ref Rect position, MaterialProperty prop, GUIContent label)
        {
            /** ( draw material attribute ) original version
            MaterialPropertyHandler handler = MaterialPropertyHandler.GetHandler(((Material)base.target).shader, prop.name);
            if (handler != null)
            {
                handler.OnGUI(ref position, prop, (label.text != null) ? label : new GUIContent(prop.displayName), this);
                if (handler.propertyDrawer != null)
                {
                    return;
                }
            }
            */
            
            var coreModule = lazyCoreModule.Value;
            if (coreModule == null)
                return false;

            var handlerType = coreModule.GetType("UnityEditor.MaterialPropertyHandler");
            // get handler
            var GetHandleFunc = handlerType.GetMethod("GetHandler", BindingFlags.Static| BindingFlags.NonPublic, null, new[] { typeof(Shader), typeof(string) }, null);
            var handlerInst = GetHandleFunc.Invoke(null, new object[] { ((Material)editor.target).shader, prop.name });
            if (handlerInst == null)
                return false;
            // call OnGUI
            var onGUIFunc = handlerType.GetMethod("OnGUI");
            var paramObjs = new object[] { position, prop, (label.text != null) ? label : new GUIContent(prop.displayName), editor };
            onGUIFunc.Invoke(handlerInst, paramObjs);

            //get result
            position = (Rect)paramObjs[0];

            // check propertyDrawer
            var propertyDrawerGetter = handlerType.GetProperty("propertyDrawer");
            return propertyDrawerGetter.GetValue(handlerInst) != null;
        }


        public static void ShaderProperty(this MaterialEditor editor, MaterialProperty prop, GUIContent label, int indent = 0)
        {
            var position = GetPropertyRect(editor, prop, label, false);

            ShaderProperty(editor, position, prop, label, indent, true);
        }

        /// <summary>
        /// <para>this version handle property [Vector,Texture] tooltips</para>
        /// <para>MaterialPropertyDraw set applyMaterialPropertyDraw to false, otherwise will dead loop,unity crash </para>
        /// 
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="prop"></param>
        /// <param name="label"></param>
        /// <param name="indent"></param>
        /// <param name="applyMaterialPropertyDraw">true, MaterialPropertyDraw call will dead loop,unity crash</param>
        public static void ShaderProperty(this MaterialEditor editor,Rect position, MaterialProperty prop, GUIContent label, int indent = 0, bool applyMaterialPropertyDraw = true)
        {
            EditorGUI.indentLevel+=indent;
            DrawShaderProperty();
            EditorGUI.indentLevel-=indent;

            void DrawShaderProperty()
            {

                if (applyMaterialPropertyDraw)
                {
                    var needReturn = DrawMaterialAttribute(editor, ref position, prop, label);
                    if (needReturn)
                        return;
                }

                switch (prop.type)
                {
                    case MaterialProperty.PropType.Vector:
                        VectorProperty(editor, position, prop, label);
                        break;

                    case MaterialProperty.PropType.Texture:
                        TextureProperty(editor, position, prop, label);
                        break;

                    default:
                        DefaultShaderPropertyInternal(editor, position, prop, label);
                        break;
                }
            }
        }

        public static Vector4 VectorProperty(this MaterialEditor editor, MaterialProperty prop, GUIContent label)
        {
            Rect propertyRect = GetPropertyRect(editor, prop, label, true);
            return VectorProperty(editor, propertyRect, prop, label);
        }

        public static Vector4 VectorProperty(this MaterialEditor editor, Rect position, MaterialProperty prop, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;
            Vector4 vectorValue = EditorGUI.Vector4Field(position, label, prop.vectorValue);
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = vectorValue;
            }

            return prop.vectorValue;
        }

        public static Texture TextureProperty(this MaterialEditor editor, MaterialProperty prop, GUIContent label)
        {
            var position = GetPropertyRect(editor, prop, label, true);
            bool scaleOffset = (prop.flags & MaterialProperty.PropFlags.NoScaleOffset) == 0;
            return TextureProperty(editor, position, prop, label);
        }
        public static Texture TextureProperty(this MaterialEditor editor,Rect position, MaterialProperty prop, GUIContent label)
        {
            bool scaleOffset = (prop.flags & MaterialProperty.PropFlags.NoScaleOffset) == 0;
            return editor.TextureProperty(position, prop, label.text, label.tooltip, scaleOffset);
        }
    }
}
#endif