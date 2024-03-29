namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using System;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(DrawChildrenStatic))]
    public class DrawChildrenStaticEditor : PowerEditor<DrawChildrenStatic>
    {

        public override bool NeedDrawDefaultUI()
        {
            return true;
        }
        public override void DrawInspectorUI(DrawChildrenStatic inst)
        {
            if (!inst.rootGo)
            {
                EditorGUILayout.HelpBox("assign a RootGO", MessageType.Warning);
                return;
            }


            if (GUILayout.Button("Setup Children"))
            {
                inst.SetupChildren();
                inst.CreateCombinedParents();
            }

        }
    }
#endif

    [Serializable]
    public class DrawChildrenStaticInfo
    {
        public GameObject[] gos;
    }

    public class DrawChildrenStatic : MonoBehaviour
    {
        public GameObject rootGo;

        [SerializeField] List<DrawChildrenStaticInfo> infoList = new List<DrawChildrenStaticInfo>();
        /// <summary>
        /// combine children (mesh) to this parents
        /// </summary>
        [SerializeField] List<GameObject> parentGos = new List<GameObject>();

        Transform combineTarget;
        const string COMBINE_TARGET = "_CombineTarget";

        public void Awake()
        {
            for (int i = 0; i < parentGos.Count; i++)
            {
                var go = parentGos[i];
                var childrenGos = infoList[i].gos;
                StaticBatchingUtility.Combine(childrenGos, go);
            }
        }

        public void SetupChildren()
        {
            infoList.Clear();

            if (!rootGo)
            {
                rootGo = gameObject;
            }

            var renders = rootGo.GetComponentsInChildren<Renderer>(true);
            var groups = renders.Where(r => !r.gameObject.CompareTag(Tags.EditorOnly))
                .GroupBy(r => r.sharedMaterial);

            foreach (var group in groups)
            {
                var groupRenders = new GameObject[group.Count()];

                infoList.Add(new DrawChildrenStaticInfo
                {
                    gos = group.Select(r => r.gameObject).ToArray(),
                });
            }

            SetupCombineTarget();
        }

        private void SetupCombineTarget()
        {
            combineTarget = rootGo.transform.Find(COMBINE_TARGET);
            if (!combineTarget)
            {
                combineTarget = new GameObject(COMBINE_TARGET).transform;
                combineTarget.SetParent(rootGo.transform, false);
            }
        }

        public void CreateCombinedParents()
        {
            if (!combineTarget)
                return;

            parentGos.Clear();
            combineTarget.gameObject.DestroyChildren<Transform>(true);

            foreach (var info in infoList)
            {
                var go = new GameObject("Group: "+ info.gos.Count());
                parentGos.Add(go);
                go.transform.SetParent(combineTarget, true);
            }

            //void CloneGroup(DrawChildrenStaticInfo info, GameObject go)
            //{
            //    foreach (var child in info.gos)
            //    {
            //        var c = Instantiate(child, go.transform);
            //        c.transform.position = child.transform.position;
            //        c.transform.rotation = child.transform.rotation;
            //        c.transform.localScale = child.transform.lossyScale;
            //    }
            //}

        }

    }
}
