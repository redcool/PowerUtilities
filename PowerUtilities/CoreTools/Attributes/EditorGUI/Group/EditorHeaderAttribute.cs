namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(EditorHeaderAttribute))]
    public class EditorHeaderAttributeEditor : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var attr = attribute as EditorHeaderAttribute;
            var isGroupOn = MaterialGroupTools.IsGroupOn(attr.groupName);

            if (isGroupOn)
                return base.GetHeight();
            return -20;
        }
        public override void OnGUI(Rect position)
        {
            var attr = attribute as EditorHeaderAttribute;
            var isGroupOn = MaterialGroupTools.IsGroupOn(attr.groupName);

            if (!isGroupOn)
                return;
            var pos = position;
            pos.x += 15;
            EditorGUI.LabelField(pos,GUIContentEx.TempContent(attr.header),EditorStyles.boldLabel);
        }
    }

#endif
    public class EditorHeaderAttribute : PropertyAttribute
    {
        public string groupName;
        public string header;
        public EditorHeaderAttribute(string groupName,string header)
        {
            this.groupName = groupName; 
            this.header = header;
        }
    }
}
