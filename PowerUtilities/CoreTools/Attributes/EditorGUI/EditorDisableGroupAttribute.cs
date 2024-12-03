namespace PowerUtilities
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EditorDisableGroupAttribute))]
    public class EditorDisableGroupEditor : PropertyDrawer
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
                var prop = property.serializedObject.FindProperty(attr.targetPropName);
                isDisable = prop != null ? prop.intValue == 0 : true;
            }

            if (attr.isRevertMode)
                isDisable = !isDisable;
            
            EditorGUI.BeginDisabledGroup(isDisable);
            EditorGUI.PropertyField(position, property, label,true);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif

    /// <summary>
    /// show editor gui in disable mode
    /// </summary>
    public class EditorDisableGroupAttribute : PropertyAttribute
    {
        /// <summary>
        /// Show DisableGroup when empty,
        /// otherwise check targetPropName's intValue
        /// </summary>
        public string targetPropName;

        /// <summary>
        /// show gui in disableGroup when {targetPropName} is enabled
        /// </summary>
        public bool isRevertMode;

        /// <summary>
        /// line count,
        /// </summary>
        public int heightScale = 1;
    }
}
