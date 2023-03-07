using GameUtilsFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    using System;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditorInternal;
    using static UnityEditor.Progress;

    [CustomEditor(typeof(SyncTargetSkeletonTest))]
    public class SyncTargetSkeletonEditor : PowerEditor<SyncTargetSkeletonTest>
    {
        public override void DrawInspectorUI(SyncTargetSkeletonTest inst)
        {
            DrawDefaultGUI();
            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("Record Target Skeleton"))
            {
                inst.info = SyncTargetSkeletonTest.RecordSkeleton(inst.skinned, inst.targetSkinned,inst.rootBone,inst.targetRootBone);
            }
            GUILayout.EndVertical();
        }
    }
#endif

    public class SyncTargetSkeletonTest : MonoBehaviour
    {
        public SkinnedMeshRenderer skinned;
        public Transform rootBone;

        public SkinnedMeshRenderer targetSkinned;
        public Transform targetRootBone;

        public SkeletonInfo info;

        [Serializable]public class SkeletonInfo
        {
            public Transform[] boneTrs;
            public Vector3[] offsets;
            public Vector3[] bindposePositions;
            public Quaternion[] bindpostRot;

            public Transform[] targetBoneTrs;
            public Vector3[] targetBindposePos;

            public SkeletonInfo(int boneLength)
            {
                boneTrs = new Transform[boneLength];
                offsets = new Vector3[boneLength];
                bindposePositions = new Vector3[boneLength];
                bindpostRot = new Quaternion[boneLength];

                targetBoneTrs = new Transform[boneLength];
                targetBindposePos = new Vector3[boneLength];
            }
        }

        public static SkeletonInfo RecordSkeleton(SkinnedMeshRenderer skinned,SkinnedMeshRenderer targetSkinned, 
            Transform rootBone,Transform targetRootBone)
        {
            if (!skinned || !targetSkinned || !rootBone || !targetRootBone)
                throw new ArgumentNullException("skinned or rootBone is null");

            var curRootBoneName = rootBone.name;
            var targetRootBoneName = targetRootBone.name;

            var curBonePaths = SkinnedMeshRendererTools.GetBonesPath(skinned, curRootBoneName);
            //var bonePaths = SkinnedMeshRendererTools.GetBonesPath(targetSkinned, targetRootBoneName);

            var info = new SkeletonInfo(curBonePaths.Length);

            for (int i = 0; i < curBonePaths.Length; i++)
            {
                var bonePath = curBonePaths[i];
                var targetBonePath = bonePath.Replace(curRootBoneName, targetRootBoneName);
                
                var curBone = rootBone.Find(bonePath);
                var targetBone = targetRootBone.Find(targetBonePath);
                info.offsets[i] = Vector3.zero;
                if (targetBone)
                {
                    info.offsets[i] = (targetBone.position - curBone.position);
                }
                info.boneTrs[i] = curBone;
                info.bindposePositions[i] = curBone.position;
                info.bindpostRot[i] = curBone.rotation;

                info.targetBoneTrs[i] = targetBone;

            }
            return info;
        }

        public void Start()
        {
            //MappingSkeleton(skinned, targetSkinned,out bonesOffset,out bindposePostions);
            //offset = b.position - b.root.position - (a.position-a.root.position);
        }

        void LateUpdate()
        {
            info.boneTrs.ForEach((boneTr, i) =>
            {
                if (info.targetBoneTrs[i])
                {
                    //boneTr.position = info.targetBoneTrs[i].position - info.offsets[i];// + boneTr.root.position;// - info.targetBoneTrs[i].root.position;
                    boneTr.rotation = info.targetBoneTrs[i].rotation;
                }
                boneTr.position = info.bindposePositions[i];
                //boneTr.rotation = info.bindpostRot[i];
            });
        }
        

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < info.boneTrs.Length; i++)
            {
                var boneTr = info.boneTrs[i];
                DrawSyncBones(boneTr, info.targetBoneTrs[i]);
            }
        }


        private void DrawSyncBones(Transform a,Transform b)
        {
            if (!a || !b)
                return;

            var offset = (b.position-b.root.position) - (a.position-a.root.position);
            Debug.DrawLine(a.position, a.position+offset,Color.red);
            Debug.DrawLine(a.position, b.position,Color.green);
            Debug.DrawLine(b.position, a.position+offset,Color.blue);
        }
    }
}