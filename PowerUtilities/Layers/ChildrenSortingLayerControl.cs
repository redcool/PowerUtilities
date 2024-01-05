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

        public override string Version => "0.0.2";

        public override void DrawInspectorUI(ChildrenSortingLayerControl inst)
        {
            EditorGUILayout.BeginVertical("Box");
            
            DrawDefaultInspector();

            if (GUILayout.Button("Sort Children"))
            {
                inst.StartSortChildren();
            }

            EditorGUILayout.EndVertical();

            if (inst.sortedChildList == null)
                return;

            DrawStatisticsInfo(inst);
        }

        private void DrawStatisticsInfo(ChildrenSortingLayerControl inst)
        {
            inst.isFoldStatistics = EditorGUILayout.BeginFoldoutHeaderGroup(inst.isFoldStatistics, "StatisticsInfo");
            if (!inst.isFoldStatistics)
                return;

            for (int i = 0; i < inst.sortedChildList.Count; i++)
            {
                var child = inst.sortedChildList[i];
                var childInfo = inst.sortedChildInfo[i];

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
    /// Set sortingOrder 
    /// 
    /// Canvas
    /// ParticleSystem
    /// SpriteRenderer
    /// 
    /// </summary>
    [ExecuteAlways]
    public class ChildrenSortingLayerControl : MonoBehaviour
    {
        [HelpBox()] public string helpStr = "Auto setup children's sortingLayer by children's order";


        [Tooltip("group's sorting layer")]
        [SortingLayerIndex] public int sortingLayerId = 0;

        /// <summary>
        /// all children 
        /// </summary>
        [HideInInspector] public List<Transform> childList = new List<Transform>();
        /// <summary>
        /// only sorted children
        /// </summary>
        [HideInInspector] public List<Transform> sortedChildList = new List<Transform>();
        /// <summary>
        /// sorted children's sorting info
        /// </summary>
        [HideInInspector] public List<string> sortedChildInfo = new List<string>();
        [HideInInspector] public bool isFoldStatistics;

        [Header("Start Sorting Index")]
        [Tooltip("sortingLayerId get from base canvas's soringIndex")]
        public Canvas baseCanvas;
        [Tooltip("base sorting index")]
        public int startSortingOrder = 0;
        /// <summary>
        /// sorting index in this group
        /// </summary>
        int sortingOrder = 0;

        private void OnTransformChildrenChanged()
        {
            StartSortChildren();
        }

        public void StartSortChildren()
        {
            if (baseCanvas)
            {
                startSortingOrder = baseCanvas.sortingOrder;
            }
            // reset
            sortingOrder = startSortingOrder;
            childList.Clear();

            sortedChildInfo.Clear();
            sortedChildList.Clear();

            // sorting
            gameObject.FindChildrenRecursive(ref childList);

            SetupChildrenSortingOrder();
        }

        public static SortingLayer GetSortingLayer(int id)
        {
            if (id >= SortingLayer.layers.Length)
                id = SortingLayer.layers.Length - 1;
            return SortingLayer.layers[id];
        }

        public void SetupChildrenSortingOrder()
        {
            var sortingLayer = GetSortingLayer(sortingLayerId);

            var sb = new StringBuilder();

            for (int i = 0; i < childList.Count; i++)
            {
                var tr = childList[i];
                var canvas = tr.GetComponent<Canvas>();
                var ps = tr.GetComponent<ParticleSystem>();
                var sr = tr.GetComponent<SpriteRenderer>();

                if (!canvas && !ps && !sr)
                {
                    continue;
                }

                if (canvas)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingLayerID = sortingLayer.id;
                    canvas.sortingOrder = sortingOrder;
                    sortingOrder++;

                    sb.Append($"{canvas.sortingOrder}");
                }

                if (ps)
                {
                    var psr = ps.GetComponent<ParticleSystemRenderer>();
                    psr.sortingLayerID = sortingLayer.id;
                    psr.sortingOrder = sortingOrder;
                    sortingOrder++;

                    sb.Append($"{psr.sortingOrder}");
                }

                if (sr)
                {
                    sr.sortingLayerID = sortingLayer.id;
                    sr.sortingOrder = sortingOrder;
                    sortingOrder++;

                    sb.Append($"{sr.sortingOrder}");
                }

                // statistics
                sortedChildList.Add(tr);

                sb.Insert(0,$"{sortingLayer.name} , ");
                sortedChildInfo.Add(sb.ToString());
                sb.Clear();
            }
        }
    }
}
