#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Random = UnityEngine.Random;

namespace PowerUtilities.Scenes.Tools
{
    public class PlacementInfo{
        public Vector2 scale = new Vector2(0.1f,1f);
        public Vector2 rotation = new Vector2(0,359);
        public Transform target;
        public LayerMask colliderLayers = 1;

        public Transform minTr, maxTr;
        public float rayMaxHeight = 500;
    }
    public class PlacementWindow : EditorWindow
    {
        const string ROOT_PATH = "PowerUtilities/TransformTools";
        const string helpStr = @"
Hierarchy中对选中节点的子节点进行
随机缩放
随机旋转
放置到地面
        ";

        PlacementInfo info = new PlacementInfo();

        [MenuItem(ROOT_PATH + "/放置")]
        static void Init()
        {
            var win = GetWindow<PlacementWindow>();
            win.titleContent = new GUIContent("场景放置");
            win.Show();
        }

        void OnSelectionChange()
        {
            Repaint();
        }

        public static void RandomDistribution(Transform[] trs,Vector3 min,Vector3 max)
        {
            foreach (var tr in trs)
            {
                tr.position = RandomTools.Range(min,max);
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.HelpBox(helpStr, MessageType.Info);

            EditorGUITools.BeginVerticalBox(() =>
            {

                EditorGUITools.BeginHorizontalBox(() => {
                    info.target = (Transform)EditorGUILayout.ObjectField("Target: ", info.target, typeof(Transform), true);
                    if (GUILayout.Button("this"))
                    {
                        info.target = Selection.activeTransform;
                    }
                });

                EditorGUITools.BeginVerticalBox(()=> {
                    info.minTr = (Transform)EditorGUILayout.ObjectField("Min:",info.minTr,typeof(Transform),true); 
                    info.maxTr = (Transform)EditorGUILayout.ObjectField("Max:", info.maxTr, typeof(Transform), true);

                    EditorGUI.BeginDisabledGroup(!info.target || !info.minTr || !info.maxTr);

                    if (GUILayout.Button("框内随机放置"))
                    {
                        RandomDistribution(info.target.GetComponentsInChildren<Transform>(),info.minTr.position,info.maxTr.position);
                    }
                    EditorGUI.EndDisabledGroup();

                });

                EditorGUITools.BeginVerticalBox(() =>
                {
                    //EditorGUIUtility.labelWidth = 100;
                    info.scale.x = EditorGUILayout.FloatField("min scale:", info.scale.x);
                    info.scale.y = EditorGUILayout.FloatField("max scale:", info.scale.y);
                    if (GUILayout.Button("随机缩放"))
                    {
                        RandomScale(info.target, info.scale);
                    }
                }
                );

                EditorGUITools.BeginVerticalBox(() =>
                {
                    info.rotation.x = EditorGUILayout.FloatField("min rotation:", info.rotation.x);
                    info.rotation.y = EditorGUILayout.FloatField("max rotation:", info.rotation.y);
                    if (GUILayout.Button("随机旋转"))
                    {
                        RandomRotation(info.target, info.rotation);
                    }

                });


                EditorGUITools.BeginVerticalBox(() =>
                {
                    info.colliderLayers = EditorGUITools.LayerMaskField("Collide Layer:", info.colliderLayers);
                    info.rayMaxHeight = EditorGUILayout.FloatField("Ray Max Height:", info.rayMaxHeight);

                    if (GUILayout.Button("放置到地面"))
                    {
                        PutOnLand(info.target);
                    }
                });
            });

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

        private void PutOnLand(Transform tr)
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

            var children = tr.GetComponentsInChildren<Renderer>();

            foreach (var r in children)
            {
                Transform item = r.transform;
                var c = item.GetComponent<Collider>();
                if (c)
                    c.enabled = false;

                var pos = GetCollidePoint(item.position + new Vector3(0, info.rayMaxHeight,0));
                item.position = pos;

                if (c)
                    c.enabled = true;
            }
        }

        private void RandomRotation(Transform tr, Vector2 rotation)
        {
            var children = tr.GetComponentsInChildren<Renderer>();
            foreach (var r in children)
            {
                Transform item = r.transform;
                var rot = Random.Range(rotation.x, rotation.y);
                item.rotation = Quaternion.Euler(0, rot, 0);
            }
        }

        private void RandomScale(Transform tr, Vector2 scale)
        {
            var children = tr.GetComponentsInChildren<Renderer>();
            foreach (var r in children)
            {
                Transform item = r.transform;
                var s = Random.Range(scale.x, scale.y);
                item.localScale = new Vector3(s, s, s);
            }
        }
    }
}
#endif