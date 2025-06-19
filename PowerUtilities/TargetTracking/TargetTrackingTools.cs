using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class TargetTrackingTools
    {
        public static void FindChildObj(Transform parent, string childName, ref Transform childTr, ref Vector3 childPos)
        {
            if (!childTr)
                childTr = parent.Find(childName);

            if (childTr)
                childPos = childTr.position;
        }

        public static void TrySetupRT(RenderTextureDescriptor desc, string trackTextureName, ref RenderTexture trackRT)
        {
            desc.sRGB = false;
            desc.enableRandomWrite = true;
            RenderTextureTools.TryCreateRT(ref trackRT, desc, trackTextureName, FilterMode.Bilinear);
        }

        public static void UpdateBoxSceneFogMaterial(Material mat, Vector3 minPos, Vector3 maxPos, Texture trackRT)
        {
            if (!mat)
                return;
            mat.SetVector("_MinWorldPos", minPos);
            mat.SetVector("_MaxWorldPos", maxPos);
            mat.SetTexture("_SceneFogMap", trackRT);
        }

        public static void UpdateBoxSceneFogMaterial(Renderer boxSceneFogRender, ref Material boxSceneFogMat, Vector3 minPos, Vector3 maxPos, RenderTexture trackRT)
        {
            if (!boxSceneFogRender)
                return;

            var mat = boxSceneFogMat = Application.isPlaying ? boxSceneFogRender.material : boxSceneFogRender.sharedMaterial;
            UpdateBoxSceneFogMaterial(mat, minPos, maxPos, trackRT);
        }

        public static void FindBorderObjectsInChildren(Transform parentTR,
            string minPosTrName, ref Transform minPosTr, ref Vector3 minPos,
            string maxPosTrName, ref Transform maxPosTr, ref Vector3 maxPos)
        {
            if (parentTR.childCount < 2)
                return;

            FindChildObj(parentTR, minPosTrName, ref minPosTr, ref minPos);
            FindChildObj(parentTR, maxPosTrName, ref maxPosTr, ref maxPos);
        }
    }
}
