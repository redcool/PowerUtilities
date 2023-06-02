using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(BaseUIMaterialPropUpdater),true)]
    public class BaseUIMaterialPropUpdaterEditor : PowerEditor<BaseUIMaterialPropUpdater>
    {
        public override void DrawInspectorUI(BaseUIMaterialPropUpdater inst)
        {
        }

        public override bool NeedDrawDefaultUI() => true;
        private void OnEnable()
        {
            version = "v(0.0.4)";
        }
    }
#endif
    /// <summary>
    /// ui components(Graphic) use material clone
    /// Renderers use MaterialPropertyBlock update variables
    /// </summary>
    [ExecuteInEditMode]
    public abstract class BaseUIMaterialPropUpdater : MonoBehaviour
    {
        [Header("UI Graphs")]
        public Graphic[] graphs;
        [SerializeField]List<Material> graphSharedMaterialList = new List<Material>();

        [Header("Renderers")]
        public Renderer[] renderers;
        
        static MaterialPropertyBlock rendererBlock;
        // private vars
        //[HideInInspector]
        [SerializeField] bool isFirstMaterialReaded;

        private void Start()
        {
            SetupRenderers(out var isRenderersValid,out var isGraphsValid);

            enabled = isRenderersValid || isGraphsValid;

            ReadMaterial(enabled, isRenderersValid);

            var needInitGraphs = graphSharedMaterialList.Count == 0;
            if(needInitGraphs)
                SaveGraphMaterials();

            InstantiateGraphMaterials();
        }

        void SetupRenderers(out bool isRenderersValid,out bool isGraphsValid)
        {
            if (graphs == null || graphs.Length == 0)
            {
                if (gameObject.TryGetComponent<Graphic>(out var comp))
                    graphs = new[] { comp };
            }

            if (renderers == null || renderers.Length == 0)
            {
                if (gameObject.TryGetComponent<Renderer>(out var comp))
                    renderers = new[] { comp };
            }

            isRenderersValid = (renderers != null && renderers.Length > 0);
            isGraphsValid = (graphs != null && graphs.Length > 0);
        }

        void InstantiateGraphMaterials()
        {
            graphs.ForEach((g, id) =>
            {
                var targetMat = graphSharedMaterialList[id] ?? g.material;
                if (targetMat && !g.material.name.Contains("Clone"))
                    g.material = Instantiate(targetMat);
            });
        }

        private void SaveGraphMaterials()
        {
            graphs.ForEach((g) => graphSharedMaterialList.Add(g.material));
        }

        void ReadMaterial(bool isValid,bool isRenderersValid)
        {
            if (isValid && !isFirstMaterialReaded)
            {
                var firstMat = isRenderersValid ? renderers[0].sharedMaterial : graphs[0].materialForRendering;
                if (firstMat)
                    ReadFirstMaterial(firstMat);

                isFirstMaterialReaded = true;
            }
        }

        private void LateUpdate()
        {
            if (rendererBlock == null)
                rendererBlock= new MaterialPropertyBlock();

            MaterialPropCodeGenTools.UpdateComponentsMaterial(graphs, (graph, id) =>
            {
                UpdateGraphMaterial(graph, id);
            });
            MaterialPropCodeGenTools.UpdateComponentsMaterial(renderers, (render, id) =>
            {
                rendererBlock.Clear();
                render.GetPropertyBlock(rendererBlock);
                UpdateBlock(rendererBlock);
                render.SetPropertyBlock(rendererBlock);
            });

        }

        private void UpdateGraphMaterial(Graphic graph, int id)
        {
            // get material instance

            if (graphSharedMaterialList.Count <= id)
                return;

            TryInitGraphMat(graph, id);

            // dont update default material
            if (graph.materialForRendering == graph.defaultMaterial)
                return;

            UpdateMaterial(graph.materialForRendering);
        }

        void TryInitGraphMat(Graphic graph,int id)
        {
            var targetMat = graphSharedMaterialList[id];
            if (!targetMat)
                targetMat = graphSharedMaterialList[id] = graph.defaultMaterial;

            if (graph.material == graph.defaultMaterial
            || graph.material.shader != targetMat.shader
            )
            {
                graph.material = Instantiate(targetMat);
            }
        }

        void RestoreGraphsMaterial()
        {
            graphs.ForEach((g, id) =>
            {
                if (graphSharedMaterialList.Count > id)
                    g.material = graphSharedMaterialList[id];
            });
            graphSharedMaterialList.Clear();
        }

        private void OnDestroy()
        {
            RestoreGraphsMaterial();
            rendererBlock =null;
        }

        [ContextMenu("Reset")]
        private void OnReset()
        {
            //RestoreGraphsMaterial();
            //if (Application.isEditor)
            //    Start();
        }

        public abstract void ReadFirstMaterial(Material mat);

        public abstract void UpdateMaterial(Material mat);
        public abstract void UpdateBlock(MaterialPropertyBlock block);
    }
}
