#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    public static class BakeryEditorTools
    {
        static Dictionary<Light, SerializedObject> lightDict = new Dictionary<Light, SerializedObject>();

        public static Texture BakeryDefaultSpotCookieTexture => TextureCacheTool.GetTexture("ftUnitySpotTexture"
            , () => AssetDatabaseTools.FindAssetsInProject<Texture>("ftUnitySpotTexture").FirstOrDefault()
            );

        //public static Texture2D BakeryDefaultSpotCookieTexture => AssetDatabaseTools.FindAssetsInProject<Texture2D>("ftUnitySpotTexture").FirstOrDefault();

        public static bool TryGetBakeryLightSO(Light light, out SerializedObject bakeryLightSo, bool dontUseCache = false)
        {
            var isCached = lightDict.TryGetValue(light, out bakeryLightSo);

            if (dontUseCache || !isCached || bakeryLightSo==null)
            {
                bakeryLightSo = light.GetComponents(typeof(MonoBehaviour))
                .Where(mono => mono && mono.GetType().Name.StartsWith("Bakery"))
                .Select(comp => new SerializedObject(comp))
                .FirstOrDefault();

                lightDict[light] = bakeryLightSo;
            }

            return bakeryLightSo == null;
        }

        public static Type GetBakeryLightType(string name)
        {
            return TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
                .Where(t => t.Name== (name))
                .FirstOrDefault();
        }

        public static void AddBakeryLight(Light light)
        {
            Type bakeryLightType = null;
            if (light.type == LightType.Directional)
            {
                bakeryLightType = GetBakeryLightType("BakeryDirectLight");
            }
            else if (light.type == LightType.Spot ||  light.type == LightType.Point)
            {
                bakeryLightType = GetBakeryLightType("BakeryPointLight");
            }
            if (bakeryLightType != null)
                light.gameObject.GetOrAddComponent(bakeryLightType);
        }

        public static void RemoveBakeryLight(Light light)
        {
            var t1 = light.GetComponent("BakeryDirectLight");
            if (t1 != null)
                Object.DestroyImmediate(t1);

            var t2 = light.GetComponent("BakeryPointLight");
            if (t2 != null)
                Object.DestroyImmediate(t2);
        }

        public static bool SyncBakeryLight(Light light)
        {
            TryGetBakeryLightSO(light, out var bLightSo,true);
            return SyncBakeryLight(light,bLightSo);
        }

        public static bool SyncBakeryLight(Light light,SerializedObject bLightSo)
        {
            if (!light || bLightSo == null || !bLightSo.targetObject)
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