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
        /// <summary>
        /// left 
        /// </summary>
        public Color color = default;

        public Color right = default,top = default,bottom = default;
        public int lineHeight = 2;

        public EditorBorderAttribute(int lineCount = 1, string colorStr = "#749C75", string topColorStr = default,string rightColorStr= default,string bottomColorStr= default)
        {
            this.lineCount = lineCount;
            SetupColor(colorStr, ref color);
            SetupColor(rightColorStr,ref right);
            SetupColor(topColorStr, ref top);
            SetupColor(bottomColorStr, ref bottom);
        }

        private void SetupColor(string str,ref Color c)
        {
            if (!string.IsNullOrEmpty(str))
                ColorUtility.TryParseHtmlString(str, out c);
        }
    }
}
