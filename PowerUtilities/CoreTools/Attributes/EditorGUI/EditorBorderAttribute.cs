namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// A decorator ,draw box color in editor
    /// 
    /// editor : EditorBorderAttributeDecorator
    /// </summary>
    public class EditorBorderAttribute : PropertyAttribute
    {
        public int lineCount = 1;
        public Color color = default;

        public EditorBorderAttribute(int lineCount = 1, string colorStr = "#749C75")
        {
            this.lineCount = lineCount;
            ColorUtility.TryParseHtmlString(colorStr, out color);
        }
    }
}
