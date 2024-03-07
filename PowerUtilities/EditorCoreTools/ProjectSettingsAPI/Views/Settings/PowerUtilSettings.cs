#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    [ProjectSettingGroup("PowerUtils/PowerUtilSettings")]
    [SOAssetPath("Assets/PowerUtilities/PowerUtilSettings.asset")]
    public class PowerUtilSettings :ScriptableObject
    {
        // -------- lightmap preview
        [Header("LightmapPreview Window")]
        [Tooltip("check this,when need click LightmapPreviewWindow select lightmap object")]
        public bool isCheckLightmapPreviewWin = false;

        // -------- uv2 save
        [Header("Lightmap UV(mesh uv2) Save ")]
        [EditorBorder()]
        [Tooltip("disable ModelImport's GenerateLightmapUVS when save done")]
        public bool isDisableGenerateUV2 = true;

        [Tooltip("write generated lightmapUV to disk")]
        [EditorButton(onClickCall = "SaveUV2",text = "Save Models LightmapUV")]
        public bool isSaveProjectModelsLightmapUV;

        // -------- uv2 restore
        [Header("Lightmap UV(mesh uv2) Restore ")]
        [EditorBorder()]
        [Tooltip("restore generated lightmapUV")]
        [EditorButton(onClickCall = "RestoreUV2", text = "Restore Models LightmapUV")]
        public bool isRestoreProjectmodelsLightmapUV;


        void SaveUV2()
        {
            LightmapUVSaveTools.SaveUV2(isDisableGenerateUV2);
        }

        void RestoreUV2()
        {
            LightmapUVSaveTools.RestoreMesh();
        }
    }
}
#endif