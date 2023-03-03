namespace GameUtilsFramework
{
    using UnityEngine;
    using System;
    using PowerUtilities;
#if UNITY_EDITOR
    using UnityEditor;
    using static GameUtilsFramework.SkeletonSync;
    using System.Linq;

    [CustomEditor(typeof(SkeletonSync))]
    public class SyncSkeletonEditor : PowerEditor<SkeletonSync>
    {
        const string helpStr = "Sync TargetSkinned to Skinned";

        public override void DrawInspectorUI(SkeletonSync inst)
        {
            //EditorGUILayout.SelectableLabel(helpStr);
            DrawDefaultGUI();
            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("Record Target Skeleton"))
            {
                inst.info = SkeletonSync.RecordSkeleton(inst.skinned, inst.targetSkinned,inst.rootBone,inst.targetRootBone);
            }
            GUILayout.EndVertical();
        }
    }

    //[CustomPropertyDrawer(typeof(SkeletonSyncInfo))]
    public class SkeletonSyncInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var boneTrs = property.FindPropertyRelative(nameof(SkeletonSyncInfo.boneTrs));
            var offsets = property.FindPropertyRelative(nameof(SkeletonSyncInfo.offsets));
            var targetBones = property.FindPropertyRelative(nameof(SkeletonSyncInfo.targetBoneTrs));

            var size = boneTrs.FindPropertyRelative("Array.size").intValue;
            EditorGUILayout.PropertyField(boneTrs);

            if (boneTrs.isExpanded)
            {
                var rect = new Rect(0,position.y,EditorGUIUtility.currentViewWidth,18);
                for (int i = 0; i < size; i++)
                {
                    rect.y += i * 18;
                    //GUILayout.BeginHorizontal();
                    EditorGUI.ObjectField(rect,boneTrs.GetArrayElementAtIndex(i));
                    //rect.x += 100;
                    //EditorGUI.PropertyField(rect, targetBones.GetArrayElementAtIndex(i));
                    ////rect.x += 100;
                    //EditorGUI.PropertyField(rect, offsets.GetArrayElementAtIndex(i));
                    //GUILayout.EndHorizontal();
                }

            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18;
            var boneTrs = property.FindPropertyRelative("boneTrs");
            var size = 1;
            if(boneTrs.isExpanded)
                size += boneTrs.FindPropertyRelative("Array.size").intValue;

            return size * 18;
        }
    }
#endif

    public class SkeletonSync : MonoBehaviour
    {
        [Header("Reference Skinned")]
        public SkinnedMeshRenderer targetSkinned;
        public Transform targetRootBone;

        [Header("Synchronized Skinned")]
        public SkinnedMeshRenderer skinned;
        public Transform rootBone;

        [Tooltip("< syncPositionMinBoneDepth will sync position")]
        public int syncPositionMinBoneDepth =2;
        public SkeletonSyncInfo info;

        [Serializable]public class SkeletonSyncInfo
        {
            public Transform[] boneTrs;
            public Vector3[] offsets;
            public Transform[] targetBoneTrs;
            public string[] bonePaths;
            public int[] boneDepths;

            public SkeletonSyncInfo(int boneLength)
            {
                boneTrs = new Transform[boneLength];
                offsets = new Vector3[boneLength];
                targetBoneTrs = new Transform[boneLength];
                bonePaths = new string[boneLength];
                boneDepths = new int[boneLength];
            }
        }

        public static SkeletonSyncInfo RecordSkeleton(SkinnedMeshRenderer skinned,SkinnedMeshRenderer targetSkinned, 
            Transform rootBone,Transform targetRootBone)
        {
            if (!skinned || !targetSkinned || !rootBone || !targetRootBone)
                throw new ArgumentNullException("skinned or rootBone is null");

            var curRootBoneName = rootBone.name;
            var targetRootBoneName = targetRootBone.name;

            var curBonePaths = SkinnedMeshRendererTools.GetBonesPath(skinned, curRootBoneName);
            //var bonePaths = SkinnedMeshRendererTools.GetBonesPath(targetSkinned, targetRootBoneName);

            var info = new SkeletonSyncInfo(curBonePaths.Length);

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
                info.targetBoneTrs[i] = targetBone;
                info.bonePaths[i] = bonePath;
                info.boneDepths[i] = bonePath.Count(c=> c == '/');
            }
            return info;
        }

        public void Start()
        {
            if (info == null || info.boneTrs == null)
                info = RecordSkeleton(skinned, targetSkinned, rootBone, targetRootBone);
        }

        void LateUpdate()
        {
            if (info ==null || info.boneTrs == null)
                return;

            info.boneTrs.ForEach((boneTr, i) =>
            {
                if (info.targetBoneTrs[i])
                {
                    boneTr.rotation = info.targetBoneTrs[i].rotation;
                    if (info.boneDepths[i] < syncPositionMinBoneDepth)
                        boneTr.position = info.targetBoneTrs[i].position - info.offsets[i];
                }
            });
        }
        

        private void OnDrawGizmosSelected()
        {
            return;
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