namespace PowerUtilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Show toogle group in inspector gui( fields )
    /// 
    /// [EditorGroup("Fog",true)] public bool _IsGlobalFogOn;
    /// [EditorGroup("Fog")][Range(0, 1)] public float _GlobalFogIntensity = 1;
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Field)]
    public class EditorGroupAttribute : PropertyAttribute
    {
        public string groupName;
        public bool isHeader;
        public Color titleColor;
        public int intentOffset = 1;
        public string tooltip;

        public EditorGroupAttribute(string groupName, bool isHeader = false, string titleColorStr = "#404E7C")
        {
            this.isHeader = isHeader;
            this.groupName=groupName;
            ColorUtility.TryParseHtmlString(titleColorStr, out titleColor);
        }
    }

}