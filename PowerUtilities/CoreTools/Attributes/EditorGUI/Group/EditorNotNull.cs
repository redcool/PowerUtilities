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
    [CustomPropertyDrawer(typeof(EditorNotNull))]
    public class EditorNotNullDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var isValid = property.objectReferenceValue != null;

            GUI.contentColor = !isValid ? Color.red : Color.white;

            EditorGUI.PropertyField(position, property, label,true);
        }
    }
#endif

    public class EditorNotNull : PropertyAttribute
    {

    }
}
