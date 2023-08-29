namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(LightmapLoader))]
    public class LightmapLoaderEditor : PowerEditor<LightmapLoader>
    {
        public override string Version => "v(0.0.1)";
        
        public override bool NeedDrawDefaultUI() => true;

        public override void DrawInspectorUI(LightmapLoader inst)
        {
            GUILayout.Label("Options");

            if (GUILayout.Button("Load Lightmaps"))
            {
                LightmapLoader.LoadLightmaps(inst.lightmaps, inst.shadowMasks);
            }

            if (GUILayout.Button("Get Scene Lights"))
            {
                var lights = GameObject.FindObjectsOfType<Light>();
                inst.lightBakingInfos = new LightintBakingInfo[lights.Length];
                lights.ForEach((light, id) =>
                {
                    var bakingOutput = light.bakingOutput;
                    inst.lightBakingInfos[id] = new LightintBakingInfo { 
                        light = light 
                        ,lightmapBakeType = bakingOutput.lightmapBakeType
                        ,mixedLightingMode = bakingOutput.mixedLightingMode
                    };
                });
            }

        }
    }

    [CustomPropertyDrawer(typeof(LightintBakingInfo))]
    public class LightingBakingColumnDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var light = property.FindPropertyRelative(nameof(LightintBakingInfo.light));
            var lightmapBakeType = property.FindPropertyRelative(nameof(LightintBakingInfo.lightmapBakeType));
            var mixedLightingMode = property.FindPropertyRelative(nameof(LightintBakingInfo.mixedLightingMode));
            var pos = position;

            EditorGUI.BeginChangeCheck();

            //pos.x = 0;
            pos.width = 40;
            EditorGUI.LabelField(pos, "Light");

            pos.x += pos.width;
            pos.width = 100;
            EditorGUI.PropertyField(pos, light, GUIContent.none);
            
            pos.x += pos.width;
            pos.width = 120;
            EditorGUI.LabelField(pos,"lightmapBakeType");

            pos.x += pos.width;
            pos.width = 100;
            EditorGUI.PropertyField(pos,lightmapBakeType, GUIContent.none);

            pos.x += pos.width;
            pos.width = 120;
            GUI.Label(pos,"mixedLightingMode");

            pos.x += pos.width;
            pos.width = 100;
            EditorGUI.PropertyField(pos, mixedLightingMode, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                var lightObj = light.objectReferenceValue as Light;
                if (lightObj != null)
                {
                    var bakingOutput = lightObj.bakingOutput;
                    bakingOutput.mixedLightingMode = (MixedLightingMode)mixedLightingMode.enumValueIndex;
                    bakingOutput.lightmapBakeType = (LightmapBakeType)lightmapBakeType.enumValueFlag;
                    lightObj.bakingOutput = bakingOutput;

                }
            }
    }
    }

#endif

    [Serializable]
    public class LightintBakingInfo
    {
        public Light light;
        public LightmapBakeType lightmapBakeType = LightmapBakeType.Mixed;
        public MixedLightingMode mixedLightingMode = MixedLightingMode.Shadowmask;
    }

    public class LightmapLoader : MonoBehaviour
    {
        public Texture2D[] lightmaps;
        public Texture2D[] shadowMasks;
        public LightintBakingInfo[] lightBakingInfos;

        public bool isAutoLoad = true;
        // Start is called before the first frame update
        void OnEnable()
        {
            if (isAutoLoad)
            {
                LoadLightmaps(lightmaps, shadowMasks);
                LoadLightBakingInfos(lightBakingInfos);
            }
        }

        public static void LoadLightmaps(Texture2D[] lightmaps, Texture2D[] shadowMasks)
        {
            var lightmapDatas = new LightmapData[lightmaps.Length];
            for (int i = 0; i < lightmaps.Length; i++)
            {
                var data = lightmapDatas[i] = new LightmapData();
                data.lightmapColor = lightmaps[i];
                data.shadowMask = i < shadowMasks.Length ? shadowMasks[i] : null;
            }
            LightmapSettings.lightmaps = lightmapDatas;
        }

        public static void LoadLightBakingInfos(LightintBakingInfo[] infos)
        {
            infos.ForEach(info =>
            {
                if (info.light == null)
                    return;

                var bakingOutput = info.light.bakingOutput;

                bakingOutput.lightmapBakeType = info.lightmapBakeType;
                bakingOutput.mixedLightingMode= info.mixedLightingMode;
                info.light.bakingOutput = bakingOutput;
            });
        }
    }
}