#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Random = UnityEngine.Random;
using UnityEditorInternal;

namespace PowerUtilities.Scenes.Tools
{
    public class PlacementInfo{
        public Vector2 scale = new Vector2(0.1f,1f);
        public Vector2 rotation = new Vector2(0,359);
        public Transform target;
        public LayerMask colliderLayers = 1;
    }
    public class PlacementWindow : EditorWindow
    {
        const string ROOT_PATH = "Game/Editor工具";
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

        public void OnGUI()
        {
            EditorGUILayout.HelpBox(helpStr, MessageType.Info);

            var target = Selection.activeTransform;
            if (!target)
            {
                EditorGUILayout.HelpBox("Hierarchy先选中一个节点", MessageType.Warning);
                return;
            }

            info.target = target;

            EditorGUITools.BeginVerticalBox(() =>
            {
                EditorGUILayout.ObjectField("Target : ", target, typeof(Transform), true);

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

                    if (GUILayout.Button("放置到地面"))
                    {
                        PutOnLand(info.target);
                    }
                });
            });

        }

        private void PutOnLand(Transform tr)
        {
            Func<Vector3, Vector3> GetCollidePoint = (pos) =>
            {
                var ray = new Ray(pos + new Vector3(0, 1, 0), Vector3.down);

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

                var pos = GetCollidePoint(item.position);
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