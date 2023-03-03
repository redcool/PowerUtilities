using GameUtilsFramework;
using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSyncTest : MonoBehaviour
{
    public SkinnedMeshRenderer targetSkinned;
    public SkinnedMeshRenderer skinned;

    public string rootBoneName = "Root";
    public Vector3[] boneOffsets;
    Dictionary<Transform, Transform> boneTransforms = new Dictionary<Transform, Transform>();
    // Start is called before the first frame update
    void Start()
    {
        var bonePaths = skinned.GetBonesPath(rootBoneName);
        boneOffsets = new Vector3[bonePaths.Length];
        for (int i = 0; i < bonePaths.Length; i++)
        {
            var bonePath = bonePaths[i];
            var curBone = skinned.rootBone.Find(bonePath);
            var targetBone = targetSkinned.rootBone.Find(bonePath);
            if(targetBone )
            {
                boneOffsets[i] = targetBone.position - curBone.position  - targetBone.root.position;
            }

            boneTransforms[curBone] = targetBone;
        }
        //targetSkinned.RetargetBones("Root", skinned.rootBone);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        boneTransforms.ForEach((item, id) => {
            if (item.Value)
            {
                item.Value.position = item.Value.root.position + item.Key.position + boneOffsets[id];
                //item.Value.position = item.Key.position + boneOffsets[id];
            }
        });
    }
}
