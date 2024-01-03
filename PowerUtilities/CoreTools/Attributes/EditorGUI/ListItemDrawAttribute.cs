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
            //default is 1 row
            var rowCount = 1;

            var attr = attribute as ListItemDrawAttribute;
            // show title and title is unfold
            if (attr.isShowTitleRow && attr.isTitleUnfold)
                rowCount += GetContentRowCount(attr);

            // dont show tile
            if (!attr.isShowTitleRow)
                rowCount += GetContentRowCount(attr);


            return base.GetPropertyHeight(property, label) * rowCount;

            //---------- methods
            static int GetContentRowCount(ListItemDrawAttribute attr)
            {
                if (attr.countInARow > 0)
                {
                    return attr.propNames.Length / attr.countInARow;
                }

                return 1;
            }
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
                attr.isTitleUnfold = EditorGUI.Foldout(pos, attr.isTitleUnfold, label,true);
                pos.y += pos.height;

                if (!attr.isTitleUnfold)
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
                var xOffset = i == 0 ? 0 : pos.width;
                SetPos(ref pos, xOffset, propWidth,i);

                ShowPropertyOrLabel(pos, propName, prop);
                //EditorGUI.LabelField(pos, GUIContentEx.TempContent(propName));
            }

            EditorGUI.EndProperty();

            //-------------- methods
            
            static void ShowPropertyOrLabel(Rect pos, string propName, SerializedProperty prop)
            {
                if (prop != null)
                {
                    EditorGUI.PropertyField(pos, prop, GUIContent.none);
                }
                else if (prop == null)
                {
                    EditorGUI.LabelField(pos, GUIContentEx.TempContent(propName));
                }
            }
            
            void SetPos(ref Rect pos, float offsetX, float width, int index)
            {
                pos.x += offsetX;
                pos.width = width;

                // a new row
                if (attr.countInARow > 0 && index > 0 && (index % attr.countInARow == 0))
                {
                    pos.x = position.x;
                    pos.y += EditorGUIUtility.singleLineHeight;
                }
            }
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
        /// <summary>
        /// label or propertyNames
        /// </summary>
        public string[] propNames;

        /// <summary>
        /// width = [0,1]  : percent
        /// width = 0 : use last empty space
        /// </summary>
        public float[] propWidths;

        /// <summary>
        /// show a foldable title
        /// </summary>
        public bool isShowTitleRow;

        /// <summary>
        /// is title folded?
        /// </summary>
        public bool isTitleUnfold;
        public int countInARow;
        public ListItemDrawAttribute(string propNamesStr,string widthStr,char splitChar=',',int countInARow=0)
        {
            propNames = propNamesStr.SplitBy(splitChar);
            propWidths = widthStr.SplitBy(splitChar)
                .Select(str=> string.IsNullOrEmpty(str) ? 0:Convert.ToSingle(str))
                .ToArray();

            this.countInARow = countInARow;
        }
    }
}
