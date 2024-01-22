namespace GameUtilsFramework
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using System;
    using PowerUtilities;
    using System.Linq;
    using System.Collections.Generic;
    using static GameUtilsFramework.SkeletonSync;
#if UNITY_2020
    using Tooltip = PowerUtilities.TooltipAttribute;
#endif
#if UNITY_EDITOR
    [CustomEditor(typeof(SkeletonSync))]
    public class SyncSkeletonEditor : PowerEditor<SkeletonSync>
    {
        const string helpStr = "Drive RootBone(Skeleton) by TargetRootBone(Skeleton)";

        public override void DrawInspectorUI(SkeletonSync inst)
        {
            EditorGUILayout.HelpBox(helpStr, MessageType.Info);

            DrawDefaultInspector();

            GUILayout.BeginVertical("Box");
            GUILayout.Label("Options");
            if (GUILayout.Button("Auto Mapping Skeleton"))
            {
                //inst.info = SkeletonSync.MappingSkeleton(inst.skinned, inst.targetSkinned,inst.rootBone,inst.targetRootBone);
                inst.info  = SkeletonSync.MappingSkeleton(inst.rootBone, inst.targetRootBone);
            }
            if (GUILayout.Button("Restore Skeleton from info"))
            {
                SkeletonSync.ReassignSkeleton(inst.rootBone, inst.info);
            }
            GUILayout.EndVertical();
        }
    }

    [CustomPropertyDrawer(typeof(SkeletonSyncInfo))]
    public class SkeletonSyncInfoDrawer : PropertyDrawer
    {
        int LINE_HEIGHT = 18;
        int ITEM_WIDTH = 200;
        int INDENT = 0;
        int TITLE_LINE_COUNT = 2;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var boneTrs = property.FindPropertyRelative(nameof(SkeletonSyncInfo.boneTrs));
            var offsets = property.FindPropertyRelative(nameof(SkeletonSyncInfo.offsets));
            var targetBones = property.FindPropertyRelative(nameof(SkeletonSyncInfo.targetBoneTrs));
            var depths = property.FindPropertyRelative(nameof(SkeletonSyncInfo.boneDepths));
            var targetBonePaths = property.FindPropertyRelative(nameof(SkeletonSyncInfo.targetBonePaths));

            var startPos = position;
            startPos.height = LINE_HEIGHT;
            //startLocalPos.y += LINE_HEIGHT;

            var sizeProp = boneTrs.FindPropertyRelative("Array.size");

            boneTrs.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(startPos, boneTrs.isExpanded, label);
            var viewRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, 100);
            if (boneTrs.isExpanded)
            {
                GUI.Box(position,"");
                //scrollPos = GUI.BeginScrollView(position, scrollPos, viewRect);
                {
                    DrawInfoHeader(ref startPos, sizeProp);

                    EditorGUI.indentLevel+=INDENT;
                    for (int i = 0; i < boneTrs.arraySize; i++)
                    {
                        if (i>= targetBonePaths.arraySize)
                        {
                            EditorGUI.LabelField(startPos, "Skeleton Info not complete inited");
                            break;
                        }

                        DrawInfoItem(ref startPos, boneTrs, targetBones, depths, targetBonePaths, i);
                    }
                    EditorGUI.indentLevel -=INDENT;
                }
                //GUI.EndScrollView();
            }
            EditorGUI.EndFoldoutHeaderGroup();
        }

        private void DrawInfoItem(ref Rect startPos,
            SerializedProperty boneTrs, SerializedProperty targetBones, SerializedProperty depths,
            SerializedProperty targetBonePaths,int i)
        {

            var boneDepth = depths.GetArrayElementAtIndex(i).intValue;
            var boneTrProp = boneTrs.GetArrayElementAtIndex(i);
            var targetBoneProp = targetBones.GetArrayElementAtIndex(i);
            var targetBonePath = targetBonePaths.GetArrayElementAtIndex(i);

            //EditorGUI.indentLevel += boneDepth;
            // bone id
            startPos.x = 0;
            startPos.y += LINE_HEIGHT;
            EditorGUI.LabelField(startPos, i.ToString());
            var bgColor = targetBoneProp.objectReferenceValue ? GUI.color : Color.yellow;
            EditorGUITools.DrawColorLabel(startPos, EditorGUITools.TempContent(i.ToString()), bgColor);
            // bone
            startPos.x +=40;
            EditorGUI.ObjectField(startPos, boneTrProp.objectReferenceValue, typeof(Transform), true);

            // target bone
            EditorGUI.BeginChangeCheck();
            startPos.x += ITEM_WIDTH;
            targetBoneProp.objectReferenceValue = EditorGUI.ObjectField(startPos, targetBoneProp.objectReferenceValue, typeof(Transform), true);
            if (EditorGUI.EndChangeCheck() && targetBoneProp.objectReferenceValue!=null)
            {
                Transform boneTr = (Transform)targetBoneProp.objectReferenceValue;
                var targetRootBone = ((SkeletonSync)targetBoneProp.serializedObject.targetObject).targetRootBone;
                targetBonePath.stringValue = boneTr.GetHierarchyPath(targetRootBone.name);
            }

            //--------- offset
            startPos.x += ITEM_WIDTH;
            //var offsetProp = offsets.GetArrayElementAtIndex(i);
            //offsetProp.vector3Value = EditorGUI.Vector3Field(startLocalPos, "", offsetProp.vector3Value);
            EditorGUI.LabelField(startPos, targetBonePath.stringValue);
        }

        void DrawInfoHeader(ref Rect startPos, SerializedProperty sizeProp)
        {
            startPos.y += LINE_HEIGHT;
            EditorGUI.PropertyField(startPos, sizeProp);

            startPos.width = ITEM_WIDTH;
            startPos.y += LINE_HEIGHT;
            EditorGUI.LabelField(startPos, "Bone");
            startPos.x += ITEM_WIDTH;
            EditorGUI.LabelField(startPos, "TargetBone");
            startPos.x += ITEM_WIDTH;
            EditorGUI.LabelField(startPos, "Offset");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var boneTrs = property.FindPropertyRelative(nameof(SkeletonSyncInfo.boneTrs));
            var size = 1;
            if (boneTrs.isExpanded)
                size += boneTrs.arraySize+TITLE_LINE_COUNT;

            return size * LINE_HEIGHT;
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
            public string[] bonePaths;
            public int[] boneDepths;

            public Transform[] targetBoneTrs;
            public string[] targetBonePaths;

            public SkeletonSyncInfo(int boneLength)
            {
                boneTrs = new Transform[boneLength];
                offsets = new Vector3[boneLength];
                targetBoneTrs = new Transform[boneLength];
                bonePaths = new string[boneLength];
                boneDepths = new int[boneLength];
                targetBonePaths = new string[boneLength];
            }
        }


        public static SkeletonSyncInfo MappingSkeleton(Transform rootBone,Transform targetRootBone)
        {
            if (!rootBone || !targetRootBone)
                throw new ArgumentNullException("rootBone is null");


            var curRootBoneName = rootBone.name;
            var targetRootBoneName = targetRootBone.name;

            var curBonePaths = rootBone.GetComponentsInChildren<Transform>()
                .Where(tr => tr != rootBone)
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
                    if(targetBonesDict.TryGetValue(curBone.name, out targetBone))
                    {
                        targetBonesDict[curBone.name] = targetBone;
                        targetBonePath = targetBone.GetHierarchyPath(targetRootBone);
                    }
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
                info.targetBonePaths[i] = targetBonePath;
                info.bonePaths[i] = curBonePath;
                info.boneDepths[i] = curBonePath.Count(c => c == '/');
            }

            return info;
        }

        public static void ReassignSkeleton(Transform rootBone,SkeletonSyncInfo info)
        {
            if (!rootBone || info == null)
                return;

            for (int i = 0; i < info.bonePaths.Length; i++)
            {
                var bonePath = info.bonePaths[i];
                info.boneTrs[i] = rootBone.Find(bonePath);
            }
        }

        public void Start()
        {
            if (info == null || info.boneTrs == null)
                info = MappingSkeleton(rootBone, targetRootBone);
        }

        void LateUpdate()
        {
            if (info ==null || info.boneTrs == null)
                return;

            info.boneTrs.ForEach((boneTr, i) =>
            {
                if (boneTr && info.targetBoneTrs[i])
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