using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PowerUtilities
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ListItemDrawAttribute))]
    public class ListItemAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var inst = attribute as ListItemDrawAttribute;
            var pos = position;

            for (int i = 0; i < inst.propNames.Length; i++)
            {
                var propName = inst.propNames[i];
                var propWidth = inst.propWidths[i];

                // transform to percent rate,
                UpdatePropWidth(ref position, ref pos, ref propWidth);

                var prop = property.FindPropertyRelative(propName);

                // update pos
                // first one
                var xOffset = i == 0 ? 0 : pos.width;
                SetPos(ref pos, xOffset, propWidth);

                if (prop != null)
                    EditorGUI.PropertyField(pos, prop, GUIContent.none);
                else if (prop == null)
                    EditorGUI.LabelField(pos, GUIContentEx.TempContent(propName));
            }

            EditorGUI.EndProperty();
        }
        void SetPos(ref Rect pos, float offsetX, float w)
        {
            pos.x += offsetX;
            pos.width = w;
        }
        private static void UpdatePropWidth(ref Rect position, ref Rect pos, ref float propWidth)
        {
            if (propWidth <= 0)
                propWidth = position.width - pos.x;
            else if (propWidth <= 1)
                propWidth *= position.width;
        }
    }
#endif

    /// <summary>
    /// properties show in editor list
    /// 
    ///     [ListItemDraw("tag,q:,renderQueue,kw:,keywords", "120,10,50,20,")]
    ///     public List<TagInfo> tagInfoList = new List<TagInfo>();
    /// </summary>
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ListItemDrawAttribute : PropertyAttribute
    {
        public string[] propNames;

        /// <summary>
        /// width = [0,1]  : percent
        /// width = 0 : use last empty space
        /// </summary>
        public float[] propWidths;
        public ListItemDrawAttribute(string propNamesStr,string widthStr,char splitChar=',')
        {
            propNames = propNamesStr.SplitBy(splitChar);
            propWidths = widthStr.SplitBy(splitChar)
                .Select(str=> string.IsNullOrEmpty(str) ? 0:Convert.ToSingle(str))
                .ToArray();
        }
    }
}
