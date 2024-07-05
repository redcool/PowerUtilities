#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GameUtilsFramework
{
    public static class MixamoImporter
    {
        [InitializeOnLoadMethod]
        static void OnInit()
        {
            AssetPostProcessorControl.onPostProcessAsset += (imp, obj) =>
            {
                RemoveMixamoRig(obj as GameObject);
            };
        }

        public static void RemoveMixamoRig(GameObject gameObject)
        {
            var setting = ScriptableObjectTools.CreateGetInstance<PowerAssetImporterSetting>();
            if (!setting.isRemoveMixamoRig)
                return;

            var trs = gameObject.GetComponentsInChildren<Transform>();
            trs.ForEach(tr =>
            {
                var nameName = Regex.Replace(tr.gameObject.name, @"mixamorig\d*:", "");
                tr.gameObject.name = nameName;
            });
        }

    }
}
#endif