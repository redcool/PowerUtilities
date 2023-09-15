namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Rendering;
    using System;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(GammaUIParticleControl))]
    public class GammaUIParticleEditor : PowerEditor<GammaUIParticleControl>
    {
        public override bool NeedDrawDefaultUI() =>true;
        public override string Version => "2.0.1";
        public override void DrawInspectorUI(GammaUIParticleControl inst)
        {
            EditorGUILayout.HelpBox(nameof(GammaUIParticleControl)+", control ugui children particle", MessageType.Info);

            if (inst.uiMaterial && GUILayout.Button("Setup"))
            {
                inst.SetupUIGraph();
                inst.SetupParticles();

                inst.SetParticleSystemRendererMaterails();
            }
        }

    }
#endif
    [Serializable]
    public class ParticleRenderMaterialInfo
    {
        public Material mainMat, trailMat;
    }


    [CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class GammaUIParticleControl : MonoBehaviour//,IMaterialModifier
    {
        [Header("UI")]
        public Material uiMaterial;
        public bool useMaterialInstance;


        [Header("Particles")]
        [LayerIndex] public int particleLayer;
        public bool autoSetParticleStencilOps = true;


        [Header("Stencil")]
        public string stencilName = "_Stencil";
        [Range(0,255)]public int stencilValue = 1;
        public string stencilComp = "_StencilComp";
        public string stencilOp = "_StencilOp";

        [Header("Debug")]
        public Graphic uiGraph;
        public Material uiMaterialInst;
        //public ParticleSystem[] particles;
        public ParticleSystemRenderer[] particleRenderers;
        public List<ParticleRenderMaterialInfo> particleRenderMaterialInfoList = new List<ParticleRenderMaterialInfo>();


        public void OnEnable()
        {
            SetParticleSystemRendererMaterails();
        }

#if UNITY_EDITOR
        public void Update()
        {
            SetParticleSystemRendererMaterails();
        }
#endif
        public void SetParticleSystemRendererMaterails()
        {
            SetMaterialValue(uiMaterialInst,false);

            foreach (var matInfo in particleRenderMaterialInfoList)
            {
                SetMaterialValue(matInfo.mainMat, autoSetParticleStencilOps);
                SetMaterialValue(matInfo.trailMat, autoSetParticleStencilOps);
            }

            void SetMaterialValue(Material mat,bool isSetStencilOpComp=true)
            {
                if (mat == null)
                    return;

                mat.SetFloat(stencilName, stencilValue);
                if (isSetStencilOpComp)
                {
                    mat.SetFloat(stencilComp, (int)CompareFunction.Equal);
                    mat.SetFloat(stencilOp, (int)StencilOp.Keep);
                }
            }
        }

        public void SetupUIGraph()
        {
            if (!uiGraph)
            {
                uiGraph = GetComponent<Graphic>();
            }
            if (!uiMaterial)
                return;

            uiMaterialInst = uiMaterial;
            if (useMaterialInstance)
            {
                uiMaterialInst = Instantiate(uiMaterial);
            }
            uiMaterialInst.SetFloat(stencilName, stencilValue);

            uiGraph.material = uiMaterialInst;
        }

        public void SetupParticles()
        {
            //particles = GetComponentsInChildren<ParticleSystem>();
            particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>();

            particleRenderMaterialInfoList.Clear();

            for (int i = 0; i < particleRenderers.Length; i++)
            {
                //var p = particles[i];
                var pr = particleRenderers[i];
                pr.gameObject.layer = particleLayer;

                var matInfo = new ParticleRenderMaterialInfo
                {
                    mainMat = GetClonedMat(pr.sharedMaterial),
                    trailMat = GetClonedMat(pr.trailMaterial)
                };
                particleRenderMaterialInfoList.Add(matInfo);

                pr.material = matInfo.mainMat;
                pr.trailMaterial = matInfo.trailMat;
            }

            Material GetClonedMat(Material originalMat)
            {
                if (originalMat == null)
                    return null;
                if (originalMat.name.Contains("(Clone)"))
                    originalMat.name = originalMat.name.Substring(0, originalMat.name.Length-7);
                return Instantiate(originalMat);
            }
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!uiMaterialInst)
            {
                SetupUIGraph();
            }
            uiMaterialInst.SetFloat(stencilName, stencilValue);
            return uiMaterialInst;
        }
    }
}
