#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(PowerUtilSettings))]
    public class PowerUtilSettingsEditor : Editor
    {
    }

    [ProjectSettingGroup("PowerUtils/PowerUtilSettings")]
    [SOAssetPath("Assets/PowerUtilities/PowerUtilSettings.asset")]
    public class PowerUtilSettings : ScriptableObject
    {
        // -------- lightmap preview
        [Header("LightmapPreview Window")]
        [Tooltip("check this,when need click LightmapPreviewWindow select lightmap object")]
        public bool isCheckLightmapPreviewWin = false;
        [Tooltip("lightmapPreviewWin ShowLog")]
        public bool lightmapPreviewWinShowLog;

        // -------- uv2 save
        [Header("Lightmap UV(mesh uv2) Save ")]
        [EditorBorder()]
        [Tooltip("disable ModelImport's GenerateLightmapUVS when save done")]
        public bool isDisableGenerateUV2 = true;

        [Tooltip("write generated lightmapUV to disk, \n select nothing will use assets Folder")]
        [EditorButton(onClickCall = "ExportModels", text = "Export fbx with uv2")]
        public bool isExportModels;


        void ExportModels()
        {
            FBXExportTools.ExportModels(isDisableGenerateUV2);
        }

    }
}
#endif