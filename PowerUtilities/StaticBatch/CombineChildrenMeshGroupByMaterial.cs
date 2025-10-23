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

        [EditorButton(onClickCall = "Start",tooltip ="combine children meshes now")]
        public bool isCombine;

        // Start is called before the first frame update
        void Start()
        {
            if (isCombineOnStart)
            {
                var meshList = MeshTools.CombineMeshesGroupByMaterial(gameObject, isDisableOriginalRenderers, isIncludeInactive);

#if UNITY_EDITOR
                if (isSaveMeshes)
                {
                    var sceneAssetFolder = AssetDatabaseTools.CreateGetSceneFolder();
                    foreach (var mesh in meshList)
                    {
                        AssetDatabaseTools.CreateAssetAtSceneFolder(mesh, mesh.name + ".asset");
                    }
                    AssetDatabaseTools.SaveRefresh();
                }
#endif
            }
        }


    }
}
