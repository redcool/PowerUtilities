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
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var rowCount = 1;
            var attr = attribute as ListItemDrawAttribute;
            if(attr.isShowTitleRow && attr.isTitleFold)
                rowCount++;

            return base.GetPropertyHeight(property, label) * rowCount;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var attr = attribute as ListItemDrawAttribute;
            var pos = position;
            pos.height = EditorGUIUtility.singleLineHeight;

            //1 show label
            if (attr.isShowTitleRow)
            {
                attr.isTitleFold = EditorGUI.Foldout(pos, attr.isTitleFold, label,true);
                pos.y += pos.height;

                if (!attr.isTitleFold)
                    return;
            }

            //2 show contents
            for (int i = 0; i < attr.propNames.Length; i++)
            {
                var propName = attr.propNames[i];
                var propWidth = attr.propWidths[i];

                // transform to percent rate,
                UpdatePropWidth(ref position, ref pos, ref propWidth);

                var prop = property.FindPropertyRelative(propName);

                // update pos
                // first one
                var xOffset = i == 0 ? 0 : pos.width;
                SetPos(ref pos, xOffset, propWidth);

                if (prop != null)
                {
                    EditorGUI.PropertyField(pos, prop, GUIContent.none);
                }
                else if (prop == null)
                {
                    EditorGUI.LabelField(pos, GUIContentEx.TempContent(propName));
                }
                //EditorGUI.LabelField(pos, GUIContentEx.TempContent(propName));
            }

            EditorGUI.EndProperty();
        }
        void SetPos(ref Rect pos, float offsetX, float width)
        {
            pos.x += offsetX;
            pos.width = width;
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
    /// demo1
    ///     [ListItemDraw("tag,q:,renderQueue,kw:,keywords", "120,10,50,20,")]
    ///     public List<TagInfo> tagInfoList = new List<TagInfo>();
    ///     
    ///     tag : 1 TagInfo.propertyName, q: only label, renderQueue : 2 TagInfo.propertyName, kw: only label, keywords : 3 TagInfo.propertyName
    ///     
    ///     propNames, that canot be found in object, will show as Label
    ///     
    /// demo2 
    ///         [ListItemDraw("x:,x,y:,y,z:,z,w:,w", "15,80,15,80,15,80,15,80",isShowTitleRow =true)]
    ///         Vector4 spriteUVST;
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

        public bool isShowTitleRow;
        public bool isTitleFold;
        public ListItemDrawAttribute(string propNamesStr,string widthStr,char splitChar=',')
        {
            propNames = propNamesStr.SplitBy(splitChar);
            propWidths = widthStr.SplitBy(splitChar)
                .Select(str=> string.IsNullOrEmpty(str) ? 0:Convert.ToSingle(str))
                .ToArray();
        }
    }
}
