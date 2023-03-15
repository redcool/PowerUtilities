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

        public Material weightMat;
        public Color weightColor = Color.white;

        ///
        public bool isStartRecordChildren;
        public bool isUpdateChildren;
    }
}
#endif