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
    public abstract class SRPFeature : ScriptableObject
    {
        public const string SRP_FEATURE_MENU = "SrpRenderFeatures/Passes";
        
        public abstract ScriptableRenderPass GetPass();

        [Tooltip("This pass can run?")]
        public bool enabled = true;

        [Tooltip("Which camera can run this pass ?")]
        public string cameraTag = "MainCamera";

        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public int renderPassEventOffset = 0;
        //[HideInInspector]
        public bool isFoldout;
    }

}
