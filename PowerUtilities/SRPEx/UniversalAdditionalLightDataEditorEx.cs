#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    [CanEditMultipleObjects]
    // open some ui for convenient
    [CustomEditor(typeof(UniversalAdditionalLightData))]
    class UniversalAdditionalLightDataEditorEx : Editor
    {
        (string, bool) bakingOutputFoldInfo = ("BakingOutputSettings", false);

        readonly GUIContent BakingOutputContent = new GUIContent("Output", "setup light baking output");

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();

            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.indentLevel++;

            DrawLightBakingSettings();

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
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