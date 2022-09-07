#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.SocialPlatforms;

namespace PowerUtilities
{
    /// <summary>
    /// Material Tooltips
    /// 
    ///  [Tooltip(show float value)]
    ///  _FloatVlaue("_FloatVlaue0", range(0,1)) = 0.1
    /// </summary>
    public class MaterialTooltipDecorator : MaterialPropertyDrawer
    {
        public string tooltip;
        public MaterialTooltipDecorator(string tooltip)
        {
            this.tooltip = tooltip;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if(!string.IsNullOrEmpty(tooltip))
                label.tooltip= tooltip;

        }
    }
}
#endif