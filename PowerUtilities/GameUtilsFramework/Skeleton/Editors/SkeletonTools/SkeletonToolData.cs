#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameUtilsFramework
{
    [Serializable]
    public class SkeletonToolData : ScriptableObject
    {
        public bool enable;
        public GameObject skeletonObj;
        public bool isShowHierarchy=true;
        public bool isKeepChildren = true;
    }
}
#endif