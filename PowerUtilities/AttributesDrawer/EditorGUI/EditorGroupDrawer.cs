#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(EditorGroupAttribute))]
    public class EditorGroupDrawer : PropertyDrawer
    {
        (string groupName, bool isOn) groupInfo;
        /// <summary>
        /// Title line 's height
        /// 20,too narrow, 22 more better
        /// </summary>
        public const float TITLE_LINE_HEIGHT = 22;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var groupAttr = attribute as EditorGroupAttribute;
            var isGroupOn = MaterialGroupTools.IsGroupOn(groupAttr.groupName);
            // header path
            if (groupAttr.isHeader)
            {
                if (!isGroupOn)
                    return TITLE_LINE_HEIGHT;

                // prop height
                var height = EditorGUI.GetPropertyHeight(property, property.isExpanded);
                // add header's height
                height += TITLE_LINE_HEIGHT;
                //height += EditorGUIUtility.singleLineHeight;
                return height;
            }

            // other content path
            if (isGroupOn)
            {
                return EditorGUI.GetPropertyHeight(property, property.isExpanded);
            }

            return -2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var groupAttr = attribute as EditorGroupAttribute;

            //return;
            groupInfo.groupName = groupAttr.groupName;
            groupInfo.isOn = MaterialGroupTools.IsGroupOn(groupAttr.groupName);

            //draw header
            if (groupAttr.isHeader)
            {
                if (groupInfo.isOn)
                    position.height -= TITLE_LINE_HEIGHT;

                groupInfo.isOn = EditorGUITools.DrawTitleFoldout(position, groupInfo.isOn, GUIContentEx.TempContent(groupInfo.groupName, groupAttr.tooltip), groupAttr.titleColor, EditorStyles.helpBox);

                MaterialGroupTools.SetState(groupInfo.groupName, groupInfo.isOn);
                position.y += TITLE_LINE_HEIGHT;
            }

            //draw contents
            if (!groupInfo.isOn)
                return;

            EditorGUI.indentLevel += groupAttr.intentOffset;
            EditorGUI.PropertyField(position, property, new GUIContent(property.displayName, label.tooltip));
            EditorGUI.indentLevel -= groupAttr.intentOffset;
        }
    }

}
#endif