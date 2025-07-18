#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;

namespace PowerUtilities.Scenes
{
    public class PlacementInfo{
        public Vector2 scale = new Vector2(0.1f,1f);
        public Vector2 rotation = new Vector2(0,359);
        public Transform target;
        public LayerMask colliderLayers = 1;

        public Transform minTr, maxTr;
        public float rayMaxHeight = 500;

        [Tooltip("random position 's radius")]
        public Vector3 radius = Vector3.one;

    }

    public class PlacementWindow : EditorWindow
    {
        readonly GUIContent
            ROOT_TEXT = new GUIContent("Root", "update children"),
            FELL_TO_GROUND = new GUIContent("Fell to ground", "子节点落地"),
            BOUNDS_RAND_POS_TEXT = new GUIContent("Bounds Position Random ", "框内随机子节点位置"),
            RAND_POS_TEXT = new GUIContent("PositionRandom","子节点位置随机微调"),
            RAND_RATOTE_TEXT = new GUIContent("Rotation Random","子节点随机旋转"),
            RAND_SCALE_TEXT = new GUIContent("Scale Random","子节点随机缩放")
            ;


        const string ROOT_PATH = CommonUXMLEditorWindow.ROOT_MENU + "/Scene";
        const string helpStr = @"
Hierarchy中对选中节点的子节点进行
随机缩放
随机旋转
放置到地面
        ";

        PlacementInfo info = new PlacementInfo();

        public List<Transform> rootChildrenList = new List<Transform>();
        private Vector2 scrollPos;

        [MenuItem(ROOT_PATH + "/"+nameof(PlacementWindow))]
        static void Init()
        {
            var win = GetWindow<PlacementWindow>();
            win.titleContent = new GUIContent(nameof(PlacementWindow));
            win.Show();
        }


        public void OnGUI()
        {
            SetupChildrenList();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.HelpBox(helpStr, MessageType.Info);

            EditorGUITools.BeginVerticalBox(() =>
            {
                EditorGUITools.BeginHorizontalBox(() =>
                {
                    info.target = (Transform)EditorGUILayout.ObjectField(ROOT_TEXT, info.target, typeof(Transform), true);
                    if (GUILayout.Button("this"))
                    {
                        info.target = Selection.activeTransform;
                    }
                });


                EditorGUILayout.LabelField("Bound Placements");
                EditorGUITools.BeginVerticalBox(() =>
                {
                    info.minTr = (Transform)EditorGUILayout.ObjectField("Min:", info.minTr, typeof(Transform), true);
                    info.maxTr = (Transform)EditorGUILayout.ObjectField("Max:", info.maxTr, typeof(Transform), true);

                    EditorGUI.BeginDisabledGroup(!info.target || !info.minTr || !info.maxTr);
                    if (GUILayout.Button(BOUNDS_RAND_POS_TEXT))
                    {
                        RandomDistribution(info.minTr.position, info.maxTr.position);
                    }
                    EditorGUI.EndDisabledGroup();
                }, indentLevelAdd: EditorGUI.indentLevel + 1);

                //---------------
                {
                    EditorGUI.BeginDisabledGroup(!info.target);
                    //==========Position
                    EditorGUILayout.LabelField("Position");
                    EditorGUITools.BeginVerticalBox(() =>
                    {
                        info.radius = EditorGUILayout.Vector3Field("Radius:",info.radius);
                        if (GUILayout.Button(RAND_POS_TEXT))
                        {
                            RandomPosition(info.radius);
                        }
                    }, indentLevelAdd: EditorGUI.indentLevel + 1);


                    //==========Scale
                    EditorGUILayout.LabelField("Scale");
                    EditorGUITools.BeginVerticalBox(() =>
                    {
                        //EditorGUIUtility.labelWidth = 100;
                        info.scale.x = EditorGUILayout.FloatField("min scale:", info.scale.x);
                        info.scale.y = EditorGUILayout.FloatField("max scale:", info.scale.y);
                        if (GUILayout.Button(RAND_SCALE_TEXT))
                        {
                            RandomScale(info.scale);
                        }
                    },indentLevelAdd:EditorGUI.indentLevel+1);

                    //==========Rotation
                    EditorGUILayout.LabelField("Rotation");
                    EditorGUITools.BeginVerticalBox(() =>
                    {
                        info.rotation.x = EditorGUILayout.FloatField("min rotation:", info.rotation.x);
                        info.rotation.y = EditorGUILayout.FloatField("max rotation:", info.rotation.y);
                        if (GUILayout.Button(RAND_RATOTE_TEXT))
                        {
                            RandomRotation(info.rotation);
                        }

                    }, indentLevelAdd: EditorGUI.indentLevel + 1);

                    //==========RayTrace
                    EditorGUILayout.LabelField("RayTrace");
                    EditorGUITools.BeginVerticalBox(() =>
                    {
                        info.colliderLayers = EditorGUITools.LayerMaskField("Collide Layer:", info.colliderLayers);
                        info.rayMaxHeight = EditorGUILayout.FloatField("Ray Max Height:", info.rayMaxHeight);

                        if (GUILayout.Button(FELL_TO_GROUND))
                        {
                            PutOnLand();
                        }
                    }, indentLevelAdd: EditorGUI.indentLevel + 1);

                    EditorGUI.EndDisabledGroup();
                }
            });

            EditorGUILayout.EndScrollView();
        }


        private void SetupChildrenList()
        {
            if (info.target)
                info.target.FindChildren(ref rootChildrenList);
        }

        private void OnFocus()
        {
            SceneView.duringSceneGui -= OnDrawScene;
            SceneView.duringSceneGui += OnDrawScene;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnDrawScene;
        }

        void OnDrawScene(SceneView scene)
        {
            if (info.minTr && info.maxTr)
            {
                var min = info.minTr.position;
                var max = info.maxTr.position;
                var size = (max - min);

                var center = size*0.5f + min;
                Handles.DrawWireCube(center,size);
            }
        }

        private void PutOnLand()
        {
            Func<Vector3, Vector3> GetCollidePoint = (pos) =>
            {
                var ray = new Ray(pos, Vector3.down);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue,info.colliderLayers))
                {
                    return hit.point;
                }
                return pos;
            };

            foreach (var r in rootChildrenList)
            {
                Undo.RecordObject(r, $"random position :{r.name}");

                var c = r.GetComponent<Collider>();
                if (c)
                    c.enabled = false;

                var pos = GetCollidePoint(r.position + new Vector3(0, info.rayMaxHeight,0));
                r.position = pos;

                if (c)
                    c.enabled = true;
            }
        }

        public void RandomDistribution(Vector3 min, Vector3 max)
        {
            foreach (var tr in rootChildrenList)
            {
                Undo.RecordObject(tr, $"RandomDistribution :{tr.name}");
                tr.position = RandomTools.Range(min, max);
            }
        }

        private void RandomRotation( Vector2 rotation)
        {
            foreach (var r in rootChildrenList)
            {
                Undo.RecordObject(r, $"RandomRotation :{r.name}");

                var rot = Random.Range(rotation.x, rotation.y);
                r.rotation = Quaternion.Euler(0, rot, 0);
            }
        }

        private void RandomScale(Vector2 scale)
        {
            foreach (var r in rootChildrenList)
            {
                Undo.RecordObject(r, $"RandomScale :{r.name}");

                var s = Random.Range(scale.x, scale.y);
                r.localScale = new Vector3(s, s, s);
            }
        }

        void RandomPosition(Vector3 radius)
        {
            foreach (var r in rootChildrenList)
            {
                Undo.RecordObject(r, $"random position :{r.name}");
                r.position += Vector3.Scale(Random.insideUnitSphere ,radius);
            }
        }
    }
}
#endif