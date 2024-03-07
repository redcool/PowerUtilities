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
            var attr = attribute as EditorButtonAttribute;
            
            if (!string.IsNullOrEmpty(attr.text))
                label.text = attr.text;

            if (!string.IsNullOrEmpty(attr.imageAssetPath))
                label.image = AssetDatabase.LoadAssetAtPath<Texture>(attr.imageAssetPath);

            position = EditorGUI.IndentedRect(position);
            if (GUI.Button(position, label))
            {
                property.boolValue = true;

                CallTargetMethold(property, attr);
            }
            //property.serializedObject.ApplyModifiedPropertiesWithoutUndo();


            //================ inner methods
            static void CallTargetMethold(SerializedProperty property, EditorButtonAttribute attr)
            {
                if (!string.IsNullOrEmpty(attr.onClickCall))
                {
                    var inst = property.serializedObject.targetObject;
                    var instType = inst.GetType();
                    ReflectionTools.InvokeMember(instType, attr.onClickCall, inst, null);
                }
            }
        }
    }
#endif

    public class EditorButtonAttribute : PropertyAttribute
    {
        public string onClickCall;
        public string text;
        public string imageAssetPath;
    }
}
