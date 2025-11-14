#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Linq;
    using System.Reflection;
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

        public bool IsDisabled(SerializedProperty property, EditorDisableGroupAttribute attr)
        {
            // check 1 prop
            var isDisable = string.IsNullOrEmpty(attr.targetPropName);
            if (!isDisable)
            {
                var prop = property.FindPropertyInObject(attr.targetPropName);
                if(prop == null)
                    return false;

                if(prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    isDisable = prop.objectReferenceValue;
                }
                else
                {
                    isDisable = prop.intValue == 0;
                }
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
            return isDisable;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorDisableGroupAttribute;
            attr.isGroupDisabled = IsDisabled(property,attr);
            
            EditorGUI.BeginDisabledGroup(attr.isGroupDisabled);
            EditorGUI.PropertyField(position, property, label,true);
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif