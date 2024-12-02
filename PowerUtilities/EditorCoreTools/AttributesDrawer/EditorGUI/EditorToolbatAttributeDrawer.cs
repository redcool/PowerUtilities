#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections.Generic;
    using UnityEngine;

    using UnityEditor;

    [CustomPropertyDrawer(typeof(EditorToolbarAttribute))]
    public class EditorToolbatAttributeDrawer : PropertyDrawer
    {
        List<GUIContent> labelList = new List<GUIContent>();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            labelList.Clear();
            var attr = attribute as EditorToolbarAttribute;

            for (int i = 0; i < attr.texts.Length; ++i)
            {
                var text = attr.texts[i];
                var tooltip = text;
                if (attr.tooltips != null && attr.tooltips.Length > i)
                    tooltip = attr.tooltips[i];

                label = new GUIContent(text, tooltip);

                var imagePath = (attr.imageAssetPaths != null && attr.imageAssetPaths.Length > i) ? attr.imageAssetPaths[i] : "";
                if (!string.IsNullOrEmpty(imagePath))
                    label.image = AssetDatabase.LoadAssetAtPath<Texture>(imagePath);

                labelList.Add(label);
            }

            position = EditorGUI.IndentedRect(position);

            var newId = GUI.Toolbar(position, property.intValue, labelList.ToArray());
            if(newId != property.intValue)
            {
                property.intValue = newId;
                CallTargetMethold(property, attr, newId);
            }


            //================ inner methods
            static void CallTargetMethold(SerializedProperty property, EditorToolbarAttribute attr,int buttonId)
            {
                if (string.IsNullOrEmpty(attr.onClickCall))
                    return;

                property.UpdatePropertyValue(inst =>
                {
                    var instType = inst.GetType();
                    ReflectionTools.InvokeMember(instType, attr.onClickCall, inst, new object[] { buttonId });
                });
            }
        }
    }
}
#endif