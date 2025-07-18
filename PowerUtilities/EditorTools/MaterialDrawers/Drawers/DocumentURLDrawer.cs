#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    /// <summary>
    /// Material's Group Item Attribute
    /// </summary>
    public class DocumentURLDrawer : MaterialPropertyDrawer
    {
        private float m_VersionNumber;
        
        public DocumentURLDrawer(float versionNumber)
        {
            m_VersionNumber = versionNumber;
        }
        
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.LabelField(position, $"Version: {m_VersionNumber}    {MaterialPropertyI18NSO.Text("DocumentURLDesc")}");
            
            GUIContent gc = EditorGUIUtility.IconContent("_Help");
            position.x = EditorGUIUtility.currentViewWidth - 40;
            EditorGUI.LabelField(position, gc);
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (position.Contains(e.mousePosition))
                {
                    Application.OpenURL(prop.displayName);
                }
            }
        }
    }
}
#endif