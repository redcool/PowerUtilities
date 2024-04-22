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
    [CustomPropertyDrawer(typeof(EditorToolbarAttribute))]
    public class EditorToolbatAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //property.serializedObject.Update();
            var attr = attribute as EditorToolbarAttribute;

            var contents = new List<GUIContent>();
            for (int i = 0; i < attr.texts.Length; ++i)
            {
                var text = attr.texts[i];
                var imagePath = (attr.imageAssetPaths != null && attr.imageAssetPaths.Length> i )?attr.imageAssetPaths[i] : "";

                if (!string.IsNullOrEmpty(imagePath))
                    label.image = AssetDatabase.LoadAssetAtPath<Texture>(imagePath);
                label.text = text;
                contents.Add(label);
            }

            var texts = attr.texts.Where(t => !string.IsNullOrEmpty(t));
            position = EditorGUI.IndentedRect(position);

            var newId = GUI.Toolbar(position, property.intValue, contents.ToArray());
            if(newId != property.intValue)
            {
                property.intValue = newId;
                CallTargetMethold(property, attr, newId);
            }
            //property.serializedObject.ApplyModifiedPropertiesWithoutUndo();


            //================ inner methods
            static void CallTargetMethold(SerializedProperty property, EditorToolbarAttribute attr,int buttonId)
            {
                if (!string.IsNullOrEmpty(attr.onClickCall))
                {
                    var inst = property.serializedObject.targetObject;
                    var instType = inst.GetType();
                    ReflectionTools.InvokeMember(instType, attr.onClickCall, inst, new object[] { buttonId });
                }
            }
        }
    }
#endif
    /// <summary>
    /// show GUI.Toolbat for int 
    /// 
    /// like: 
    ///     [EditorToolbar(onClickCall = "Test", texts = new[] { "a", "b" })]
    ///     public int test;
    ///     
    ///     Test(int buttonId){}
    /// </summary>
    public class EditorToolbarAttribute : PropertyAttribute
    {
        /// <summary>
        /// Func(int buttonId)
        /// </summary>
        public string onClickCall;
        public string[] texts;
        public string[] imageAssetPaths;
    }
}
