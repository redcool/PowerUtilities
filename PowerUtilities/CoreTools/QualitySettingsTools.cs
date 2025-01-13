using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

    }
}
