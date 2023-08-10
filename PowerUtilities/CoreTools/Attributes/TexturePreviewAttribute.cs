namespace PowerUtilities
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(TexturePreviewAttribute))]
    public class TexturePreviewAttributeEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = attribute as TexturePreviewAttribute;
            return attr.textureSize;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null && ! property.objectReferenceValue.GetType().IsSubclassOf( typeof(Texture)))
                return;
            
            var attr = attribute as TexturePreviewAttribute;

            // show property
            var propPos = position;
            propPos.width = position.width - attr.textureSize-2;
            propPos.height = 18;
            //EditorGUI.PropertyField(propPos, property, label);
            EditorGUI.LabelField(propPos, label);

            // show preview static
            var texPos = new Rect();
            texPos.position = new Vector2(position.xMax - attr.textureSize, position.y);
            texPos.size = new Vector2(attr.textureSize, attr.textureSize);
            property.objectReferenceValue= EditorGUI.ObjectField(texPos, property.objectReferenceValue, typeof(Texture), false);
            //EditorGUI.DrawPreviewTexture(texPos, property.objectReferenceValue as Texture);

            // show preview float
            if (attr.showFloatPreview && position.Contains(Event.current.mousePosition))
            {
                var startPos = Event.current.mousePosition;
                var texturePos = position;
                texturePos.position = startPos;
                texturePos.size = new Vector2(attr.textureSize,attr.textureSize);
                
                EditorGUI.DrawPreviewTexture(texturePos, property.objectReferenceValue as Texture,null,ScaleMode.StretchToFill);
            }
        }
        
    }

#endif
    /// <summary>
    /// Show big preview texture in inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TexturePreviewAttribute : PropertyAttribute
    {
        public int textureSize = 64;
        public bool showFloatPreview;

        public TexturePreviewAttribute(int textureSize=64)
        {
            this.textureSize = Mathf.Max(18,textureSize);
        }
    }
}
