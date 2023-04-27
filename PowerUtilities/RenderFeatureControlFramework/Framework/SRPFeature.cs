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

    /// <summary>
    /// SRPPass's parameters
    /// 
    /// </summary>
    public abstract class SRPFeature : ScriptableObject
    {
        public const string SRP_FEATURE_MENU = "SrpRenderFeatures/Passes";
        [Header("Pass Options")]

        [Tooltip("Skip when false")]
        public bool enabled = true;

        [Tooltip("Interrupt others pass when this pass done")]
        public bool interrupt;

        [Header("Pass Options / Filters")]
        [Tooltip("Which camera can run this pass ? only work for Game Camera")]
        public string gameCameraTag = "MainCamera";

        [Header("Pass Options / URP Event")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public int renderPassEventOffset = 0;

        [Header("Pass Options / Pass Log")]
        public string log;
        [HideInInspector]
        public bool isFoldout;
        public abstract ScriptableRenderPass GetPass();
    }

}
