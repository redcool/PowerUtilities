#if UNITY_EDITOR && BAKERY_INCLUDED
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class BakeryEditorTools
    {

        public static Texture2D BakeryDefaultSpotCookieTexture => AssetDatabase.LoadAssetAtPath<Texture2D>(ftLightmaps.GetRuntimePath());

        public static bool SyncBakeryLight(Light light)
        {
            var bLight = light.GetComponent<BakeryDirectLight>();
            if (bLight)
            {
                bLight.color = light.color;
                bLight.intensity = light.intensity;
                bLight.indirectIntensity = light.bounceIntensity;
                if (RenderPipelineTools.IsHDRenderPipeline())
                    bLight.intensity /= Mathf.PI;
            }

            var pLight = light.GetComponent<BakeryPointLight>();
            if (pLight)
            {
                pLight.color = light.color;
                pLight.intensity = light.intensity;
                pLight.indirectIntensity = light.bounceIntensity;

                pLight.cutoff = light.range;
                pLight.angle = light.spotAngle;

                if (light.type == LightType.Point)
                {
                    pLight.projMode = light.cookie == null ? BakeryPointLight.ftLightProjectionMode.Omni : BakeryPointLight.ftLightProjectionMode.Cubemap;
                    pLight.cubemap = light.cookie as Cubemap;

                }
                else if (light.type == LightType.Spot)
                {
                    pLight.projMode = BakeryPointLight.ftLightProjectionMode.Cookie;
                    pLight.cookie = light.cookie == null ? BakeryDefaultSpotCookieTexture : (Texture2D)light.cookie;
                }

                if (RenderPipelineTools.IsHDRenderPipeline() || RenderPipelineTools.IsUniversalPipeline())
                {
                    pLight.realisticFalloff = true;
                    pLight.falloffMinRadius = 0.01f;
                    pLight.projMode = BakeryPointLight.ftLightProjectionMode.Cone;
                    pLight.innerAngle = light.innerSpotAngle;
                }

                if (RenderPipelineTools.IsHDRenderPipeline())
                {
                    pLight.intensity /= Mathf.PI;
                }
            }

            return bLight || pLight;
        }
    }
}
#endif