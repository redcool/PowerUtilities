#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PowerUtilities
{
    [CustomEditor(typeof(PowerUtilsWindowShortcutSetting))]
    public class PowerUtilsWindowShortcutSettingEditor : PowerEditor<PowerUtilsWindowShortcutSetting>
    {
        List<List<(string displayName,string typeName, string tooltip)>> allWinTypes = new List<List<(string,string, string)>>
        {
            new List<(string,string ,string)>{
                ("SRPPipelineAssetWin","PowerUtilities.SRPPipelineAssetWin" ,"Manager project's pipeline assets"),
            },
            new List<(string, string, string)>
            {
                ("SceneObjectFinderWin","PowerUtilities.SceneObjectFinderWin","Find Hierarchy objects and select them"),
                ("ItemPlacementWin","PowerUtilities.ItemPlacementWindow","Place objects in SceneView"),
                ("PlacementWindow","PowerUtilities.Scenes.PlacementWindow","Random place objects in [min,max] border"),
            }
        };

        List<string> typeLables = new List<string>
        {
            "Pipeline Manager","Object Managers"
        };

        public override void DrawInspectorUI(PowerUtilsWindowShortcutSetting inst)
        {
            EditorGUILayout.BeginVertical();
            allWinTypes.ForEach((winTypes ,typeId)=>
            {
                EditorGUILayout.SelectableLabel(typeLables[typeId]);

                EditorGUILayout.BeginHorizontal();

                winTypes.ForEach(winType =>
                {
                    if (GUILayout.Button(GUIContentEx.TempContent($"Open {winType.displayName}", winType.tooltip)))
                    {
                        EditorWindowTools.OpenWindow(Type.GetType(winType.typeName));
                    }
                });
                EditorGUILayout.EndHorizontal();

            });
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// powerutils windows shortcuts
    /// show in Preferences/Windows
    /// </summary>
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Windows/"+nameof(PowerUtilsWindowShortcutSetting))]
    [SOAssetPath(SOAssetPathAttribute.PATH_POWER_UTILITIES+ nameof(PowerUtilsWindowShortcutSetting) +".asset")]
    public class PowerUtilsWindowShortcutSetting : ScriptableObject
    {

    }
}
#endif