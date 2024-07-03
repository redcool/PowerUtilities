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
    public class MixamoImporter
    #if MIXAMO_RENAME
        : AssetPostprocessor
    #endif
    {
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

#if MIXAMO_RENAME
        private void OnPostprocessModel(GameObject gameObject)
        {
            RemoveMixamoRig(gameObject);
        }
#endif

    }
}
#endif