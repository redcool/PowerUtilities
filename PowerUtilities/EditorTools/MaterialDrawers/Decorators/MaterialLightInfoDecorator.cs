#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// copy light info to material's value
    /// type 
    ///     direction  : copy direction direction
    ///     color : copy color 
    /// </summary>
    public class MaterialLightInfoDecorator : MaterialPropertyDrawer
    {
        public enum InfoType
        {
            direction, color
        }
        InfoType infoType;
        string groupName;

        Light light;
        GUIContent lightInfoContent = new GUIContent("Get from light", "Drop Light object");

        public MaterialLightInfoDecorator()
        {
            infoType = InfoType.direction;
        }
        public MaterialLightInfoDecorator(string type)
        {
            infoType = GetInfoType(type.ToLower());
        }
        public MaterialLightInfoDecorator(string groupName, string type) : this(type)
        {
            this.groupName = groupName;
        }

        InfoType GetInfoType(string typeStr) => typeStr switch
        {
            nameof(InfoType.direction) => InfoType.direction,
            nameof(InfoType.color) => InfoType.color,
            _ => InfoType.direction
        };

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (MaterialGroupTools.IsGroupOn(groupName))
                return base.GetPropertyHeight(prop, label, editor);
            return 0;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!MaterialGroupTools.IsGroupOn(groupName))
                return;

            if (!string.IsNullOrEmpty(groupName))
                EditorGUI.indentLevel++;

            var c = GUI.color; GUI.color = Color.gray;
            light = (Light)EditorGUI.ObjectField(position, lightInfoContent, light, typeof(Light), true);
            GUI.color = c;

            if (!string.IsNullOrEmpty(groupName))
                EditorGUI.indentLevel--;

            if (!light)
                return;

            switch (infoType)
            {
                case InfoType.direction:
                    prop.vectorValue = -light.transform.localToWorldMatrix.GetColumn(2);
                    break;
                case InfoType.color:
                    prop.colorValue = light.color.linear * light.intensity;
                    break;
            }

            light = null;
        }
    }
}
#endif