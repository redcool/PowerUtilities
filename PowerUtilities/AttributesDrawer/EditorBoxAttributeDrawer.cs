#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(EditorBoxAttribute))]
    public class EditorBoxAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0; // 
            var attr = (EditorBoxAttribute)attribute;

            var lines = 0;
            if (attr.isFolded)
                lines = 1;

            if (attr.boxType == EditorBoxAttribute.BoxType.VBox)
            {
                lines += attr.PropNames.Length - 1;
            }

            return base.GetPropertyHeight(property, label) * lines;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (EditorBoxAttribute)attribute;

            if (attr.PropNames.Length == 0)
            {
                base.OnGUI(position, property, label);
                return;
            }

            //EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth / Mathf.Max(1, attr.PropNames.Length) / 2;
            EditorGUITools.UpdateLabelWidth(EditorGUIUtility.currentViewWidth / Mathf.Max(1, attr.PropNames.Length) / 2);
            var itemWidth = EditorGUIUtility.currentViewWidth;
            if (attr.boxType == EditorBoxAttribute.BoxType.HBox)
                itemWidth /= Mathf.Max(1, attr.PropNames.Length);


            if (attr.isFolded = BeginHeader(attr))
            {
                EditorGUI.indentLevel++;

                BeginBox(attr);

                foreach (var propName in attr.PropNames)
                {
                    var prop = property.serializedObject.FindProperty(propName);
                    if (prop != null)
                    {
                        EditorGUILayout.PropertyField(prop, true, GUILayout.Width(itemWidth));
                    }
                }

                EndBox(attr);

                EditorGUI.indentLevel--;
            }

            EndHeader(attr);
            EditorGUITools.RestoreLabelWidth();
        }
        //===========
        static bool BeginHeader(EditorBoxAttribute attr)
        {
            if (attr.isShowFoldout)
                return EditorGUILayout.BeginFoldoutHeaderGroup(attr.isFolded, attr.header);
            else
                EditorGUILayout.LabelField(attr.header, EditorStyles.boldLabel);
            return true;
        }

        static void EndHeader(EditorBoxAttribute attr)
        {
            if (attr.isShowFoldout)
                EditorGUILayout.EndFoldoutHeaderGroup();
        }

        static void BeginBox(EditorBoxAttribute attr)
        {
            if (attr.boxType == EditorBoxAttribute.BoxType.VBox)
                EditorGUILayout.BeginVertical("Box");
            else
                EditorGUILayout.BeginHorizontal("Box");
        }

        static void EndBox(EditorBoxAttribute attr)
        {
            if (attr.boxType == EditorBoxAttribute.BoxType.VBox)
                EditorGUILayout.EndVertical();
            else
                EditorGUILayout.EndHorizontal();
        }
    }
}
#endif