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
            if (inst.graphs.Length == 0&&  PrefabTools.IsOuterPrefabMode(target))
            {
                PrefabTools.ModifyPrefab(inst.gameObject, (go) => {
                    var updater = go.GetComponent<BaseUIMaterialPropUpdater>();
                    updater?.Setup();
                });

            }
        }

        public override bool NeedDrawDefaultUI() => true;
        private void OnEnable()
        {
            version = "v(0.0.4.3)";
        }
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
                var firstMat = isRenderersValid ? renderers[0].sharedMaterial : graphs[0].materialForRendering;
                if (firstMat)
                    ReadFirstMaterial(firstMat);

                isFirstMaterialReaded = true;
            }
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

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (graphCachedMaterialList.Count == 0 || !graphCachedMaterialList[0])
                graphCachedMaterialList.Add(Instantiate(baseMaterial));
            
            return graphCachedMaterialList[0];
        }
    }
}
