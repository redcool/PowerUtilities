#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PowerUtilities;
using System;
using Object = UnityEngine.Object;
using System.Linq;
using PlasticGui;

namespace GameUtilsFramework
{

    public class SkeletonSceneViewTool
    {
        static SkeletonSceneViewToolData skeletonToolData = null;
        public static SkeletonSceneViewToolData SkeletonToolData
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

        /// <summary>
        /// draw bone weights
        /// </summary>
        static Mesh snapshotMesh = new Mesh();

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
                SkeletonToolData.isSceneViewMouseDrag || 
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
            var e = Event.current;

                SetupMouseStates(e, ref SkeletonToolData.isSceneViewMouseDown, ref SkeletonToolData.isSceneViewMouseDrag, ref SkeletonToolData.isSceneViewMouseUp);
                SetupSelectionRect(e);

            TryFindSkinned();
            SetupBonesScreenPos();

            if (SkeletonToolData.enable)
            {
                if (SkeletonToolData.isShowWeights)
                    DrawWeights(SkeletonToolData);
            }

            DrawGUI();
        }

        private static void DrawGUI()
        {
            Handles.BeginGUI();
            DrawToolbar(SkeletonToolData);


            if (SkeletonToolData.enable)
            {

                DrawSkeletonHierarchy(SkeletonToolData);

                RecordSelectedTransform(ref lastSelectedInfos);
                ReverseChildrenPos();

                TryRectSelect();
            }

            Handles.EndGUI();
        }

        static void SetupSelectionRect(Event e)
        {
            if(SkeletonToolData.isSceneViewMouseDown && Selection.activeObject == null)
            {
                SkeletonToolData.isRectSelectionStart = true;
                SkeletonToolData.selectionRect = new Rect(e.mousePosition.x,e.mousePosition.y,1,1);
            }
            if (SkeletonToolData.isSceneViewMouseUp)
            {
                SkeletonToolData.isRectSelectionStart = false;
            }

            if (SkeletonToolData.isSceneViewMouseDrag)
            {
                var lastRect = SkeletonToolData.selectionRect;
                var width = Mathf.Abs(e.mousePosition.x - lastRect.x);
                var height = Mathf.Abs(e.mousePosition.y - lastRect.y);
                var x = e.mousePosition.x < lastRect.x ? e.mousePosition.x : lastRect.x;
                var y = e.mousePosition.y < lastRect.y ? e.mousePosition.y : lastRect.y;
                SkeletonToolData.selectionRect = new Rect(x, y, width, height);

            }
        }

        static void TryRectSelect()
        {
            if (!SkeletonToolData.isRectSelectionStart)
                return;

            var lastRect = SkeletonToolData.selectionRect;
            var selectedIds = new List<int>();
            for (int i = 0; i < skeletonLinesList.Count; i++)
            {
                var pos = skeletonLinesList[i];
                if (lastRect.Contains(pos))
                {
                    selectedIds.Add(i);
                }
            }
            if(selectedIds.Count > 0)
                Selection.objects = selectedIds.Select(id => skeletonToolData.skinned.bones[id]).ToArray();
        }

        static void SetupMouseStates(Event e, ref bool isMouseDown, ref bool isMouseMove, ref bool isMouseUp)
        {
            isMouseDown = e.type == EventType.MouseDown;
            isMouseMove = (e.type == EventType.MouseDrag);
            isMouseUp = e.type == EventType.MouseUp;
        }

        static void TryFindSkinned()
        {
            if (Selection.activeGameObject)
            {
                var skinned = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();
                if (skinned)
                    SkeletonToolData.skinned =skinned;
            }
        }

        static void SetupBonesScreenPos()
        {
            if (SkeletonToolData.skinned)
            {
                SkeletonToolData.bonesScreenPosList.Clear();
                foreach (var boneTr in SkeletonToolData.skinned.bones)
                {
                    SkeletonToolData.bonesScreenPosList.Add(HandleUtility.WorldToGUIPoint(boneTr.position));
                }
            }
        }

        private static void RecordSelectedTransform(ref (Transform, Vector3)[] childrenInfo)
        {
            if (SkeletonToolData.isSceneViewMouseDown)
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

        private static void DrawToolbar(SkeletonSceneViewToolData data)
        {
            var toolbarWidth = data.enable ? EditorGUIUtility.currentViewWidth - 200 : 120;
            GUILayout.BeginArea(new Rect(50, 0, toolbarWidth, 24));
            GUILayout.BeginHorizontal(EditorStylesEx.ShurikenModuleBg);
            {
                data.enable = GUILayout.Toggle(data.enable, "SkeletonTool");
                if (data.enable)
                {
                    //data.skeletonObj = EditorGUILayout.ObjectField(data.skeletonObj, typeof(GameObject), true) as GameObject;
                    EditorGUILayout.ObjectField(data.skinned, typeof(SkinnedMeshRenderer), true);

                    data.isShowWeights = GUILayout.Toggle(data.isShowWeights, "Show Weights");
                    data.isShowHierarchy = GUILayout.Toggle(data.isShowHierarchy, "Show Hierarchy");
                    data.isKeepChildren = GUILayout.Toggle(data.isKeepChildren, "Keep Children");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private static void LoadDefaultSkeletonData(ref SkeletonSceneViewToolData data)
        {
            if (data)
                return;

            const string PATH = "Assets/PowerUtilities/SkeletonData.asset";
            var items = AssetDatabaseTools.FindAssetsInProject<SkeletonSceneViewToolData>("SkeletonData");
            if (items.Length == 0)
            {
                PathTools.CreateAbsFolderPath(PATH);
                AssetDatabase.Refresh();

                data = ScriptableObject.CreateInstance<SkeletonSceneViewToolData>();
                AssetDatabase.CreateAsset(data, PATH);
            }
            else
            {
                data = items[0];
            }
            if (!data.weightMat)
            {
                var shader = Shader.Find("Hidden/PowerUtilities/Unlit/ShowVertexColor");
                data.weightMat = new Material(shader);

                if (!shader)
                    throw new Exception("Hidden/PowerUtilities/Unlit/ShowVertexColor not found");
            }
        }

        private static void DrawSkeletonHierarchy(SkeletonSceneViewToolData data)
        {
            if (!data.isShowHierarchy)
                return;

            if (data.skinned)
            {
                data.skinned.bones.ForEach(b => {
                    if (b.parent && data.skinned.bones.Contains(b.parent)){
                        var p1 = HandleUtility.WorldToGUIPoint(b.position);
                        var p2 = HandleUtility.WorldToGUIPoint(b.parent.position);
                        Handles.DrawLine(p1,p2,3);
                    }
                }
                );
                DrawJoints(data.skinned.bones);
            }

            /* 
            if (!data.skeletonObj || !data.isShowHierarchy)
                return;
            FindSkeletonLines(data, skeletonLinesList);
            Handles.DrawLines(skeletonLinesList.ToArray());
            DrawJoints(data);
            */
        }
        private static void DrawJoints(SkeletonSceneViewToolData data)
        {
            var trs = data.skeletonObj.GetComponentsInChildren<Transform>();
            DrawJoints(trs);
        }
        private static void DrawJoints(Transform[] trs)
        {
            for (int i = 0; i < trs.Length; i++)
            {
                var tr = trs[i];
                var pos = SkeletonToolData.bonesScreenPosList[i];
                
                if (GUI.Button(new Rect(pos.x - 5, pos.y - 5, 10, 10), ""))
                {
                    Selection.activeTransform = tr;
                }
            }
        }

        private static void FindSkeletonLines(SkeletonSceneViewToolData data,List<Vector3> skeletonLinesList)
        {
            //if (data.isSceneViewMouseDrag || skeletonLinesList.Count == 0)
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

        static void DrawWeights(SkeletonSceneViewToolData data)
        {
            var boneTr = Selection.activeTransform;
            if (!data.skinned || !boneTr)
                return;

            var skinTr = data.skinned.transform;

            var weights = data.skinned.sharedMesh.boneWeights;
            var bones = data.skinned.bones;
            var boneId = bones.FindIndex(tr => tr == boneTr);

            if (!snapshotMesh) 
                snapshotMesh = new Mesh();

            data.skinned.BakeMesh(snapshotMesh);

            Color[] colors = GetVertexWeightColor(data, weights, boneId);
            snapshotMesh.colors = colors;

            data.weightMat.SetPass(0);
            Graphics.DrawMeshNow(snapshotMesh, skinTr.position, skinTr.rotation);
        }

        private static Color[] GetVertexWeightColor(SkeletonSceneViewToolData data, BoneWeight[] weights, int boneId)
        {
            var colors = new Color[weights.Length];
            for (int i = 0; i< weights.Length; i++)
            {
                var boneWeight = weights[i];
                var c = data.weightColor;
                c.a = 0;

                c.a += boneWeight.boneIndex0 == boneId ? boneWeight.weight0 : 0;
                c.a += boneWeight.boneIndex1 == boneId ? boneWeight.weight1 : 0;
                c.a += boneWeight.boneIndex2 == boneId ? boneWeight.weight2 : 0;
                c.a += boneWeight.boneIndex3 == boneId ? boneWeight.weight3 : 0;

                colors[i]  =c;
            }

            return colors;
        }

    }
}
#endif