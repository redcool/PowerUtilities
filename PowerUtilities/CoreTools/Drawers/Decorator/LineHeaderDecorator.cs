#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEditor;
    using UnityEngine;

    public class LineHeaderDecorator : MaterialPropertyDrawer
    {
        string header;

        public LineHeaderDecorator(string header)
        {
            this.header = $"---------------- {header} --------------------------------";
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position.y += 8;
            position = EditorGUI.IndentedRect(position);
            EditorGUI.DropShadowLabel(position, header, EditorStyles.boldLabel);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 24;
        }
    }
}
#endif