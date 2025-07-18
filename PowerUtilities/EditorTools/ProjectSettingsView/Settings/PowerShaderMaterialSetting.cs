using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PowerUtilities
{

#if UNITY_EDITOR
    [CustomEditor(typeof(PowerShaderMaterialSetting))]
    public class PowerShaderMaterialSettingEditor : PowerEditor<PowerShaderMaterialSetting>
    {
        public override bool NeedDrawDefaultUI() => true;

        public override void DrawInspectorUI(PowerShaderMaterialSetting inst)
        {
            EditorGUITools.BeginHorizontalBox(() =>
            {
                // 1
                if (GUILayout.Button("Sync Material keywords"))
                {
                    foreach (var shaderObj in inst.shaders)
                    {
                        var mats = shaderObj.GetMaterialsRefShader();
                        var result = shaderObj.SyncMaterialKeywords(inst.toggleTypeString, mats);
                        Debug.Log(result);

                    }
                }
                
                // 2
                if (EditorGUITools.ButtonWithConfirm("Clear Materials Keywords", "remove all material keywords"))
                {
                    inst.shaders.ForEach(shader =>
                    {
                        shader.ClearMaterialKeywords();
                    });
                }
            }, EditorStyles.helpBox.ToString());
        }
    }
#endif

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/PowerShaderMaterialSetting", isUseUIElment = false)]
    [SOAssetPath("Assets/PowerUtilities/PowerShaderMaterialSetting.asset")]
    public class PowerShaderMaterialSetting : ScriptableObject
    {
        [HelpBox]
        public string helpBox = "Sync material keywords and uniform variable, for GroupAPI,like [GroupToggle()]";
        [Tooltip("Check shaders")]
        public Shader[] shaders;

        [Tooltip("Material attribute that include keyword ")]
        public string toggleTypeString = "GroupToggle";
    }
}
