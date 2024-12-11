namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using System.Linq;
    using System.Reflection;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] names = default;

            var attr = attribute as EnumFlagsAttribute;

            // use enum property 
            if (attr.type == null)
            {
                names = property.enumNames;
            }
            else
            {
                if (!string.IsNullOrEmpty(attr.memberName))
                    names = attr.type.GetMemberValue<string[]>(attr.memberName, null, null);
            }

            if (attr.isFlags)
            {
                property.intValue = EditorGUI.MaskField(position, label, property.intValue, names);
            }
            else
            {
                var contents = names.Select(n => new GUIContent(n)).ToArray();
                property.intValue = EditorGUI.Popup(position, label, property.intValue, contents);
            }
        }

    }

#endif

    /// <summary>
    /// flags Enum
    /// like LayerMask
    /// 
    /// demo:
    ///         [EnumFlags(isFlags = false, type = typeof(QualitySettings), memberName = "names")]
    ///         public int qualityLevel = 3;
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class EnumFlagsAttribute : PropertyAttribute
    {
        /// <summary>
        /// show as Mask(multi selected) or popup (one selected)
        /// </summary>
        public bool isFlags;

        /// <summary>
        /// get names from this Type(enum or other/memberName)
        /// </summary>
        public Type type;

        /// <summary>
        /// get names from Type.fieldName,like QualitySettings.names
        /// </summary>
        public string memberName;
        public EnumFlagsAttribute(bool IsFlags = true)
        {
            this.isFlags = IsFlags;
        }
    }
}