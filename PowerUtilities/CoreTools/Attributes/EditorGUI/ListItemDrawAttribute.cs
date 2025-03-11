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
            var attr = attribute as ListItemDrawAttribute;

            int rowCount = GetRowCount(property, attr);


            // show title and title is unfold
            if (attr.isShowTitleRow && attr.isTitleUnfold)
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
        /// <summary>
        /// only support 1 child
        /// </summary>
        /// <param name="property"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static int GetRowCount(SerializedProperty property, ListItemDrawAttribute attr)
        {
            var rowCount = Mathf.Max(1,attr.rowCount);
            if (!string.IsNullOrEmpty(attr.rowCountArrayPropName))
            {
                var rowCountProp = property.FindPropertyRelative(attr.rowCountArrayPropName);
                if (rowCountProp != null && rowCountProp.isExpanded)
                    rowCount += rowCountProp.arraySize+3;
            }

            return rowCount;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            var attr = attribute as ListItemDrawAttribute;
            var labelTooltips = "";

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
                SetPos(ref pos, xOffset, propWidth, i);

                // find next prop tooltips
                if(prop == null)
                    labelTooltips = FindNextPropTooltips(property, attr, i);

                ShowPropertyOrLabel(pos, propName, prop, labelTooltips);
                //EditorGUI.LabelField(pos, GUIContentEx.TempContent(propName));
            }
            if(attr.propNames.Length == 0)
            {
                EditorGUI.SelectableLabel(pos, property.stringValue);
            }

            EditorGUI.EndProperty();

            //-------------- methods
            
            static void ShowPropertyOrLabel(Rect pos, string propName, SerializedProperty prop,string labelTooltips)
            {
                if (prop != null)
                {
                    EditorGUI.PropertyField(pos, prop, GUIContent.none);
                }
                else
                {
                    EditorGUI.LabelField(pos, GUIContentEx.TempContent(propName, labelTooltips));
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

            static string FindNextPropTooltips(SerializedProperty property, ListItemDrawAttribute attr, int itemIndex)
            {
                if (itemIndex + 1 < attr.propNames.Length)
                {
                    var nextProp = property.FindPropertyRelative(attr.propNames[itemIndex + 1]);
                    if (nextProp != null)
                        return nextProp.tooltip;
                }
                return "";
            }
        }

        private static void UpdatePropWidth(ref Rect position, ref Rect pos, ref float propWidth)
        {
            if (propWidth <= 0)
                propWidth = position.width - pos.xMax;
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
        /// <summary>
        /// show item count in one row
        /// </summary>
        public int countInARow;
        /// <summary>
        /// row count
        /// </summary>
        public int rowCount = 1;
        /// <summary>
        /// get row count from rowCountPropName(children's arrat property name)
        /// </summary>
        public string rowCountArrayPropName = "";
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
