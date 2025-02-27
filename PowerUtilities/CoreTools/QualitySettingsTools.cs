using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class QualitySettingsTools
    {
        static LODGroup[] sceneLodGroups;

        public static void FindSetSceneMeshLod(int maxMeshLod,bool isForceFind=false)
        {
            if(sceneLodGroups == null || sceneLodGroups.Length == 0 || isForceFind)
                sceneLodGroups = GameObject.FindObjectsByType<LODGroup>(FindObjectsSortMode.None);

            foreach (LODGroup lod in sceneLodGroups)
                lod.ForceLOD(maxMeshLod);
        }

        public static void ResetSceneMeshLod(int maxMeshLod = 0)
        {
            if (sceneLodGroups == null)
                return;

            foreach (LODGroup lod in sceneLodGroups)
                lod.ForceLOD(maxMeshLod);
        }

        /// <summary>
        /// when action done, restore qualitySettings.level
        /// </summary>
        /// <param name="action"></param>
        public static void SetQualityContent(Action action)
        {
            if (action == null)
                return;

            var lastLevel = QualitySettings.GetQualityLevel();
            action?.Invoke();

            QualitySettings.SetQualityLevel(lastLevel);
        }

        /// <summary>
        /// Set target pipelineAsset
        /// </summary>
        /// <param name="qualityLevel"></param>
        /// <param name="pipelineAsset"></param>
        public static void SetRenderPipelineAssetAt(int qualityLevel, RenderPipelineAsset pipelineAsset)
        {
            SetQualityContent(() =>
            {
                QualitySettings.SetQualityLevel(qualityLevel);
                QualitySettings.renderPipeline = pipelineAsset;
            });

        }
        public static void CleanAllRenderPipelineAsset()
        {

            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                SetQualityContent(() =>
                {
                    QualitySettings.SetQualityLevel(i);
                    QualitySettings.renderPipeline = null;
                });
            }
        }
    }
}
