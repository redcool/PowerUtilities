#if UNITY_EDITOR
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
    [Serializable]
    public class SkeletonSceneViewToolData : ScriptableObject
    {
        public bool enable;
        public GameObject skeletonObj;
        public SkinnedMeshRenderer skinned;
        public bool isShowHierarchy=true;
        public bool isKeepChildren = false;
        public bool isShowWeights;
        public List<Vector3> bonesScreenPosList = new List<Vector3>();

        public Material weightMat;
        public Color weightColor = Color.white;

        ///
        public bool isSceneViewMouseDown;
        public bool isSceneViewMouseDrag;
        public bool isSceneViewMouseUp;

        /// rect selection
        public bool isRectSelectionStart;
        public Rect selectionRect;
    }
}
#endif