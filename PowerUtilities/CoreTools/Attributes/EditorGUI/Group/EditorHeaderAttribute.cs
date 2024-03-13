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

    [CustomPropertyDrawer(typeof(EditorHeaderAttribute))]
    public class EditorHeaderAttributeEditor : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var attr = attribute as EditorHeaderAttribute;
            var isGroupOn = MaterialGroupTools.IsGroupOn(attr.groupName);

            if (isGroupOn)
                return base.GetHeight() + 5;
            return -20;
        }
        public override void OnGUI(Rect position)
        {
            var attr = attribute as EditorHeaderAttribute;
            var isGroupOn = MaterialGroupTools.IsGroupOn(attr.groupName);

            if (!isGroupOn)
                return;

            var pos = position;
            pos.x += attr.posOffset;
            pos.y += 5;

            var style = EditorStyles.boldLabel;
            //style.fontSize = 16;
            style.alignment = TextAnchor.UpperLeft;

            var lastColor = GUI.contentColor;
            GUI.color = attr.color;
            
            EditorGUI.LabelField(pos,GUIContentEx.TempContent(attr.header), style);
            GUI.color = lastColor;
            
        }
    }

#endif
    public class EditorHeaderAttribute : PropertyAttribute
    {
        public string groupName;
        public string header;
        public Color color = new Color(0.1f, 0.82f, 0.1f);
        public int posOffset = 0;
        public EditorHeaderAttribute(string groupName, string header, string colorStr = null)
        {
            this.groupName = groupName;
            this.header = header;

            if (!string.IsNullOrEmpty(colorStr))
                ColorUtility.TryParseHtmlString(colorStr, out color);
        }
    }
}
