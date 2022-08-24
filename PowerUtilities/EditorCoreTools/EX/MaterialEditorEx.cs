#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
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


        #endregion

        /// <summary>
        /// this version handle property [Vector,Texture] tooltips
        /// 
        /// applyMaterialPropertyDraw set true, MaterialPropertyDraw call will dead loop,unity crash,
        /// 
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="prop"></param>
        /// <param name="label"></param>
        /// <param name="label"></param>
        /// <param name="applyMaterialPropertyDraw">true, MaterialPropertyDraw call will dead loop,unity crash</param>
        public static void ShaderProperty(this MaterialEditor editor, MaterialProperty prop, GUIContent label, int indent = 0,bool applyMaterialPropertyDraw=true)
        {
            EditorGUI.indentLevel+=indent;
            switch (prop.type)
            {
                case MaterialProperty.PropType.Vector:
                    VectorProperty(editor, prop, label);
                    break;
                case MaterialProperty.PropType.Texture:
                    TextureProperty(editor, prop, label);
                    break;
                default:
                    {
                        if (applyMaterialPropertyDraw)
                            editor.ShaderProperty(prop, label);
                        else
                            DefaultShaderPropertyInternal(editor, prop, label);
                    }
                    break;
            }
            EditorGUI.indentLevel-=indent;
        }

        public static void ShaderProperty(this MaterialEditor editor,Rect position, MaterialProperty prop, GUIContent label, int indent = 0, bool applyMaterialPropertyDraw = true)
        {
            EditorGUI.indentLevel+=indent;
            switch (prop.type)
            {
                case MaterialProperty.PropType.Vector:
                    VectorProperty(editor, position, prop, label);
                    break;
                case MaterialProperty.PropType.Texture:
                    TextureProperty(editor, position, prop, label);
                    break;
                default:
                    {
                        if (applyMaterialPropertyDraw)
                            editor.ShaderProperty(position,prop, label);
                        else
                            DefaultShaderPropertyInternal(editor, position, prop, label);
                    }
                    break;
            }
            EditorGUI.indentLevel-=indent;
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