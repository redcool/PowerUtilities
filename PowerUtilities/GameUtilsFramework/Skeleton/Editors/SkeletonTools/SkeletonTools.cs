#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PowerUtilities;
using System;
using Object = UnityEngine.Object;
using System.Linq;

namespace GameUtilsFramework
{

    public class SkeletonTools
    {
        static SkeletonToolData skeletonToolData = null;
        public static SkeletonToolData SkeletonToolData
        {
            get
            {
                if (skeletonToolData == null)
                    LoadDefaultSkeletonData(ref skeletonToolData);
                return skeletonToolData;
            }
        }

        /// <summary>
        /// record selectedTransform 's children when mouse down
        /// </summary>
        static (Transform tr, Vector3 pos)[] lastSelectedInfos;

        /// <summary>
        /// store positions for draw lines
        /// </summary>
        static List<Vector3> skeletonLinesList = new List<Vector3>();

        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            SceneView.duringSceneGui += SceneView_duringSceneGui;

            EditorApplication.hierarchyChanged -= EditorApplication_hierarchyChanged;
            EditorApplication.hierarchyChanged += EditorApplication_hierarchyChanged;
        }

        private static void EditorApplication_hierarchyChanged()
        {
        }

        private static void ReverseChildrenPos()
        {
            var tr = Selection.activeTransform;
            if (!tr || 
                SkeletonToolData.isUpdateChildren || 
                !skeletonToolData.isKeepChildren ||
                lastSelectedInfos == null)

                return;

            foreach (var info in lastSelectedInfos)
            {
                info.tr.position = info.pos;
            }
        }

        private static void SceneView_duringSceneGui(SceneView view)
        {
            Handles.BeginGUI();
            DrawToolbar(SkeletonToolData);

            if (SkeletonToolData.enable)
            {
                var e = Event.current;

                SkeletonToolData.isStartRecordChildren = e.type == EventType.MouseDown && e.isMouse;
                SkeletonToolData.isUpdateChildren = (e.type == EventType.MouseDrag);

                DrawSkeletonHierarchy(SkeletonToolData);

                RecordSelectedTransform(ref lastSelectedInfos);
                ReverseChildrenPos();
            }

            Handles.EndGUI();

        }

        private static void RecordSelectedTransform(ref (Transform, Vector3)[] childrenInfo)
        {
            if (SkeletonToolData.isStartRecordChildren)
            {
                var trs = Selection.transforms;
                var list = new List<(Transform, Vector3)>();
                foreach (var tr in trs)
                {
                    var q = tr.GetComponentsInChildren<Transform>()
                    .SkipWhile(t => t== tr)
                    .Select(tr => (tr, tr.position));

                    list.AddRange(q);
                }

                childrenInfo = list.ToArray();
            }
        }

        private static void DrawToolbar(SkeletonToolData data)
        {
            var toolbarWidth = data.enable ? EditorGUIUtility.currentViewWidth - 200 : 120;
            GUILayout.BeginArea(new Rect(50, 0, toolbarWidth, 24));
            GUILayout.BeginHorizontal(EditorStylesEx.ShurikenModuleBg);
            {
                data.enable = EditorGUILayout.ToggleLeft(nameof(SkeletonTools), data.enable);
                if (data.enable)
                {
                    data.skeletonObj = EditorGUILayout.ObjectField(data.skeletonObj, typeof(GameObject), true) as GameObject;
                    data.isShowHierarchy = EditorGUILayout.Toggle("Show Hierarchy", data.isShowHierarchy);
                    data.isKeepChildren = EditorGUILayout.Toggle("Keep Children", data.isKeepChildren);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private static void LoadDefaultSkeletonData(ref SkeletonToolData data)
        {
            if (data)
                return;

            const string PATH = "Assets/PowerUtilities/SkeletonData.asset";
            var items = AssetDatabaseTools.FindAssetsInProject<SkeletonToolData>("SkeletonData");
            if (items.Length == 0)
            {
                PathTools.CreateAbsFolderPath(PATH);
                AssetDatabase.Refresh();

                data = ScriptableObject.CreateInstance<SkeletonToolData>();
                AssetDatabase.CreateAsset(data, PATH);
            }
            else
            {
                data = items[0];
            }
        }



        private static void DrawSkeletonHierarchy(SkeletonToolData data)
        {
            if (!data.skeletonObj || !data.isShowHierarchy)
                return;

            FindSkeletonLines(data, skeletonLinesList);
            Handles.DrawLines(skeletonLinesList.ToArray());

            DrawJoints(data);
        }

        private static void DrawJoints(SkeletonToolData data)
        {
            var trs = data.skeletonObj.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trs.Length; i++)
            {
                var tr = trs[i];
                var pos = HandleUtility.WorldToGUIPoint(tr.position);
                if (GUI.Button(new Rect(pos.x - 5, pos.y - 5, 10, 10), "1"))
                {
                    Selection.activeTransform = tr;
                }
            }
        }

        private static void FindSkeletonLines(SkeletonToolData data,List<Vector3> skeletonLinesList)
        {
            //if (data.isUpdateChildren || skeletonLinesList.Count == 0)
            {
                skeletonLinesList.Clear();
                FindPositions(data.skeletonObj.transform, skeletonLinesList);
            }
        }

        static void FindPositions(Transform tr, List<Vector3> posList)
        {
            if (!tr)
                return;

            var pos = HandleUtility.WorldToGUIPoint(tr.position);
            for (int i = 0; i < tr.childCount; i++)
            {
                var child = tr.GetChild(i);
                var childPos = HandleUtility.WorldToGUIPoint(child.position);

                posList.Add(pos);
                posList.Add(childPos);

                FindPositions(child, posList);
            }
        }
    }
}
#endif