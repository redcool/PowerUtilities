namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Rendering;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(RenderingLayerMaskAttribute))]
    public class RenderingLayerMaskDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Draw(position, property, label);
        }

        public static void Draw(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            var mask = property.intValue;
            var isUint = property.type =="uint";
            if(isUint && mask == int.MaxValue)
            {
                mask =-1;
            }
            
            mask = EditorGUI.MaskField(position,label, mask, GraphicsSettings.currentRenderPipeline.renderingLayerMaskNames);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = isUint && mask ==-1 ? int.MaxValue : mask;
            }
            EditorGUI.showMixedValue = false;
        }

        public static void Draw(SerializedProperty prop,GUIContent label)
        {
            Draw(EditorGUILayout.GetControlRect(),prop, label);
        }
    }
#endif

    public class RenderingLayerMaskAttribute : PropertyAttribute
    {
    }
}
