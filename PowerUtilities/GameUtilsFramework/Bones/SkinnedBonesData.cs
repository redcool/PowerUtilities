using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameUtilsFramework
{
    public class SkinnedBonesData : ScriptableObject
    {
        public string rootBonePath;
        public string[] bonesPath;
        public SkinnedMeshRenderer skinnedPrefab;
    }
}
