#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using UnityEditor;
    using UnityEngine;
    //[CustomPropertyDrawer(typeof(TagInfo))]
    public class TagInfoDrawer : PropertyDrawer
    {
        void SetPos(ref Rect pos, float offsetX, float w)
        {
            pos.x += offsetX;
            pos.width = w;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var tag = property.FindPropertyRelative("tag");
            var queue = property.FindPropertyRelative("renderQueue");
            var keywords = property.FindPropertyRelative("keywords");

            var pos = position;
            SetPos(ref pos, 0, 120);
            EditorGUI.PropertyField(pos, tag, GUIContent.none);

            SetPos(ref pos, pos.width, 10);
            EditorGUI.LabelField(pos, "q:");

            SetPos(ref pos, pos.width, 50);
            EditorGUI.PropertyField(pos, queue, GUIContent.none);

            SetPos(ref pos, pos.width, 20);
            EditorGUI.LabelField(pos, "kw:");

            SetPos(ref pos, pos.width, position.width - pos.x);
            EditorGUI.PropertyField(pos, keywords, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
    [Serializable]
    public class TagInfo
    {
        /// <summary>
        /// object 's tag
        /// </summary>
        public string tag;

        /// <summary>
        /// object material's renderQueue
        /// </summary>
        public int renderQueue = 2000;

        /// <summary>
        /// material's keywords
        /// </summary>
        [Tooltip("keywords,split by ,")]
        public string keywords="";
        public override string ToString()
        {
            return $"{tag},q:{renderQueue},kw:{keywords.Length}";
        }
    }
}
#endif