#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameUtilsFramework
{
    [ProjectSettingGroup("PowerUtils/SkeletonViewTool")]
    [SOAssetPath("Assets/PowerUtilities/SkeletonData.asset")]
    [Serializable]
    public class SkeletonSceneViewToolData : ScriptableObject
    {
        public bool enable;
        public GameObject skeletonObj;
        public SkinnedMeshRenderer skinned;
        public bool isShowHierarchy = true;
        public bool isKeepChildren = false;
        public bool isShowWeights;
        public List<Vector3> bonesScreenPosList = new List<Vector3>();

        [Header("Skin Weight color")]
        [Tooltip("material show vertex skin weight, use ShowVertexColor.shader")]
        public Material weightMat;

        [Tooltip("bone weight vertex color")]
        [ColorUsage(true, true)]
        public Color weightColor = Color.white;

        [Tooltip("joint point color")]
        [ColorUsage(true, true)]
        public Color jointColor = Color.gray;

        [Header("Skeleton color")]
        [ColorUsage(true, true)]
        [Tooltip("bone line color")]
        public Color boneHierarchyLineColor;
        public float boneHierarchyLineWidth =4;
        ///
        public bool isSceneViewMouseDown;
        public bool isSceneViewMouseDrag;
        public bool isSceneViewMouseUp;

        /// rect selection
        public bool isRectSelectionStart;
        public Rect selectionRect;

        public Material WeightMat
        {
            get
            {
                if (!weightMat)
                {
                    var shader = Shader.Find("Hidden/PowerUtilities/Unlit/ShowVertexColor");
                    weightMat = new Material(shader);

                    if (!shader)
                        throw new Exception("Hidden/PowerUtilities/Unlit/ShowVertexColor not found");
                }
                return weightMat;
            }
        }
    }
}
#endif