namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using Object = UnityEngine.Object;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EditorObjectFieldAttribute))]
    public class EditorObjectFieldAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //property.serializedObject.Update();
            var obj = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(Object), true);
            if (property.serializedObject.targetObject != null)
                fieldInfo.SetValue(property.serializedObject.targetObject, obj);
            //property.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    /// <summary>
    /// Show inspector object field can use sceneObject
    /// </summary>
    public class EditorObjectFieldAttribute : PropertyAttribute
    {

    }
}
