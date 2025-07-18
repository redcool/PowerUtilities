#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Group Item ui
    /// 
    /// xxxDrawer : only call drawer,
    /// xxxDecorator : call draw decorator first, then call property drawer 
    /// </summary>
    public abstract class BaseGroupItemDrawer : MaterialPropertyDrawer
    {
        public const char ITEM_SPLITTER = ' ';
        public static float MIN_LINE_HEIGHT = -2;
        public static float LINE_HEIGHT = EditorGUIUtility.singleLineHeight;

        string tooltip;

        public string groupName;
        public BaseGroupItemDrawer(string groupName,string tooltip)
        {
            this.groupName = groupName;
            this.tooltip = tooltip;
        }

        public bool IsGroupOn()
            => MaterialGroupTools.IsGroupOn(groupName);
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (IsGroupOn())
            {
                var baseHeight = MaterialEditor.GetDefaultPropertyHeight(prop);
                return baseHeight;
            }
            
            return MIN_LINE_HEIGHT;
        }


        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!IsGroupOn())
                return;

            var mat = (editor.target as Material);
            var isDisabled = MaterialGroupTools.IsGroupDisabled(groupName);

            //check MaterialDisableGroupDecorator
            isDisabled = isDisabled || MaterialDisableGroupDecorator.IsPropertyDisabled(editor, prop.name);

            EditorGUI.BeginDisabledGroup(isDisabled);
            {
                if (!string.IsNullOrEmpty(tooltip))
                    label.tooltip = tooltip;

                var lastLabelWidth = EditorGUIUtility.labelWidth;
                
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.4f ;

                EditorGUI.indentLevel += MaterialGroupTools.GroupIndentLevel(groupName);
                // ui
                DrawGroupUI(position, prop, label, editor);

                // box colors
                var pos = position;
                pos.height += 2;
                EditorGUITools.DrawBoxColors(pos, 2, MaterialGroupTools.GetBackgroundColor(groupName), MaterialGroupTools.GetColumnColor(groupName));

                EditorGUI.indentLevel -= MaterialGroupTools.GroupIndentLevel(groupName);
                EditorGUIUtility.labelWidth = lastLabelWidth;
            }
            EditorGUI.EndDisabledGroup();
        }

        public abstract void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor);
    }
}
#endif