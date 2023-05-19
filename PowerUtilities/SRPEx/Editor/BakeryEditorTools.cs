#if UNITY_EDITOR && BAKERY_INCLUDED
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class BakeryEditorTools
    {
        static Dictionary<Light, SerializedObject> lightDict = new Dictionary<Light, SerializedObject>();

        public static Texture2D BakeryDefaultSpotCookieTexture => AssetDatabaseTools.FindAssetsInProject<Texture2D>("ftUnitySpotTexture").FirstOrDefault();

        public static SerializedObject GetBakeryLightSO(Light light,bool dontUseCache=false)
        {
            var isCached = lightDict.TryGetValue(light, out SerializedObject bakeryLightSo);

            if(!isCached || dontUseCache)
            {
                var bakeryLight = light.GetComponents(typeof(MonoBehaviour))
                .Where(mono => mono.GetType().Name.StartsWith("Bakery"))
                .FirstOrDefault();

                if(bakeryLight != null)
                    bakeryLightSo = lightDict[light] = new SerializedObject(bakeryLight);
            }
            
            return bakeryLightSo;
        }

        public static bool SyncBakeryLight(Light light)
        {
            //var bLightSo = GetBakeryLightSO(light);
            var bLightSo = light.GetComponents(typeof(MonoBehaviour))
                .Where(comp => comp.GetType().Name.StartsWith("Bakery"))
                .Select(comp => new SerializedObject(comp))
                .FirstOrDefault()
                ;

            if (bLightSo == null)
                return false;

            bLightSo.UpdateIfRequiredOrScript();
            // update BakeryDirectLight
            bLightSo.FindProperty("color").colorValue = light.color;
            bLightSo.FindProperty("intensity").floatValue = light.intensity;
            bLightSo.FindProperty("indirectIntensity").floatValue = light.bounceIntensity;
            if (RenderPipelineTools.IsHDRenderPipeline())
                bLightSo.FindProperty("intensity").floatValue = light.intensity/Mathf.PI;

            //update BakeryPointLight
            if (light.type == LightType.Point || light.type == LightType.Spot)
            {
                bLightSo.FindProperty("cutoff").floatValue = light.range;
                bLightSo.FindProperty("angle").floatValue = light.spotAngle;

                if (light.type == LightType.Point)
                {
                    //BakeryPointLight.ftLightProjectionMode
                    bLightSo.FindProperty("projMode").enumValueIndex = light.cookie == null ? 0 : 2;//BakeryPointLight.ftLightProjectionMode.Omni : BakeryPointLight.ftLightProjectionMode.Cubemap;
                    bLightSo.FindProperty("cubemap").objectReferenceValue =  light.cookie as Cubemap;
                }
                else if (light.type == LightType.Spot)
                {
                    bLightSo.FindProperty("projMode").enumValueIndex = 1;//BakeryPointLight.ftLightProjectionMode.Cookie
                    bLightSo.FindProperty("cookie").objectReferenceValue = light.cookie == null ? BakeryDefaultSpotCookieTexture : (Texture2D)light.cookie;
                }

                if (RenderPipelineTools.IsHDRenderPipeline() || RenderPipelineTools.IsUniversalPipeline())
                {
                    bLightSo.FindProperty("realisticFalloff").boolValue = true;
                    bLightSo.FindProperty("falloffMinRadius").floatValue = 0.01f;
                    bLightSo.FindProperty("projMode").enumValueIndex = 4;//BakeryPointLight.ftLightProjectionMode.Cone;
                    bLightSo.FindProperty("innerAngle").floatValue = light.innerSpotAngle;
                }
            }

            bLightSo.ApplyModifiedProperties();

            return true;
        }
    }
}
#endif