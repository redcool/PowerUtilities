namespace GameUtilsFramework
{
    using UnityEngine;
    using System;
    using PowerUtilities;
#if UNITY_EDITOR
    using UnityEditor;
    using static GameUtilsFramework.SkeletonSync;
    using System.Linq;
    using System.Collections.Generic;

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
                //inst.info = SkeletonSync.RecordSkeleton(inst.skinned, inst.targetSkinned,inst.rootBone,inst.targetRootBone);
                inst.info  = SkeletonSync.RecordSkeleton(inst.rootBone, inst.targetRootBone);
            }
            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(SkeletonSyncInfo))]
    public class SkeletonSyncInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var boneTrs = property.FindPropertyRelative(nameof(SkeletonSyncInfo.boneTrs));
            var offsets = property.FindPropertyRelative(nameof(SkeletonSyncInfo.offsets));
            var targetBones = property.FindPropertyRelative(nameof(SkeletonSyncInfo.targetBoneTrs));

            var startPos = position;
            startPos.height = 18;
            //startPos.y += 18;

            var sizeProp = boneTrs.FindPropertyRelative("Array.size");

            boneTrs.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(startPos,boneTrs.isExpanded,label);

            if (boneTrs.isExpanded)
            {
                startPos.y += 18;
                EditorGUI.PropertyField(startPos, sizeProp);

                startPos.width = 200;
                startPos.y += 18;
                EditorGUI.LabelField(startPos, "Bone");
                startPos.x += 200;
                EditorGUI.LabelField(startPos, "TargetBone");
                startPos.x += 200;
                EditorGUI.LabelField(startPos, "Offset");

                EditorGUI.indentLevel+=2;

                for (int i = 0; i < boneTrs.arraySize; i++)
                {
                    startPos.x = 0;
                    startPos.y += 18;
                    //GUILayout.BeginHorizontal();
                    var boneTrProp = boneTrs.GetArrayElementAtIndex(i);
                    EditorGUI.ObjectField(startPos, boneTrProp.objectReferenceValue,typeof(Transform),true);

                    startPos.x += 200;
                    var targetBoneProp = targetBones.GetArrayElementAtIndex(i);

                    targetBoneProp.objectReferenceValue = EditorGUI.ObjectField(startPos, targetBoneProp.objectReferenceValue, typeof(Transform), true);
                    //EditorGUI.PropertyField(startPos, targetBoneProp.FindPropertyRelative("data"));

                    startPos.x += 200;
                    var offsetProp = offsets.GetArrayElementAtIndex(i);
                     EditorGUI.Vector4Field(startPos, "", offsetProp.vector4Value);
                    //EditorGUI.PropertyField(startPos, offsetProp);
                    //GUILayout.EndHorizontal();
                    
                }
                EditorGUI.indentLevel -=2;
            }
            EditorGUI.EndFoldoutHeaderGroup();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            var boneTrs = property.FindPropertyRelative("boneTrs");
            var size = 1;
            if(boneTrs.isExpanded)
                size += boneTrs.arraySize+1;

            return size * 18;
        }
    }
#endif

    public class SkeletonSync : MonoBehaviour
    {
        [Header("Reference Skeleton")]
        //public SkinnedMeshRenderer targetSkinned;
        public Transform targetRootBone;

        [Header("Synchronized Skeleton")]
        //public SkinnedMeshRenderer skinned;
        public Transform rootBone;

        [Header("Options")]
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


        public static SkeletonSyncInfo RecordSkeleton(Transform rootBone,Transform targetRootBone)
        {
            if (!rootBone || !targetRootBone)
                throw new ArgumentNullException("rootBone is null");


            var curRootBoneName = rootBone.name;
            var targetRootBoneName = targetRootBone.name;

            var curBonePaths = rootBone.GetComponentsInChildren<Transform>()
                .SkipWhile(tr => tr == rootBone)
                .Select(tr=>tr.GetHierarchyPath(rootBone))
                .ToArray();


            var targetBonesDict = new Dictionary<string, Transform>();
            var targetBones = targetRootBone.GetComponentsInChildren<Transform>();
            //var targetBoneNames = targetBones.Select(tr =>tr.Substring(tr.LastIndexOf("/")));
            targetBones.ForEach(tr => targetBonesDict[tr.name] = tr);

            var info = new SkeletonSyncInfo(curBonePaths.Length);

            for (int i = 0; i < curBonePaths.Length; i++)
            {
                var curBonePath = curBonePaths[i];
                var curBone = rootBone.Find(curBonePath);

                // find bone by path
                var targetBonePath = curBonePath.Replace(curRootBoneName, targetRootBoneName);
                var targetBone = targetRootBone.Find(targetBonePath);
                // find bone by name
                if (!targetBone)
                {
                    targetBonesDict.TryGetValue(curBone.name, out targetBone);
                }
                if (!targetBone)
                {
                    Debug.Log($"{curBonePath} not found.");
                }

                // record info
                info.offsets[i] = Vector3.zero;
                if (targetBone)
                {
                    info.offsets[i] = (targetBone.position - curBone.position);
                }
                info.boneTrs[i] = curBone;
                info.targetBoneTrs[i] = targetBone;
                info.bonePaths[i] = curBonePath;
                info.boneDepths[i] = curBonePath.Count(c => c == '/');
            }

            return info;
        }

        public void Start()
        {
            if (info == null || info.boneTrs == null)
                info = RecordSkeleton(rootBone, targetRootBone);
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
        

        //private void OnDrawGizmosSelected()
        //{
        //    for (int i = 0; i < info.boneTrs.Length; i++)
        //    {
        //        var boneTrProp = info.boneTrs[i];
        //        DrawSyncBones(boneTrProp, info.targetBoneTrs[i]);
        //    }
        //}

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