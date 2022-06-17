using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUtilsFramework
{
    public static class SkinnedMeshRendererTools
    {
        public static void RetargetBones(this SkinnedMeshRenderer skinned,string lastRootBoneName, Transform newRootBone)
        {
            if (!skinned || !newRootBone)
                return;

            var newBones = new Transform[skinned.bones.Length];
            for (int i = 0; i < skinned.bones.Length; i++)
            {
                var bone = skinned.bones[i];
                var bonePath = GameObjectTools.GetHierarchyPath(bone, lastRootBoneName);
                var newBone = newRootBone.Find(bonePath);
                newBones[i] = newBone; 
            }
            skinned.bones = newBones;
        }

        public static string[] GetBonesPath(this SkinnedMeshRenderer skinned,string rootBoneName)
        {
            var paths = new string[skinned.bones.Length];
            for (int i = 0; i < skinned.bones.Length; i++)
            {
                var bone = skinned.bones[i];
                var bonePath = GameObjectTools.GetHierarchyPath(bone, rootBoneName);
                paths[i] = bonePath;
            }
            return paths;
        }

        public static void ReassignBones(this SkinnedMeshRenderer skinned,string[] bonesPaths,Transform newRootBone)
        {
            var bones = new Transform[bonesPaths.Length];
            for (int i = 0; i < bonesPaths.Length; i++)
            {
                bones[i] = newRootBone.Find(bonesPaths[i]);
            }
            skinned.bones = bones;
        }
    }
}
