using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public static class GUIContentEx
    {
        static GUIContent tempContent = new GUIContent();

        /// <summary>
        /// get Temp guicontent
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tooltip"></param>
        /// <param name="image"></param>
        /// <param name="inst"></param>
        /// <returns></returns>
        public static GUIContent TempContent(string str, string tooltip = default, Texture image = default, GUIContent inst = default)
        {
            if (inst == default)
                inst = tempContent;

            inst.text = str;
            inst.tooltip = tooltip;
            inst.image = image;

            return inst;
        }

        public static GUIContent Set(this GUIContent content,string label,string tooltip,Texture image)
        {
            content.text = label;
            content.tooltip = tooltip;
            content.image = image;
            return content;
        }
    }
}
