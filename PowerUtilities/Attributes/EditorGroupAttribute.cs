namespace PowerUtilities
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
    using System;

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

            return 0;
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


                groupInfo.isOn = EditorGUI.BeginFoldoutHeaderGroup(position, groupInfo.isOn, groupInfo.groupName);
                EditorGUI.EndFoldoutHeaderGroup();
            //EditorGUI.DrawRect(position, Color.gray);

                //groupInfo.isOn = EditorGUI.Foldout(position, groupInfo.isOn, groupInfo.groupName, true, EditorStyles.foldoutHeader);
                MaterialGroupTools.GroupDict[groupInfo.groupName] = groupInfo.isOn;
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
    /// Show toogle group in inspector gui
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

        public EditorGroupAttribute(string groupName, bool isHeader = false)
        {
            this.isHeader = isHeader;
            this.groupName=groupName;
        }
    }

}