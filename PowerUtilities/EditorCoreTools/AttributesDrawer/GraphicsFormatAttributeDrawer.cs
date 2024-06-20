#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;

    [CustomPropertyDrawer(typeof(GraphicsFormatSearchableAttribute))]
    public class GraphicsFormatSearchableAttributeDrawer : PropertyDrawer
    {
        GraphicsFormatSearchProvider provider;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as GraphicsFormatSearchableAttribute;

            provider = new();
            provider.isReadTextFile = attr.isReadTextFile;
            provider.onSelectedChanged = format =>
            {
                //property.serializedObject.Update();
                property.enumValueIndex = format;
                property.serializedObject.ApplyModifiedProperties();
            };

            var pos = position;
            pos.width = EditorGUIUtility.labelWidth;

            EditorGUI.LabelField(pos, label);
            pos.x += pos.width;
            pos.width = position.width - pos.width;
            var isClicked = GUI.Button(pos, ((GraphicsFormat)property.enumValueIndex).ToString(), EditorStyles.popup);

            if (isClicked)
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
            }
        }
    }
}
#endif