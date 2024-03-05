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

        private void OnPostprocessModel(GameObject gameObject)
        {
            RemoveMixamoRig(gameObject);
        }

        private static void OnPostprocessAllAssets__(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                var imp = AssetImporter.GetAtPath(asset);
                var type = imp.GetType().Name;

                // model, prefab
                if(type.Contains("ModelImporter") || type.Contains("PrefabImporter") )
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                    RemoveMixamoRig(go);
                }
            }
        }

    }
}
#endif