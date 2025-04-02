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
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.IndentedRect(position);

            var attr = (EditorBoxAttribute)attribute;
            if (attr.PropNames.Length == 0)
            {
                base.OnGUI(position, property, label);
                return;
            }


            if (attr.isFolded = BeginHeader(attr))
            {
                //EditorGUI.indentLevel++;

                // setup widths
                var viewWidth = EditorGUITools.GetCurrentViewWidthWithIndent();
                var labelWidth = viewWidth / Mathf.Max(1, attr.PropNames.Length) * 0.45f;

                //labelWidth = Mathf.Max(labelWidth, 120);
                EditorGUITools.UpdateLabelWidth(labelWidth);

                BeginBox(attr);

                //foreach (var propName in attr.PropNames)
                for (int i = 0; i < attr.PropNames.Length; i++) 
                {
                    var propName = attr.PropNames[i];
                    var propWidthRate = attr.propWidthRates[i];

                    var prop = property.serializedObject.FindProperty(propName);
                    if (prop != null)
                    {
                        //use same label
                        //EditorGUILayout.PropertyField(prop, true);
                        EditorGUILayout.PropertyField(prop, true, GUILayout.Width(viewWidth * propWidthRate));
                    }
                }

                EndBox(attr);

                //EditorGUI.indentLevel--;
            }

            EndHeader(attr);
            EditorGUITools.RestoreLabelWidth();
        }
        //===========
        static bool BeginHeader(EditorBoxAttribute attr)
        {
            // empty title 
            if (string.IsNullOrEmpty(attr.header))
                return true;

            // show title
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