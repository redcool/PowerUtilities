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

        // -------- uv2 save
        [Header("Lightmap UV(mesh uv2) Save ")]
        [EditorBorder()]
        [Tooltip("disable ModelImport's GenerateLightmapUVS when save done")]
        public bool isDisableGenerateUV2 = true;

        [Tooltip("write generated lightmapUV to disk, \n select nothing will use assets Folder")]
        [EditorButton(onClickCall = "SaveUV2", text = "Save Models LightmapUV")]
        public bool isSaveProjectModelsLightmapUV;

        // -------- uv2 restore
        [Header("Lightmap UV(mesh uv2) Restore ")]
        [EditorBorder()]
        [Tooltip("restore generated lightmapUV, \n select nothing will use assets Folder")]
        [EditorButton(onClickCall = "RestoreUV2", text = "Restore Models LightmapUV")]
        public bool isRestoreProjectmodelsLightmapUV;

        [Header("Clear Lightmap UV info  ")]
        [EditorBorder()]
        [Tooltip("clear generated lightmapUV objects, \n select nothing will use assets Folder")]
        [EditorButton(onClickCall = "ClearModelExtendInfos", text = "Clear LightmapUV objects")]
        public bool isClearModelExtendInfos;


        void SaveUV2()
        {
            LightmapUVSaveTools.SaveUV2(isDisableGenerateUV2);
        }

        void RestoreUV2()
        {
            LightmapUVSaveTools.RestoreMesh();
        }

        void ClearModelExtendInfos()
        {
            LightmapUVSaveTools.ClearModelExtendInfos();
        }
    }
}
#endif