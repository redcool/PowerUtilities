namespace PowerUtilities.RenderFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
    using UnityEditor;
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SRPFeature), true)]
    public class SRPFeatureEditor : PowerEditor<SRPFeature>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override void DrawInspectorUI(SRPFeature inst)
        {
            //DrawDefaultInspector();    
        }
    }
#endif

    /// <summary>
    /// SRPPass's parameters
    /// 
    /// </summary>
    public abstract class SRPFeature : ScriptableObject
    {
        public const string SRP_FEATURE_MENU = "PowerUtilities/SrpRenderFeatures";
        public const string SRP_FEATURE_PASSES_MENU = SRP_FEATURE_MENU + "/Passes";

        [Header("Pass Options")]
        [Tooltip("Skip when false")]
        public bool enabled = true;

        [Tooltip("Interrupt others pass when this pass done")]
        public bool interrupt;

        [Tooltip("Only work in editor's scene camera")]
        public bool isSceneCameraOnly;

        [Header("Pass Options / Filters")]
        [Tooltip("Which camera can run this pass ? only work for Game Camera")]
        public string gameCameraTag = "MainCamera";

        [Header("Pass Options / URP Event")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public int renderPassEventOffset = 0;

        [Header("Pass Options / Pass Log")]
        [Multiline]
        public string log;

        [HideInInspector]
        public bool isFoldout;

        /// <summary>
        /// get a new pass instance
        /// </summary>
        /// <returns></returns>
        public abstract ScriptableRenderPass GetPass();
        
        // pass instance cache
        ScriptableRenderPass passInstance;

        /// <summary>
        /// get cached pass instance
        /// </summary>
        public ScriptableRenderPass PassInstance
        {
            get
            {
                if (passInstance == null)
                    passInstance = GetPass();
                return passInstance;
            }
        }
    }

}
