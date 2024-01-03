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
    using Object = UnityEngine.Object;

    [CustomEditor(typeof(BaseUIMaterialPropUpdater),true)]
    public class BaseUIMaterialPropUpdaterEditor : PowerEditor<BaseUIMaterialPropUpdater>
    {

        public override void DrawInspectorUI(BaseUIMaterialPropUpdater inst)
        {
            if (inst.graphs != null && inst.graphs.Length == 0&&  PrefabTools.IsOuterPrefabMode(target))
            {
                PrefabTools.ModifyPrefab(inst.gameObject, (go) => {
                    var updater = go.GetComponent<BaseUIMaterialPropUpdater>();
                    updater?.Setup();
                });

            }
        }

        public override bool NeedDrawDefaultUI() => true;
        public override string Version => "v(0.0.4.6)";
    }
#endif
    /// <summary>
    /// ui components(Graphic) use material clone
    /// Renderers use MaterialPropertyBlock update variables
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public abstract class BaseUIMaterialPropUpdater : MonoBehaviour, IMaterialModifier
    {
        [Header("UI Graphs")]
        public Graphic[] graphs;

        // keep for compatible
        //[HideInInspector]
        [SerializeField] List<Material> graphSharedMaterialList = new List<Material>();

        //[HideInInspector]
        public List<Material> graphCachedMaterialList = new List<Material>();

        [Header("Renderers")]
        public Renderer[] renderers;
        
        static MaterialPropertyBlock rendererBlock;
        // private vars
        [HideInInspector]
        [SerializeField] bool isFirstMaterialReaded;

        void Awake()
        {
            Setup();
        }

        public void Setup()
        {
            graphCachedMaterialList.Clear();

            SetupRenderers(out var isRenderersValid, out var isGraphsValid);
            enabled = isRenderersValid || isGraphsValid;
            ReadMaterial(enabled, isRenderersValid);
        }

        [ContextMenu("Reset")]
        private void OnReset()
        {
            //RestoreGraphsMaterial();
            //if (Application.isEditor)
            //    Start();
        }

        private void LateUpdate()
        {
            if (rendererBlock == null)
                rendererBlock= new MaterialPropertyBlock();

            for (int i = 0; i < graphs.Length; i++)
            {
                var graph = graphs[i];
                UpdateGraphMaterial(graph, i);
            }

            for (int i = 0;i < renderers.Length; i++)
            {
                rendererBlock.Clear();

                var render = renderers[i];
                render.GetPropertyBlock(rendererBlock);
                UpdateBlock(rendererBlock);
                render.SetPropertyBlock(rendererBlock);
            }

            //MaterialPropCodeGenTools.UpdateComponentsMaterial(graphs, (graph, id) =>
            //{
                
            //});
            //MaterialPropCodeGenTools.UpdateComponentsMaterial(renderers, (render, id) =>
            //{
            //});
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

        void ReadMaterial(bool isValid,bool isRenderersValid)
        {
            if (isValid && !isFirstMaterialReaded)
            {
                RestoreGraphMatFromShared();

                var firstMat = isRenderersValid ? renderers[0].sharedMaterial : graphs[0].materialForRendering;
                if (firstMat)
                    ReadFirstMaterial(firstMat);

                isFirstMaterialReaded = true;
            }
        }

        public bool RestoreGraphMatFromShared()
        {
            if (graphSharedMaterialList != null 
                && graphSharedMaterialList.Count > 0
                && graphs.Length>0
                && graphs[0].material == graphs[0].defaultMaterial
                )
            {
                graphs[0].material = graphSharedMaterialList[0];
                graphSharedMaterialList.Clear();
                return true;
            }
            return false;
        }

        private void UpdateGraphMaterial(Graphic graph,int id)
        {
            if (!graph
                ||  graph.materialForRendering == graph.defaultMaterial // dont update default material
                )
                return;

            UpdateMaterial(graph.materialForRendering);
        }

        public abstract void ReadFirstMaterial(Material mat);

        public abstract void UpdateMaterial(Material mat);
        public abstract void UpdateBlock(MaterialPropertyBlock block);

        public virtual void ReadFirstMaterialTextures(Material mat) { }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (graphCachedMaterialList.Count == 0
                || !graphCachedMaterialList[0]
                || baseMaterial.shader != graphCachedMaterialList[0].shader)
            {
                graphCachedMaterialList.Clear();
                graphCachedMaterialList.Add(Instantiate(baseMaterial));
            }

            return graphCachedMaterialList[0];
        }
    }
}
