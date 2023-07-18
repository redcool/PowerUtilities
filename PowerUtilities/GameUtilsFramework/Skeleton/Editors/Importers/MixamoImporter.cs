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
    public class MixamoImporter : AssetPostprocessor
    {
        private void OnPostprocessModel(GameObject gameObject)
        {
            var setting = ScriptableObjectTools.GetInstance<PowerAssetImporterSetting>();
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