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
    [CustomPropertyDrawer(typeof(EditorBox))]
    public class EditorBoxEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0; // 
            var attr = (EditorBox)attribute;

            var lines = 0;
            if (attr.isFolded)
                lines = 1;

            if (attr.boxType == EditorBox.BoxType.VBox) {
                lines += attr.propNames.Length - 1;
            }

            return base.GetPropertyHeight(property, label) * lines;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (EditorBox)attribute;

            if (attr.propNames.Length == 0)
            {
                base.OnGUI(position, property, label);
                return;
            }

            var itemWidth = EditorGUIUtility.currentViewWidth / Mathf.Max(1,attr.propNames.Length)-20;

            
            if (attr.isFolded = BeginHeader(attr))
            {
                EditorGUI.indentLevel++;

                BeginBox(attr);

                foreach (var propName in attr.propNames)
                {
                    var prop = property.serializedObject.FindProperty(propName);
                    if (prop != null)
                    {
                        EditorGUILayout.PropertyField(prop, true,GUILayout.Width(itemWidth));
                    }
                }

                EndBox(attr);

                EditorGUI.indentLevel--;
            }

            EndHeader(attr);

            //===========
            static bool BeginHeader(EditorBox attr)
            {
                if (attr.isShowFoldout)
                    return EditorGUILayout.BeginFoldoutHeaderGroup(attr.isFolded, attr.header);
                else
                    EditorGUILayout.LabelField(attr.header,EditorStyles.boldLabel);
                return true;
            }

            static void EndHeader(EditorBox attr)
            {
                if (attr.isShowFoldout)
                    EditorGUILayout.EndFoldoutHeaderGroup();
            }

            static void BeginBox(EditorBox attr)
            {
                if (attr.boxType == EditorBox.BoxType.VBox)
                    EditorGUILayout.BeginVertical("Box");
                else
                    EditorGUILayout.BeginHorizontal("Box");
            }

            static void EndBox(EditorBox attr)
            {
                if (attr.boxType == EditorBox.BoxType.VBox)
                    EditorGUILayout.EndVertical();
                else
                    EditorGUILayout.EndHorizontal();
            }
        }
    }
#endif

    public class EditorBox : PropertyAttribute
    {
        public enum BoxType
        {
            VBox,HBox
        }

        public BoxType boxType;
        
        public string[] propNames;
        public string propName;

        public string header;
        public bool isShowFoldout;

        [HideInInspector]
        public bool isFolded=true;


        public EditorBox(string header, string propName)
        {
            if (!string.IsNullOrEmpty(propName))
                propNames = propName.SplitBy();
            this.header = header;
        }
    }
}
