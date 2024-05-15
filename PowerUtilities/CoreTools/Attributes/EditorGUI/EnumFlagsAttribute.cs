namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

#if UNITY_EDITOR
    using UnityEditor;
    using System.Linq;
    using System.Reflection;

    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] names = default;

            var attr = attribute as EnumFlagsAttribute;

            // use enum property 
            if(attr.namesFieldType == null)
            {
                names = property.enumNames;
            }
            else
            {
                SetupNamesFromType(ref names, attr);
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


            //============= inner methods
            static void SetupNamesFromType(ref string[] names, EnumFlagsAttribute attr)
            {
                if (attr.namesFieldType != null)
                {
                    var info = attr.namesFieldType.GetMember(attr.namesFieldName).FirstOrDefault();
                    if (info == null)
                        return;

                    if (info is PropertyInfo propertyInfo)
                    {
                        names = propertyInfo.GetValue(null) as string[];
                    }

                    if (info is FieldInfo fieldInfo)
                    {
                        names = fieldInfo.GetValue(null) as string[];
                    }
                }
            }
        }
    }

#endif

    /// <summary>
    /// flags Enum
    /// like LayerMask
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class EnumFlagsAttribute : PropertyAttribute
    {
        /// <summary>
        /// show as Mask(multi selected) or popup (one selected)
        /// </summary>
        public bool isFlags;

        /// <summary>
        /// get names from this Type
        /// </summary>
        public Type namesFieldType;

        /// <summary>
        /// get names from Type.fieldName,like QualitySettings.names
        /// </summary>
        public string namesFieldName;
        public EnumFlagsAttribute(bool IsFlags = true)
        {
            this.isFlags = IsFlags;
        }
    }
}