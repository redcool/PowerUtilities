using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// SRPPass's parameters
    /// 
    /// </summary>
    public abstract class SRPFeature : ScriptableObject
    {
        public const string SRP_FEATURE_MENU = "SrpRenderFeatures/Passes";
        [Header("Pass Options")]

        [Tooltip("This pass can run?")]
        public bool enabled = true;

        [Tooltip("Which camera can run this pass ? only work for Game Camera")]
        public string gameCameraTag = "MainCamera";

        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public int renderPassEventOffset = 0;

        [HideInInspector]
        public bool isFoldout;

        public string log;
        public abstract ScriptableRenderPass GetPass();
    }

}
