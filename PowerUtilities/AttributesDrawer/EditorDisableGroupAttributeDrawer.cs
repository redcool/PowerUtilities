#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    [CustomPropertyDrawer(typeof(EditorDisableGroupAttribute))]
    public class EditorDisableGroupAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorDisableGroupAttribute;
            var lines = Mathf.Max(attr.heightScale, property.CountInProperty());

            return base.GetPropertyHeight(property, label) * lines + 2;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorDisableGroupAttribute;

            // check 1 prop
            var isDisable = string.IsNullOrEmpty(attr.targetPropName);
            if (!isDisable)
            {
                var prop = property.FindPropertyInObject(attr.targetPropName);
                isDisable = prop != null ? prop.intValue == 0 : false;
            }

            // check multi props
            if (attr.TargetPropValues.IsValid())
            {
                isDisable = attr.TargetPropValues
                    .Select(propName => property.FindPropertyInObject(propName))
                    .Where(prop => prop != null)
                    .Any(prop => prop.intValue == 0);
            }

            if (attr.isRevertMode)
                isDisable = !isDisable;
            
            EditorGUI.BeginDisabledGroup(isDisable);
            EditorGUI.PropertyField(position, property, label,true);
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif