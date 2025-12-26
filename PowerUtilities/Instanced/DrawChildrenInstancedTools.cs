using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class DrawChildrenInstancedTools
    {
        /// <summary>
        /// SetupDrawInfo lightmaps and shadow masks
        /// </summary>
        /// <param name="lightmaps"></param>
        /// <param name="shadowMasks"></param>
        public static void SetupLightmaps(ref Texture2D[] lightmaps, ref Texture2D[] shadowMasks)
        {
            lightmaps = new Texture2D[LightmapSettings.lightmaps.Length];
            shadowMasks = new Texture2D[LightmapSettings.lightmaps.Length];

            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                lightmaps[i] = LightmapSettings.lightmaps[i].lightmapColor;
                shadowMasks[i] = LightmapSettings.lightmaps[i].shadowMask;
            }
        }


        public static bool IsLightmapValid(int lightmapId, Texture[] lightmaps)
            => lightmapId > -1 && lightmapId < lightmaps.Length && lightmaps[lightmapId];

        public static void UpdateLightmap(Material mat, MaterialPropertyBlock block, int lightmapId, Texture2D[] lightmaps, Texture2D[] shadowMasks, Vector4[] lightmapCoords, bool isSubstractiveMode)
        {
            // lightmap_ST
            if (lightmapCoords.Length > 0)
            {
                //block.SetVectorArray("_LightmapST", lightmapGroup.lightmapCoords);
                //block.SetInt("_DrawInstanced", 1);
                block.SetVectorArray("unity_LightmapST", lightmapCoords);
            }
            // for substractive mode
            if (isSubstractiveMode)
                block.SetVector("unity_LightData", Vector4.zero);

            mat.DisableKeyword("UNITY_INSTANCED_LIGHTMAPSTS");
            if (IsLightmapValid(lightmapId, shadowMasks))
            {
                block.SetTexture("unity_ShadowMask", shadowMasks[lightmapId]);
            }

            if (IsLightmapValid(lightmapId, lightmaps))
            {
                block.SetTexture("unity_Lightmap", lightmaps[lightmapId]);
            }

        }
        public static void UpdateLightmapArray(Material mat,MaterialPropertyBlock block, Texture2DArray lightmapArray, List<float> lightmapIdList)
        {
            mat.EnableKeyword("UNITY_INSTANCED_LIGHTMAPSTS");
            block.SetTexture("unity_Lightmaps", lightmapArray != null ? (Texture)lightmapArray : Texture2D.blackTexture);
            block.SetFloatArray("unity_LightmapIndexArray", lightmapIdList);
        }


        public static ShadowCastingMode GetShadowCastingMode(ShadowCastingMode shadowCastMode)
        {
            return (QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask && shadowCastMode != ShadowCastingMode.Off)
                ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        public static void LightmapSwitch(Material mat, bool lightmapEnable)
        {
            if (mat.IsKeywordEnabled("LIGHTMAP_ON") != lightmapEnable)
                mat.SetKeyword("LIGHTMAP_ON", lightmapEnable);
        }
    }
}