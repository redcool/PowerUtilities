#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    [CanEditMultipleObjects]
    // open some ui for convenient
    [CustomEditor(typeof(UniversalAdditionalLightData))]
    class UniversalAdditionalLightDataEditorEx : Editor
    {
        (string, bool) bakingOutputFoldInfo = ("BakingOutputSettings", false);

        readonly GUIContent BakingOutputContent = new GUIContent("Output", "setup light baking output");

        GUIContent showDefaultSettings = new GUIContent("ShowDefaultSettings","Show all AdditionalLightData paramerters");
        bool isFoldShowDefaultSettings;

        Object scriptObj;
        private void OnEnable()
        {
            scriptObj = AssetDatabaseTools.FindAssetPathAndLoad<Object>(out _, GetType().Name);
        }
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField(GUIContentEx.TempContent("Script : ", "Script draw gui "), scriptObj, typeof(Object), false);
            }

            EditorGUI.indentLevel++;
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUITools.DrawFoldContent(showDefaultSettings,ref isFoldShowDefaultSettings, () =>
            {
                DrawDefaultInspector();
            });

            EditorGUITools.DrawColorLine(1);
            DrawLightBakingSettings();

            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
        }

        private void DrawLightBakingSettings()
        {
            EditorGUITools.DrawFoldContent(ref bakingOutputFoldInfo, () =>
            {

                var data = target as UniversalAdditionalLightData;
                var light = data.GetComponent<Light>();

                var output = light.bakingOutput;

                //lightmapBakeType
                EditorGUITools.BeginHorizontalBox(() =>
                {
                    EditorGUILayout.LabelField(nameof(output.lightmapBakeType));
                    output.lightmapBakeType = (LightmapBakeType)EditorGUILayout.EnumPopup(output.lightmapBakeType);
                });
                //mixedLightingMode
                EditorGUITools.BeginHorizontalBox(() =>
                {
                    EditorGUILayout.LabelField(nameof(output.mixedLightingMode));
                    output.mixedLightingMode = (MixedLightingMode)EditorGUILayout.EnumPopup(output.mixedLightingMode);

                });


                //probeOcclusionLightIndex
                EditorGUITools.BeginHorizontalBox(() =>
                {
                    EditorGUILayout.LabelField(nameof(output.probeOcclusionLightIndex));
                    output.probeOcclusionLightIndex = EditorGUILayout.IntField(output.probeOcclusionLightIndex);
                });

                //occlusionMaskChannel
                EditorGUITools.BeginHorizontalBox(() =>
                {
                    EditorGUILayout.LabelField(nameof(output.occlusionMaskChannel));
                    output.occlusionMaskChannel = EditorGUILayout.IntField(output.occlusionMaskChannel);
                });


                light.bakingOutput = output;
            });
        }
    }
}
#endif