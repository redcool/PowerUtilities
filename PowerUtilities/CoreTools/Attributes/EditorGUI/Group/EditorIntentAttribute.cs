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
    [CustomPropertyDrawer(typeof(EditorIntentAttribute))]
    public class EditorIntentDecorator : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return 0;
        }
        public override void OnGUI(Rect position)
        {
            var attr = attribute as EditorIntentAttribute;
            EditorGUI.indentLevel += attr.intentLevelOffset;
        }
    }
#endif

    public class EditorIntentAttribute : PropertyAttribute
    {
        public int intentLevelOffset = 1;

        public EditorIntentAttribute(int intentOffset)
        {
            intentLevelOffset = intentOffset;
        }
    }
}
