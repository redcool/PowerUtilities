#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public class TMP_SDFShaderGUI : TMPro.EditorUtilities.TMP_SDFShaderGUI
    {
        bool isStateOn;

        GUIContent[] stateNames = new[]
        {
            new GUIContent("_PresetBlendMode"),
            new GUIContent("_ZWriteMode"),
            new GUIContent("_ZTestMode"),
            new GUIContent("_CullMode"),
            new GUIContent("_GrayOn"),
        };

        protected override void DoGUI()
        {
            base.DoGUI();

            isStateOn = BeginPanel("* States", isStateOn);
            if (isStateOn)
                DrawStates(stateNames);

            EndPanel();
        }

        private void DrawStates(params GUIContent[] stateNames)
        {
            foreach (GUIContent stateName in stateNames)
            {
                var prop = FindProperty(stateName.text, m_Properties);
                m_Editor.ShaderProperty(prop, stateName);

            }
        }
    }
}
#endif