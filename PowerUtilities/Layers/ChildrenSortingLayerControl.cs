namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(ChildrenSortingLayerControl))]
    public class ChildrenSortingLayerControlEditor : PowerEditor<ChildrenSortingLayerControl>
    {
        public override bool NeedDrawDefaultUI() => true;

        public override void DrawInspectorUI(ChildrenSortingLayerControl inst)
        {
            if (GUILayout.Button("Sort Children"))
            {
                inst.OnEnable();
            }
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
    public class ChildrenSortingLayerControl : MonoBehaviour
    {
        [HelpBox()] public string HELP_STR = "Setup children's sortingLayer by children's order";
        [SortingLayerIndex] public int sortingLayerId = 0;

        public Transform[] children;
        /// <summary>
        /// sorting index in this group
        /// </summary>
        int startSortingOrder = 0;
        public void OnEnable()
        {
            startSortingOrder = 0;
            //children = GetComponentsInChildren<Transform>();
            children = new Transform[transform.childCount];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = transform.GetChild(i);
            }

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
            sortingLayerId = sortingLayer.id;

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
                }

                if (ps)
                {
                    var psr = ps.GetComponent<ParticleSystemRenderer>();
                    psr.sortingLayerID = sortingLayer.id;
                    psr.sortingOrder = startSortingOrder;
                    startSortingOrder++;
                }

                if (sr)
                {
                    sr.sortingLayerID = sortingLayer.id;
                    sr.sortingOrder = startSortingOrder;
                    startSortingOrder++;
                }
            }
        }
    }
}
