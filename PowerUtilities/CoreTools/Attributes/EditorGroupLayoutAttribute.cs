namespace PowerUtilities
{
    using UnityEngine;

    /// <summary>
    /// Show toggle group in inspector gui 
    /// ** (GUILaout, CustomEditor) can use this attribute
    /// 
    /// ** GUI(EditorGUI) should use EditorGroup
    /// 
    /// [EditorGroup("Fog",true)] public bool _IsGlobalFogOn;
    /// [EditorGroup("Fog")][Range(0, 1)] public float _GlobalFogIntensity = 1;
    /// </summary>
    public class EditorGroupLayoutAttribute : PropertyAttribute
    {
        public string groupName;
        public bool isHeader;

        public EditorGroupLayoutAttribute(string groupName, bool isHeader = false)
        {
            this.isHeader = isHeader;
            this.groupName=groupName;
        }
    }

}