#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace PowerUtilities
{
    [CustomPropertyDrawer(typeof(EditorBorderAttribute))]
    public class EditorBorderAttributeDecorator : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var attr = attribute as EditorBorderAttribute;

            var pos = EditorGUI.IndentedRect(position);
            if (!attr.isIndentRect)
                pos.x = position.x;

            pos.x += attr.posXOffset;
            //pos.y += EditorGUIUtility.singleLineHeight/2;
            pos.width += 4;

            pos.y += attr.posYOffset;
            pos.height = EditorGUIUtility.singleLineHeight * attr.lineRowCount;

            if (!MaterialGroupTools.IsGroupOn(attr.groupName))
                pos.height = EditorGUIUtility.singleLineHeight;

            EditorGUITools.DrawBoxColors(pos, attr.borderSize, default, attr.leftColor, attr.topColor, attr.rightColor, attr.bottomColor);
        }
        public override float GetHeight()
        {
            var attr = attribute as EditorBorderAttribute;
            return attr.lineHeight;
        }
    }
}
#endif