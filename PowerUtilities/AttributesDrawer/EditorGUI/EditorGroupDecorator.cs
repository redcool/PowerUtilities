#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using UnityEditor;

    //[CustomPropertyDrawer(typeof(EditorGroupAttribute))]
    public class EditorGroupDecorator : DecoratorDrawer
    {
        (string groupName, bool isOn) groupInfo;

        const float LINE_HEIGHT = 18;

        public override float GetHeight()
        {
            return 18;
        }

        public static bool DrawTitleFoldout(Rect position,bool isOn,string title,Color titleColor)
        {
            EditorGUI.DrawRect(EditorGUI.IndentedRect(position), titleColor);

            return EditorGUI.Foldout(position, isOn, title, true);
        }

        public override void OnGUI(Rect position)
        {
            var attr = attribute as EditorGroupAttribute;

            //return;
            groupInfo.groupName = attr.groupName;
            groupInfo.isOn = MaterialGroupTools.IsGroupOn(attr.groupName);

            //draw header
            GUIStyle style = "Box";
            //style.padding.left=15;
            style.alignment = TextAnchor.LowerLeft;

            groupInfo.isOn = DrawTitleFoldout(position, groupInfo.isOn, groupInfo.groupName, attr.titleColor);

            MaterialGroupTools.SetState(groupInfo.groupName, groupInfo.isOn);
        }
    }

}
#endif