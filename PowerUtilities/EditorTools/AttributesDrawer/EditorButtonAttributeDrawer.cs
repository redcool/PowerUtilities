#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using static EditorButtonAttribute;

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
                    property.UpdatePropertyValue(inst =>
                    {
                        var instType = inst.GetType();
                        ReflectionTools.InvokeMember(instType, attr.onClickCall, inst, null);
                    });
                }
            }
        }
    }
}
#endif