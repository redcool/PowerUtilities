#if UNITY_EDITOR
namespace PowerUtilities
{
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
            var isDisable = string.IsNullOrEmpty(attr.targetPropName);
            if(!isDisable)
            {
                //colorTargetInfos.Array.data[0].rtSizeMode
                var prop = property.serializedObject.FindProperty(attr.targetPropName);
                if (prop == null) // check parent property
                {
                    //var curPropPath = property.GetPropertyObjectPath();
                    prop = property.FindPropertyInObject(attr.targetPropName);
                }
                isDisable = prop != null ? prop.intValue == 0 : false;
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