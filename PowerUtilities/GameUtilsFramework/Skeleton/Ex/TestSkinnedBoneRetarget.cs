using GameUtilsFramework;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PowerUtilities
{
    public class TestSkinnedBoneRetarget : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("source skinnedMeshRenderer")]
        public SkinnedMeshRenderer skinned;

        [Tooltip("source skinnedMeshRenderer rootBoneName")]
        public string rootBoneName;

        [Header("Target")]
        [Tooltip("target skinnedMeshRenderer rootBone")]
        public Transform newRootBone;

        [Header("bone path replace")]
        [Tooltip("source bonePath pattern")]
        public string bonePathPattern = "mixamorig:";

        [Tooltip("target bonePath pattern")]
        public string bonePathReplacement = "mixamorig_";

        [Tooltip("start retarget skeleton")]
        [EditorButton(onClickCall = "TestRetarget")]
        public bool isTestRetarget;

        [Tooltip("AddShowSkinnedBones")]
        [EditorButton(onClickCall = "AddShowSkeleton")]
        public bool isAddShowSkineBones;


        void TestRetarget()
        {
            if (string.IsNullOrEmpty(rootBoneName))
                return;

            skinned.RetargetBones(
                rootBoneName,
                newRootBone,
                bonePath=> Regex.Replace(bonePath, bonePathPattern, bonePathReplacement)
            );
        }

        void AddShowSkeleton()
        {
            var showSkinnedBone = gameObject.GetOrAddComponent<ShowSkinnedBones>();
            showSkinnedBone.skinned = skinned;
        }
    }
}
