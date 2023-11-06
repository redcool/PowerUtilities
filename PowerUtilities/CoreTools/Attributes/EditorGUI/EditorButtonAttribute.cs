namespace PowerUtilities
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(EditorButtonAttribute))]
    public class EditorButtonAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //property.serializedObject.Update();

            position = EditorGUI.IndentedRect(position);
            if(GUI.Button(position, label))
            {
                property.boolValue = true;
            }
            //property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
#endif

    public class EditorButtonAttribute : PropertyAttribute
    {

    }
}
