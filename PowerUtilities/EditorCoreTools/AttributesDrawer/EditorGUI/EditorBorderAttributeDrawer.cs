﻿#if UNITY_EDITOR
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
            pos.x -= 2;
            //pos.y += EditorGUIUtility.singleLineHeight/2;
            pos.width += 4;
            pos.height = EditorGUIUtility.singleLineHeight * attr.lineCount;
            EditorGUITools.DrawBoxColors(pos, attr.lineHeight, default, attr.color, attr.top, attr.right, attr.bottom);
        }
        public override float GetHeight()
        {
            return 0;
        }
    }
}
#endif