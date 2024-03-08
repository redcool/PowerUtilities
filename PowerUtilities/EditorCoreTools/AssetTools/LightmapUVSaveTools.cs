#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public class LightmapUVSaveTools : AssetPostprocessor
    {

        public static void SaveUV2(bool disableGenerateUV2)
        {
            var folders = SelectionTools.GetSelectedFolders();
            var infos = AssetTools.GetModelsGenerateUV2(folders);

            foreach (var objInfo in infos)
            {
                //Debug.Log(objInfo.gameObject+":"+ objInfo.modelImporter);
                SaveUV2(objInfo.gameObject, objInfo.modelImporter);

                if (disableGenerateUV2)
                {
                    objInfo.modelImporter.generateSecondaryUV = false;
                    objInfo.modelImporter.SaveAndReimport();
                }
            }

            AssetDatabaseTools.SaveRefresh();
        }

        public static void SaveUV2(GameObject gameObject, ModelImporter imp)
        {
            var path = PathTools.ChangeExtName(imp.assetPath, ".asset");

            var extInfo = ScriptableObjectTools.CreateGetInstance<ModelExtendInfo>(path);
            extInfo.meshList.Clear();


            var mfs = gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in mfs)
            {
                var mesh = Object.Instantiate(mf.sharedMesh);
                AssetDatabase.AddObjectToAsset(mesh, extInfo);
                //AssetDatabaseTools.AddObjectToAsset(mesh, extInfo,true);

                extInfo.meshList.Add(mesh);
            }
            extInfo.assetPath = imp.assetPath;

            //EditorUtility.SetDirty(extInfo);
        }

        public static void RestoreMesh()
        {
            var extInfos = AssetDatabaseTools.FindAssetsInProject<ModelExtendInfo>("t:MeshExtendInfo",SelectionTools.GetSelectedFolders())
                .Where(info => !string.IsNullOrEmpty(info.assetPath));

            foreach (var extInfo in extInfos)
            {
                // need load and instantiate
                var goAsset = AssetDatabase.LoadAssetAtPath<GameObject>(extInfo.assetPath);
                var go = Object.Instantiate(goAsset);

                var mfs = go.GetComponentsInChildren<MeshFilter>();
                if (mfs.Length != extInfo.meshList.Count)
                {
                    Debug.LogWarning($"skip {extInfo.assetPath} ,not same model structure");
                    continue;
                }

                for (int i = 0; i < mfs.Length; i++)
                {
                    var mesh = extInfo.meshList[i];
                    var mf = mfs[i];
                    //mf.sharedMesh = mesh;

                    mf.sharedMesh.CopyFrom(mesh);
                }

                go.Destroy();
            }

            AssetDatabaseTools.SaveRefresh();
        }

        public static void ClearModelExtendInfos()
        {
            var paths = AssetDatabaseTools.FindAssetsPath("t:ModelExtendInfo", "asset", searchInFolders: SelectionTools.GetSelectedFolders());

            var list = new List<string>();
            AssetDatabase.DeleteAssets(paths, list);
        }
    }
}
#endif