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

        static (Transform tr, Vector3 pos)[] lastSelectedInfos;

        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui -= SceneView_duringSceneGui;
            SceneView.duringSceneGui += SceneView_duringSceneGui;

        }

        private static void ReverseChildrenPos(Event e)
        {
            var tr = Selection.activeTransform;
            if (e.type != EventType.MouseDrag || 
                !tr || 
                lastSelectedInfos == null ||
                !skeletonToolData.isKeepChildren)

                return;

            foreach (var info in lastSelectedInfos)
            {
                info.tr.position = info.pos;
            }
        }

        private static void SceneView_duringSceneGui(SceneView view)
        {
            LoadDefaultSkeletonData(ref skeletonToolData);


            Handles.BeginGUI();
            DrawToolbar(skeletonToolData);

            if (skeletonToolData.enable)
            {
                DrawSkeletonHierarchy(skeletonToolData);

                var e = Event.current;
                RecordSelectedTransform(e, ref lastSelectedInfos);
                ReverseChildrenPos(e);
            }

            Handles.EndGUI();

        }

        private static void RecordSelectedTransform(Event e, ref (Transform, Vector3)[] childrenInfo)
        {
            if (e.type == EventType.MouseDown && e.isMouse)
            {
                var lastTr = Selection.activeTransform;
                if (!lastTr) return;

                childrenInfo = lastTr.GetComponentsInChildren<Transform>()
                    .Where(tr => tr != lastTr)
                    .Select(tr => (tr, tr.position))
                    .ToArray();
            }
        }

        private static void DrawToolbar(SkeletonToolData data)
        {
            var toolbarWidth = data.enable ? EditorGUIUtility.currentViewWidth - 100 : 120;
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

            var trs = data.skeletonObj.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trs.Length; i++)
            {
                var tr = trs[i];
                var pos = HandleUtility.WorldToGUIPoint(tr.position);
                if(GUI.Button(new Rect(pos.x - 5, pos.y - 5, 10, 10), "1"))
                {
                    Selection.activeTransform = tr;
                }
                

                if (i < trs.Length - 1)
                {
                    var nextTr = trs[i + 1];
                    var nextPos = HandleUtility.WorldToGUIPoint(nextTr.position);

                    Handles.DrawLine(pos, nextPos);
                }
            }
        }
    }
}
#endif