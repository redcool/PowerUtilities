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
        /// <summary>
        /// line row count,line position.
        /// pos.height = EditorGUIUtility.singleLineHeight * attr.lineRowCount;
        /// </summary>
        public int lineRowCount = 1;
        /// <summary>
        /// edge colors (left,right,top,bottom)
        /// </summary>
        public Color leftColor = default, rightColor = default, topColor = default, bottomColor = default;
        /// <summary>
        /// line pixel size
        /// </summary>
        public int borderSize = 2;
        /// <summary>
        /// unity editor GetHeight use, default 18
        /// </summary>
        public int lineHeight = 0;
        /// <summary>
        /// check groupName is on or off
        /// </summary>
        public string groupName;
        /// <summary>
        /// Rect.y offset
        /// </summary>
        public float posYOffset = 0;
        public float posXOffset = -2;
        /// <summary>
        /// indent current rect
        /// </summary>
        public bool isIndentRect = true;

        public EditorBorderAttribute(int lineCount, string leftColorStr = default, string topColorStr = default, string rightColorStr = default, string bottomColorStr = default)
        {
            this.lineRowCount = lineCount;
            ColorTools.SetupColor(leftColorStr, ref leftColor);
            ColorTools.SetupColor(rightColorStr, ref rightColor);
            ColorTools.SetupColor(topColorStr, ref topColor);
            ColorTools.SetupColor(bottomColorStr, ref bottomColor);
        }

        public EditorBorderAttribute(int lineCount = 1)
        {
            this.lineRowCount = lineCount;
            ColorTools.SetupColor(ColorTools.G_LIGHT_GREEN, ref leftColor);
        }


    }
}
