#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public class FBXExportTools
    {

        public static void ExportModels(bool disableGenerateUV2)
        {
            //var imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeGameObject));
            //AssetTools.ExportFBX(Selection.activeGameObject, imp.assetPath);
            //return;

            var folders = SelectionTools.GetSelectedFolders();
            var infos = AssetTools.GetModelsGenerateUV2(folders);

            foreach (var objInfo in infos)
            {
                AssetTools.ExportFBX(objInfo.gameObject, objInfo.modelImporter.assetPath);

                if (disableGenerateUV2)
                {
                    objInfo.modelImporter.generateSecondaryUV = false;
                    objInfo.modelImporter.SaveAndReimport();
                }
            }

            AssetDatabaseTools.SaveRefresh();
        }

    }
}
#endif