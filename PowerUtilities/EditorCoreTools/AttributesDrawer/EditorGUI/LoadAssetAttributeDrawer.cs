#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [CustomPropertyDrawer(typeof(LoadAssetAttribute))]
    public class LoadAssetAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TryLoadAsset(property);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        private void TryLoadAsset(SerializedProperty prop)
        {
            if (prop.objectReferenceValue)
                return;

            var attr = attribute as LoadAssetAttribute;

            var pathOrName = attr.assetPathOrName;

            if (!pathOrName.StartsWith("Assets"))
            {
                var name = Path.GetFileNameWithoutExtension(pathOrName);
                var extName = Path.GetExtension(pathOrName);

                AssetDatabaseTools.FindAssetPathAndLoad<Object>(out pathOrName, name, extName, true);
            }
            prop.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(pathOrName);
        }
    }
}
#endif