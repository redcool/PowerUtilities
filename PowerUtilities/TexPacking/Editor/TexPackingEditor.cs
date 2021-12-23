#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using System.IO;
    using System.Linq;
    using System;
    using Object = UnityEngine.Object;
    using PowerUtilities;

    public class TexPacking
    {
        [MenuItem("PowerUtilities/PackingTools/TexPacking")]
        static void PackingTex()
        {
            var assetPath = "Assets/TexPacking/TexPacking/PackTex1.asset";
            PathTools.CreateAbsFolderPath(assetPath);

            PackingSelectedTextures(assetPath);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
            Debug.Log("Packing Texture Done.");
        }

        [MenuItem("PowerUtilities/PackingTools/AdjustGameObjects")]
        static void AdjustGameObjects()
        {
            var manifest = EditorTools.GetFirstFilteredFromSelection<TexPackingManifest>(SelectionMode.DeepAssets);
            var gos = EditorTools.GetFilteredFromSelection<GameObject>(SelectionMode.DeepAssets);

            if (manifest && gos.Length > 0)
            {
                AdjustGameObjects(manifest,gos);
                Debug.Log("Adjust meshes Material done.");
            }
            else
            {
                Debug.Log("Need selete manifest and atlas.");
            }
        }

        [MenuItem("PowerUtilities/PackingTools/AdjustMaterial")]
        static void AdjustMaterial()
        {
            var manifest = EditorTools.GetFirstFilteredFromSelection<TexPackingManifest>(SelectionMode.DeepAssets);
            var mats = EditorTools.GetFilteredFromSelection<Material>(SelectionMode.Assets);
            if (manifest)
            {
                foreach (var mat in mats)
                {
                    AdjustSharedMaterial(manifest, mat);
                }
            }
        }

        #region Packing Textures
        static void PackingSelectedTextures(string packManifestPath)
        {
            var texs = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
            //gather textures
            var instanceIds = texs
                .Select((tex) =>
            {
                tex.Setting(imp => {
                    imp.isReadable = true;
                    imp.textureCompression = TextureImporterCompression.Uncompressed;
                    imp.SaveAndReimport();
                });
                return tex;
            })
            .Select(t => t.GetInstanceID())
            .ToArray();

            //packing atlas
            Rect[] rects;
            var atlas = PackTextures(texs, packManifestPath,out rects);

            // manifest
            SaveManifest(packManifestPath, atlas, rects, instanceIds);
        }

        private static Texture2D PackTextures(Texture2D[] texs,string packManifestPath,out Rect[] rects)
        {
            var atlas = new Texture2D(1024, 1024);
            rects = atlas.PackTextures(texs, 0, 1024);

            var folder = Path.GetDirectoryName(packManifestPath);
            var fileName = Path.GetFileNameWithoutExtension(packManifestPath);
            var newTex = EditorTools.Save<Texture2D>(atlas.EncodeToJPG(), string.Format("{0}/{1}.jpg", folder, fileName));
            return newTex;
        }

        static void SaveManifest(string packManifestPath, Texture2D atlas,Rect[] rects,int[] instanceIds)
        {
            var manifest = ScriptableObject.CreateInstance<TexPackingManifest>();
            manifest.rects = rects;
            manifest.instanceIds = instanceIds;
            manifest.atlas = atlas;
            AssetDatabase.CreateAsset(manifest, packManifestPath);
        }

        #endregion

        #region AdjustSharedMaterial
        static void AdjustGameObjects(TexPackingManifest manifest,GameObject[] objs)
        {
            foreach (var item in objs)
            {
                GameObject go = Object.Instantiate(item) as GameObject;
                if (go)
                {
                    var mr = go.GetComponentInChildren<MeshRenderer>();
                    var mainMat = mr.sharedMaterial;
                    if (!mainMat || !mainMat.mainTexture)
                        continue;

                    AdjustSharedMaterial(manifest, mainMat);
                    Object.DestroyImmediate(go);
                }
            }
        }
        private static void AdjustSharedMaterial(TexPackingManifest manifest, Material mainMat)
        {
            if (!manifest || !mainMat)
            {
                Debug.Log("Need manifest and material");
                return;
            }
            var texId = mainMat.mainTexture.GetInstanceID();

            //adjust material offset,scale.
            var rect = manifest.GetRect(texId);
            if (rect != default(Rect))
            {
                mainMat.mainTextureOffset = new Vector2(rect.x, rect.y);
                mainMat.mainTextureScale = new Vector2(rect.width, rect.height);
            }
            mainMat.mainTexture = manifest.atlas;
        }
        #endregion
    }
}
#endif