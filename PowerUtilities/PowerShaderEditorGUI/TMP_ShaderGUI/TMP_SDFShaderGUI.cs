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
    /// <summary>
    /// Show in * States
    /// 
    /// Edit in ProjectSettings/PowerUtils/TMP Shader/TMPShaderPropertiesSetting
    /// </summary>
    public class TMP_SDFShaderGUI : TMPro.EditorUtilities.TMP_SDFShaderGUI
    {
        bool isStateOn;

        //GUIContent[] contents = new[]
        //{
        //    new GUIContent("_PresetBlendMode"),
        //    new GUIContent("_ZWriteMode"),
        //    new GUIContent("_ZTestMode"),
        //    new GUIContent("_CullMode"),
        //    new GUIContent("_GrayOn"),
        //};

        protected override void DoGUI()
        {
            base.DoGUI();

            var settings = ScriptableObjectTools.CreateGetInstance<TMPShaderPropertiesSetting>();

            isStateOn = BeginPanel("* States", isStateOn);
            if (isStateOn)
                DrawStates(settings.stateContents);

            EndPanel();
        }

        private void DrawStates(params GUIContent[] contents)
        {
            foreach (GUIContent content in contents)
            {
                if (!m_Material.HasProperty(content.text))
                    continue;

                var prop = FindProperty(content.text, m_Properties);
                m_Editor.ShaderProperty(prop, content);

            }
        }
    }
}
#endif