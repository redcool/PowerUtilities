#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Show Material's Global Illumation options( bake emission need this flags)
    /// 
    /// [MaterialGI(Emission)]_EmissionGI("_EmissionGI",int) = 0
    /// </summary>
    public class GroupMaterialGIDrawer : BaseGroupItemDrawer
    {
        public GroupMaterialGIDrawer(string groupName) : this(groupName, "") { }
        public GroupMaterialGIDrawer(string groupName, string tooltip) : base(groupName, tooltip) { }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            position = EditorGUI.IndentedRect(position);

            EditorGUI.BeginChangeCheck();
            editor.LightmapEmissionProperty();

            if (EditorGUI.EndChangeCheck())
            {
                foreach (Material mat in editor.targets)
                {
                    mat.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                }
            }
            
        }

    }

}
#endif