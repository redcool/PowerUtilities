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
    ///     forward  : copy forward direction
    ///     color : copy color 
    /// </summary>
    public class MaterialLightInfoDecorator : MaterialPropertyDrawer
    {
        public enum InfoType
        {
            forward, color
        }
        InfoType infoType;

        Light light;

        public MaterialLightInfoDecorator()
        {
            infoType = InfoType.forward;
        }
        public MaterialLightInfoDecorator(string type)
        {
            infoType = GetInfoType(type.ToLower());
        }

        InfoType GetInfoType(string typeStr) => typeStr switch
        {
            nameof(InfoType.forward) => InfoType.forward,
            nameof(InfoType.color) => InfoType.color,
            _ => InfoType.forward
        };

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            
            var c = GUI.color; GUI.color = Color.gray;
            light = (Light)EditorGUI.ObjectField(position, new GUIContent("Get from light"), light, typeof(Light), true);
            GUI.color = c;

            if (!light)
                return;

            switch (infoType)
            {
                case InfoType.forward:
                    prop.vectorValue  = - light.transform.localToWorldMatrix.GetColumn(2);
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