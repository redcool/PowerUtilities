namespace PowerUtilities
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
    using static EditorButtonAttribute;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(EditorButtonAttribute))]
    public class EditorButtonAttributeDrawer : PropertyDrawer
    {

        public string GetNameTypeText(EditorButtonAttribute attr, SerializedProperty prop) => attr.nameType switch
        {
            NameType.AttributeText => attr.text,
            NameType.MethodName => attr.onClickCall,
            _ => prop.displayName,
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //property.serializedObject.Update();
            var attr = attribute as EditorButtonAttribute;

            label.text = GetNameTypeText(attr, property);
            label.tooltip = attr.tooltip;

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
    /// <summary>
    /// show GUI.Button for bool,(int [0,1])
    /// 
    ///     [EditorButton(onClickCall ="Test")]
    ///     public bool isTest;
    ///     
    ///     Test(){}
    /// </summary>
    public class EditorButtonAttribute : PropertyAttribute
    {
        public enum NameType
        {
            MethodName, // method name called
            FieldName, // current property name
            AttributeText // text
        }
        /// <summary>
        /// call instance method
        /// </summary>
        public string onClickCall;
        /// <summary>
        /// button 's text
        /// </summary>
        public string text;
        /// <summary>
        /// button ' texture
        /// </summary>
        public string imageAssetPath;

        public NameType nameType = NameType.MethodName;
        public string tooltip;
    }
}
