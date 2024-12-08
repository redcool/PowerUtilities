#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    [CustomPropertyDrawer(typeof(ShowInSceneViewAttribute))]
    public class ShowInSceneViewAttributeDrawer : PropertyDrawer
    {
        object targetObject;
        List<Vector3> posList = new();
        

        public ShowInSceneViewAttributeDrawer()
        {
            SceneView.duringSceneGui += OnDrawSceneView;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label);
            targetObject = property.serializedObject.targetObject;
        }

        private void OnDrawSceneView(SceneView view)
        {
            if (targetObject == null)
                return;

            var attr = attribute as ShowInSceneViewAttribute;
            var baseType = attr.containerType ?? typeof(IEnumerable<Vector3>);

            var value = fieldInfo.GetValue(targetObject);
            if (!value.GetType().IsImplementOf(baseType))
                return;
            
            var buttonSize = new Vector2(120, 18);

            posList.Clear();
            posList.AddRange((IEnumerable<Vector3>)value);
            
            Handles.BeginGUI();
            for (int i = 0; i < posList.Count; ++i)
            {
                var pos = posList[i];
                var guiPos = HandleUtility.WorldToGUIPoint(pos);
                var guiRect = new Rect(guiPos, buttonSize);

                GUI.Box(guiRect, pos.ToString());
            }
            Handles.EndGUI();
        }
    }
}
#endif