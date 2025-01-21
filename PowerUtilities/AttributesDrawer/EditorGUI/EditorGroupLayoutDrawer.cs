#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(EditorGroupLayoutAttribute))]
    public class EditorGroupLayoutDrawer : PropertyDrawer
    {
        (string groupName, bool isOn) groupInfo;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //var groupAttr = attribute as EditorGroupLayoutAttribute;
            //if (groupAttr.isHeader)
            //{
            //    //return 4;
            //    //return base.GetPropertyHeight(property, label);
            //}

            //if (MaterialGroupTools.IsGroupOn(groupAttr.groupName))
            //{
            //    return 0;
            //}
            return -2;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var groupAttr = attribute as EditorGroupLayoutAttribute;

            groupInfo.groupName = groupAttr.groupName;
            groupInfo.isOn = MaterialGroupTools.IsGroupOn(groupAttr.groupName);

            
            //draw header
            if (groupAttr.isHeader)
            {
                EditorGUITools.DrawFoldContent(ref groupInfo, () =>
                {

                });
                //groupInfo.isOn = EditorGUILayout.Foldout(groupInfo.isOn, groupInfo.groupName);
                MaterialGroupTools.SetState(groupInfo.groupName, groupInfo.isOn);
            }

            //draw contents
            if (!groupInfo.isOn)
                return;

            EditorGUI.indentLevel++;
            //var pos = EditorGUILayout.GetControlRect();
            //EditorGUI.PropertyField(pos, property, label,true);
            EditorGUILayout.PropertyField(property, label);
            EditorGUI.indentLevel--;
        }
    }

}
#endif