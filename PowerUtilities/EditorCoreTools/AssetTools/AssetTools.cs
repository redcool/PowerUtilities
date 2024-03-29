﻿#if UNITY_EDITOR
using AsciiFBXExporter;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class AssetTools
    {
        /// <summary>
        /// Get Models generate UV2 enabled from  Assets folder
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<(GameObject gameObject, ModelImporter modelImporter)> GetModelsGenerateUV2(params string[] folders)
        {
            var q =
                from go in AssetDatabaseTools.FindAssetsInProject<GameObject>("t:GameObject",folders)
                where go.GetComponentInChildren<MeshFilter>()
                let imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(go)) as ModelImporter
                where imp != null && imp.generateSecondaryUV
                select (go, imp);
            return q;
        }

        public static void ExportFBX(GameObject rootGo, string assetPath)
        {
            FBXExporter.ExportGameObjToFBX(rootGo, assetPath);
        }
    }
}
#endif