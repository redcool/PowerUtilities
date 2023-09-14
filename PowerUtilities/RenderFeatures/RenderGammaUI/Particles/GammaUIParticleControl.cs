namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using PowerUtilities;
    using UnityEngine.UI;

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
                inst.Setup();
                inst.SetupParticles();
            }
        }

    }
#endif

    [CanEditMultipleObjects]
    public class GammaUIParticleControl : MonoBehaviour
    {
        [Header("UI")]
        public Graphic uiGraph;

        public Material uiMaterial;
        public bool useMaterialInstance;

        //public string stencilName = "_Stencil";
        //public int stencilValue;

        [Header("Particles")]
        public ParticleSystem[] particles;
        [LayerIndex] public int particleLayer;

        public void Setup()
        {
            if (!uiGraph)
            {
                uiGraph = GetComponent<Graphic>();
            }
            if (!uiMaterial)
                return;

            var uiMat = useMaterialInstance? Instantiate(uiMaterial) : uiMaterial ;
            uiGraph.material = uiMat;


        }

        public void SetupParticles()
        {
            particles = GetComponentsInChildren<ParticleSystem>();

            foreach (var p in particles)
            {
                p.gameObject.layer  = particleLayer;
            }
        }
    }
}
