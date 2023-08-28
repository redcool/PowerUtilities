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

        const float LINE_HEIGHT = 18;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var groupAttr = attribute as EditorGroupAttribute;
            var isGroupOn = MaterialGroupTools.IsGroupOn(groupAttr.groupName);
            if (groupAttr.isHeader)
            {
                return isGroupOn ? LINE_HEIGHT * 2 : LINE_HEIGHT;
            }

            if (isGroupOn)
            {
                return LINE_HEIGHT;
            }

            return -2;
        }

        public static bool DrawTitleFoldout(Rect position,bool isOn,string title,string titleColorStr = "#404E7C")
        {
            ColorUtility.TryParseHtmlString(titleColorStr, out var c);
            EditorGUI.DrawRect(position, c);

            return EditorGUI.Foldout(position, isOn, title, true);
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
                GUIStyle style = "Box";
                //style.padding.left=15;
                style.alignment = TextAnchor.LowerLeft;

                if (groupInfo.isOn)
                    position.height -= LINE_HEIGHT;

                groupInfo.isOn = DrawTitleFoldout(position, groupInfo.isOn, groupInfo.groupName, groupAttr.titleColorStr);
                
                MaterialGroupTools.SetState(groupInfo.groupName, groupInfo.isOn);
                position.y += LINE_HEIGHT;
            }

            //draw contents
            if (!groupInfo.isOn)
                return;

            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position,property, new GUIContent(property.displayName));
            EditorGUI.indentLevel--;
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
        public string titleColorStr;

        public EditorGroupAttribute(string groupName, bool isHeader = false, string titleColorStr = "#404E7C")
        {
            this.isHeader = isHeader;
            this.groupName=groupName;
            this.titleColorStr=titleColorStr;
        }
    }

}