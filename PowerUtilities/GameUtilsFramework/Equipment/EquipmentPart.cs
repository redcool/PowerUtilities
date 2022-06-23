using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameUtilsFramework
{

    public enum CharacterPart
    {
        None=0,
        Head,
        Eyebrows,
        FacialHair,
        Torso,
        ArmUpperLeft,ArmUpperRight,
        ArmLowerLeft,ArmLowerRight,
        HandLeft,HandRight,
        Hips, // thing part
        LegLeft,
        LegRight,
    }

    [Serializable]
    public class EquipmentPart
    {
        public string rootBoneName = "Root";
        public CharacterPart part;
        [HideInInspector]public SkinnedMeshRenderer skinned;
        public SkinnedBonesData skinData;

        /// <summary>
        /// save and place overrideSkinned, retarget overrideSkinned bones to newBone
        /// </summary>
        /// <param name="parentTr"></param>
        /// <param name="overrideSkinned"></param>
        /// <param name="lastBoneRootName"></param>
        /// <param name="newBoneRoot"></param>
        public void Equip(Transform parentTr, SkinnedMeshRenderer overrideSkinned, string lastBoneRootName, Transform newBoneRoot)
        {
            skinned = overrideSkinned;
            skinned.transform.SetParent(parentTr);
            skinned.RetargetBones(lastBoneRootName, newBoneRoot);
        }

        /// <summary>
        /// Instantiate overridePart's skinned, Equip it
        /// </summary>
        /// <param name="parentTr"></param>
        /// <param name="overridePart"></param>
        /// <param name="newBoneRoot"></param>
        public void Equip(Transform parentTr,EquipmentPart overridePart,Transform newBoneRoot)
        {
            var data = overridePart.skinData;
            var overrideSkinned = Object.Instantiate(data.skinnedPrefab);
            overrideSkinned.rootBone = newBoneRoot.Find(data.rootBonePath);
            overrideSkinned.ReassignBones(overridePart.skinData.bonesPath,newBoneRoot);

            Equip(parentTr, overrideSkinned, overridePart.rootBoneName, newBoneRoot);
        }

        public void UnequipAndDestroy()
        {
            if (!skinned)
                return;

#if UNITY_EDITOR
            GameObject.DestroyImmediate(skinned.gameObject);
#else
            GameObject.Destroy(equipPart.skinned.gameObject);
#endif
        }
    }


}
