namespace PowerUtilities
{
    using System;
    using UnityEngine;
#if UNITY_EDITOR
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
                if(!isGroupOn)
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

        public static bool DrawTitleFoldout(Rect position, bool isOn, string title, Color titleColor,GUIStyle style)
        {
            var pos = position;
            pos.height = TITLE_LINE_HEIGHT;

            // show only a style
            GUI.BeginGroup(pos, style);
            GUI.EndGroup();

            var isFold = EditorGUI.Foldout(pos, isOn, title, true);
            return isFold;
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

                groupInfo.isOn = DrawTitleFoldout(position, groupInfo.isOn, groupInfo.groupName, groupAttr.titleColor, EditorStyles.helpBox);

                MaterialGroupTools.SetState(groupInfo.groupName, groupInfo.isOn);
                position.y += TITLE_LINE_HEIGHT;
            }

            //draw contents
            if (!groupInfo.isOn)
                return;

            EditorGUI.indentLevel += groupAttr.intentOffset;
            EditorGUI.PropertyField(position, property, new GUIContent(property.displayName,label.tooltip));
            EditorGUI.indentLevel -= groupAttr.intentOffset;
        }
    }
#endif

    /// <summary>
    /// Show toogle group in inspector gui( fields )
    /// 
    /// [EditorGroup("Fog",true)] public bool _IsGlobalFogOn;
    /// [EditorGroup("Fog")][Range(0, 1)] public float _GlobalFogIntensity = 1;
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Field)]
    public class EditorGroupAttribute : PropertyAttribute
    {
        public string groupName;
        public bool isHeader;
        public Color titleColor;
        public int intentOffset = 1;

        public EditorGroupAttribute(string groupName, bool isHeader = false, string titleColorStr = "#404E7C")
        {
            this.isHeader = isHeader;
            this.groupName=groupName;
            ColorUtility.TryParseHtmlString(titleColorStr, out titleColor);
        }
    }

}