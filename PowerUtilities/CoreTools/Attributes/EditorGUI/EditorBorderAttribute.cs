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
        /// left 
        /// </summary>
        public Color color = default;

        public Color right = default, top = default, bottom = default;
        /// <summary>
        /// line pixel size
        /// </summary>
        public int borderSize = 2;
        /// <summary>
        /// unity editor GetHeight use 
        /// </summary>
        public int lineHeight = 18;

        public EditorBorderAttribute(int lineCount, string leftColorStr = default, string topColorStr = default, string rightColorStr = default, string bottomColorStr = default)
        {
            this.lineRowCount = lineCount;
            ColorTools.SetupColor(leftColorStr, ref color);
            ColorTools.SetupColor(rightColorStr, ref right);
            ColorTools.SetupColor(topColorStr, ref top);
            ColorTools.SetupColor(bottomColorStr, ref bottom);
        }

        public EditorBorderAttribute(int lineCount = 1)
        {
            this.lineRowCount = lineCount;
            ColorTools.SetupColor(ColorTools.LIGHT_GREEN, ref color);
        }


    }
}
