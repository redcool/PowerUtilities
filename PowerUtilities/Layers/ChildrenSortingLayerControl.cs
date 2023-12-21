namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Text;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(ChildrenSortingLayerControl))]
    public class ChildrenSortingLayerControlEditor : PowerEditor<ChildrenSortingLayerControl>
    {
        //public override bool NeedDrawDefaultUI() => true;

        public override void DrawInspectorUI(ChildrenSortingLayerControl inst)
        {
            EditorGUILayout.BeginVertical("Box");
            
            DrawDefaultInspector();

            if (GUILayout.Button("Sort Children"))
            {
                inst.StartSortChildren();
            }
            EditorGUILayout.EndVertical();

            if (inst.children == null)
                return;

            DrawStatisticsInfo(inst);
        }

        private void DrawStatisticsInfo(ChildrenSortingLayerControl inst)
        {
            inst.isFoldStatistics = EditorGUILayout.BeginFoldoutHeaderGroup(inst.isFoldStatistics, "StatisticsInfo");
            if (!inst.isFoldStatistics)
                return;

            for (int i = 0; i < inst.children.Length; i++)
            {
                var child = inst.children[i];
                var childInfo = inst.childrenSortingLayerNames[i];

                EditorGUILayout.BeginHorizontal("Box");
                EditorGUILayout.ObjectField(child, child.GetType(), true);
                EditorGUILayout.LabelField(childInfo);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
#endif
    /// <summary>
    /// Set startSortingOrder 
    /// 
    /// Canvas
    /// ParticleSystem
    /// SpriteRenderer
    /// 
    /// </summary>
    [ExecuteAlways]
    public class ChildrenSortingLayerControl : MonoBehaviour
    {
        [HelpBox()] public string HELP_STR = "Setup children's sortingLayer by children's order";
        [SortingLayerIndex] public int sortingLayerId = 0;

        [HideInInspector] public Transform[] children;
        [HideInInspector] public string[] childrenSortingLayerNames;
        [HideInInspector] public bool isFoldStatistics;
        /// <summary>
        /// sorting index in this group
        /// </summary>
        int startSortingOrder = 0;

#if UNITY_EDITOR
        private void Update()
        {
            if (transform.hasChanged)
            {
                StartSortChildren();
            }
        }

#endif

        public void StartSortChildren()
        {
            startSortingOrder = 0;
            //children = GetComponentsInChildren<Transform>();
            children = new Transform[transform.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = transform.GetChild(i);
            }

            childrenSortingLayerNames = new string[children.Length];

            SetupChildrenSortingOrder(children, childrenSortingLayerNames);
        }

        public static SortingLayer GetSortingLayer(int id)
        {
            if (id >= SortingLayer.layers.Length)
                id = SortingLayer.layers.Length - 1;
            return SortingLayer.layers[id];
        }

        public void SetupChildrenSortingOrder(Transform[] children,string[] childrenSortingLayerNames)
        {
            var sortingLayer = GetSortingLayer(sortingLayerId);

            var sb = new StringBuilder();

            for (int i = 0; i < children.Length; i++)
            {
                var tr = children[i];
                var canvas = tr.GetComponent<Canvas>();
                var ps = tr.GetComponent<ParticleSystem>();
                var sr = tr.GetComponent<SpriteRenderer>();

                if (!canvas && !ps && !sr)
                {
                    continue;
                }

                if (canvas)
                {
                    canvas.sortingLayerID = sortingLayer.id;
                    canvas.sortingOrder = startSortingOrder;

                    startSortingOrder++;

                    sb.Append($"{canvas.sortingOrder}");
                }

                if (ps)
                {
                    var psr = ps.GetComponent<ParticleSystemRenderer>();
                    psr.sortingLayerID = sortingLayer.id;
                    psr.sortingOrder = startSortingOrder;
                    startSortingOrder++;

                    sb.Append($"{psr.sortingOrder}");
                }

                if (sr)
                {
                    sr.sortingLayerID = sortingLayer.id;
                    sr.sortingOrder = startSortingOrder;
                    startSortingOrder++;

                    sb.Append($"{sr.sortingOrder}");
                }

                // statistics
                sb.Insert(0,$"{sortingLayer.name} , ");
                childrenSortingLayerNames[i] = sb.ToString();
                sb.Clear();
            }
        }
    }
}
