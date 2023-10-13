namespace PowerUtilities
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A decorator ,show toogle group in inspector gui( fields )
    /// 
    /// [EditorGroup("Fog",true)] public bool _IsGlobalFogOn;
    /// [EditorGroup("Fog")][Range(0, 1)] public float _GlobalFogIntensity = 1;
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Field)]
    public class EditorGroupAttribute : PropertyAttribute
    {
        public string groupName;
        public Color titleColor;

        public EditorGroupAttribute(string groupName, string titleColorStr = "#404E7C")
        {
            this.groupName=groupName;
            ColorUtility.TryParseHtmlString(titleColorStr,out titleColor);
            //titleColor *=0.7f;
        }
    }

}