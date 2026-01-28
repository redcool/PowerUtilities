using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PowerUtilities
{

    public class CombineChildrenMeshGroupByMaterial : MonoBehaviour
    {
        [Tooltip("Use current gameObject when null")]
        public GameObject rootGo;

        [Tooltip("Include inactive children mesh renderer ")]
        public bool isIncludeInactive;

        [Tooltip("combine meshes when start")]
        public bool isCombineOnStart = true;

        [Tooltip("disable orignal renderers when combined")]
        public bool isDisableOriginalRenderers = true;

        [Tooltip("Save combined meshes to SceneFolder")]
        public bool isSaveMeshes = true;

        [Tooltip("Per mesh per color")]
        public bool isRandomMeshColor;

        [Tooltip("exclude this tag")]
#if UNITY_EDITOR
        [StringListSearchable(type = typeof(TagManager), staticMemberName = nameof(TagManager.GetTags))]
#endif
        public string excludeTag = "";

        [EditorButton(onClickCall = "Start",tooltip ="combine children meshes now")]
        public bool isCombine;

        // Start is called before the first frame update
        void Start()
        {
            if (isCombineOnStart)
            {
                rootGo = rootGo ? rootGo : gameObject;
                var meshFilterList = MeshTools.CombineMeshesGroupByMaterial(rootGo, isDisableOriginalRenderers, isIncludeInactive, isRandomMeshColor, excludeTag);

                if (isSaveMeshes)
                {
                    SaveMeshFilters(meshFilterList);
                }
            }
        }

        /// <summary>
        /// Save mesh filters to scene asset folder [Editor method]
        /// </summary>
        /// <param name="meshFilterList"></param>
        public static void SaveMeshFilters(List<MeshFilter> meshFilterList)
        {
#if UNITY_EDITOR
            var sceneAssetFolder = AssetDatabaseTools.CreateGetSceneFolder();
            foreach (var mf in meshFilterList)
            {
                AssetDatabaseTools.CreateAssetAtSceneFolder(mf.sharedMesh, $"{mf.gameObject.name}_{mf.sharedMesh.name}.asset");
            }
            AssetDatabaseTools.SaveRefresh();
#endif
        }
    }
}
