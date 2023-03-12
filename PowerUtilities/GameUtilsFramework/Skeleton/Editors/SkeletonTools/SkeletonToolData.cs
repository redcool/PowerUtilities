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
    public class SkeletonToolData : ScriptableObject
    {
        public bool enable;
        public GameObject skeletonObj;
        public bool isShowHierarchy=true;
        public bool isKeepChildren = true;

        ///
        public bool isStartRecordChildren;
        public bool isUpdateChildren;
    }
}
#endif