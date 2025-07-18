#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    /// <summary>
    /// Material's Group TexturePropertyWithHDRColor Attribute
    /// </summary>
    public class GroupTextureColorDrawer : BaseGroupItemDrawer
    {
        public string colorPropName;

        public GroupTextureColorDrawer() : base("",null) {}
        public GroupTextureColorDrawer(string groupName) : base(groupName, null) { }
        public GroupTextureColorDrawer(string groupName,string tooltip) : base(groupName,tooltip) { }

        public GroupTextureColorDrawer(string groupName, string tooltip,string colorPropName) : base(groupName, tooltip) {
            this.colorPropName = colorPropName;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (IsGroupOn())
                return EditorGUIUtility.singleLineHeight;
            return MIN_LINE_HEIGHT;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            //editor.TexturePropertyMiniThumbnail(position, prop, label.text, label.tooltip);
            
            var colorProp = editor.GetProperty(colorPropName);
            if (colorProp != null)
                editor.TexturePropertyWithHDRColor(label, prop, colorProp, true);
            else
                editor.TexturePropertySingleLine(label, prop);
        }

    }

}
#endif