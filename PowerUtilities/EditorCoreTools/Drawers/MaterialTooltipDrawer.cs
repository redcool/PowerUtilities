#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace PowerUtilities
{
    public class MaterialTooltipDrawer : MaterialPropertyDrawer
    {
        public string tooltip;
        public MaterialTooltipDrawer(string tooltip)
        {
            this.tooltip = tooltip;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            label.tooltip= tooltip;
            MaterialEditorEx.ShaderProperty(editor,position, prop, label, 0, false);
        }
    }
}
#endif