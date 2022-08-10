namespace PowerUtilities.Drawers
{
    using UnityEngine;

    /// <summary>
    /// Show toogle group in inspector
    /// 
    /// [EditorGroup("Fog",true)] public bool _IsGlobalFogOn;
    /// [EditorGroup("Fog")][Range(0, 1)] public float _GlobalFogIntensity = 1;
    /// </summary>
    public class EditorGroupAttribute : PropertyAttribute
    {
        public string groupName;
        public bool isHeader;

        public EditorGroupAttribute(string groupName,bool isHeader=false)
        {
            this.isHeader = isHeader;
            this.groupName=groupName;
        }
    }

}