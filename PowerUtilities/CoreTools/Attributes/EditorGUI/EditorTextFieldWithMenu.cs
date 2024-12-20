using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PowerUtilities
{

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EditorTextFieldWithMenu))]
    public class EditorTextFieldWithMenuDrawer : PropertyDrawer
    {
        GUIContent showArrowGUI = new GUIContent("↓");

        static GenericMenu menu = new GenericMenu();

        static SerializedProperty curSelectedProp;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorTextFieldWithMenu;

            var pos = position;
            pos.width = position.width - 20;
            EditorGUI.PropertyField(pos, property, label);

            pos.x += pos.width;
            pos.width = 20;

            showArrowGUI.tooltip = attr.tooltips ?? attr.staticMemberName;

            if (GUI.Button(pos, showArrowGUI))
            {
                // need clear menu
                menu = new GenericMenu();

                if (attr.type != null && !string.IsNullOrEmpty(attr.staticMemberName))
                {
                    var names = attr.type.GetMemberValue<string[]>(attr.staticMemberName, null, null);
                    // use default
                    if(names == null && attr.defaultEnumType != null)
                    {
                        names = Enum.GetNames(attr.defaultEnumType);
                    }

                    // show names
                    if (names != null)
                    {
                        SetupMenu(menu, names, property);
                        menu.ShowAsContext();
                    }
                }
            }
        }

        void OnMenuItemSelected(object nameObj)
        {
            curSelectedProp.serializedObject.Update();
            curSelectedProp.stringValue = (string)nameObj;
            curSelectedProp.serializedObject.ApplyModifiedProperties();
        }

        void SetupMenu(GenericMenu menu, IEnumerable<string> names, SerializedProperty property) {
            curSelectedProp = property;
            names.ForEach((name, id) =>
            {
                // separator
                if (string.IsNullOrEmpty(name) || name == "separator")
                {
                    menu.AddSeparator("");
                }
                else
                    menu.AddItem(new GUIContent(name), true, OnMenuItemSelected, name);
            });
        }
    }
#endif

    /// <summary>
    /// Draw property with dropdownList(string[] from (type.memberName)
    /// 
    ///     [EditorTextFieldWithMenu(type = typeof(CreateRenderTarget),memberName = nameof(CreateRenderTarget.GetColorTargetNames))]
    ///     public string[] colorTargetNames = new[] { nameof(ShaderPropertyIds._CameraColorAttachmentA) };
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EditorTextFieldWithMenu : PropertyAttribute
    {
        /// <summary>
        /// get names from this Type
        /// </summary>
        public Type type;

        /// <summary>
        /// get names from Type.fieldName,like QualitySettings.names
        /// </summary>
        public string staticMemberName;

        /// <summary>
        /// names form enum
        /// </summary>
        public Type defaultEnumType;

        /// <summary>
        /// tooltips
        /// </summary>
        public string tooltips;
    }
}
