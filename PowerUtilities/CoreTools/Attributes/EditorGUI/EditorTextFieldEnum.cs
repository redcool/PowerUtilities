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
    [CustomPropertyDrawer(typeof(EditorTextFieldEnum))]
    public class EditorTextFieldPopupDrawer : PropertyDrawer
    {
        static GenericMenu menu = new GenericMenu();

        static SerializedProperty curSelectedProp;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorTextFieldEnum;

            var pos = position;
            pos.width = position.width - 20;
            EditorGUI.PropertyField(pos, property, label);

            pos.x += pos.width;
            pos.width = 20;

            if (GUI.Button(pos, "↓"))
            {

                if (attr.type != null && !string.IsNullOrEmpty(attr.memberName))
                {
                    var names = attr.type.GetMemberValue<string[]>(attr.memberName, null, null);

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
                menu.AddItem(new GUIContent(name),true, OnMenuItemSelected, name);
            });
        }
    }
#endif


    [AttributeUsage(AttributeTargets.Field)]
    public class EditorTextFieldEnum : PropertyAttribute
    {
        /// <summary>
        /// get names from this Type
        /// </summary>
        public Type type;

        /// <summary>
        /// get names from Type.fieldName,like QualitySettings.names
        /// </summary>
        public string memberName;

        /// <summary>
        /// names form enum
        /// </summary>
        public Type defaultEnumType;
    }
}
