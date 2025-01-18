namespace PowerUtilities.RenderFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Add this Mono to Camera's gameobject
    /// called events when (SFC)SRPPass execute
    /// </summary>
    public class SRPPassCameraMono : MonoBehaviour
    {
        [HelpBox]
        public string helpBox = "Add this Mono to Camera's gameobject, will call events when (SFC)SRPPass execute";
        /// <summary>
        /// called before SRPPass Execute
        /// </summary>
        public UnityEvent<SRPPass> OnPassExecuteBefore;

        /// <summary>
        /// called end of SRPPass Execute
        /// </summary>
        public UnityEvent<SRPPass> OnPassExecuteEnd;

        [Header("Functions")]
        [Tooltip("make BlitToTarget default blit work one time,then stop")]
        public bool isBlitToTargetBlitOnce;


        public void ShowSRPPassInfo(SRPPass p)
        {
            var genericPass = p as SRPPass<SRPFeature>;
            Debug.Log($"name:{p.featureName},camera:{GetComponent<Camera>()},feature:{genericPass?.Feature}");
        }

        /// <summary>
        /// add to OnPassExecuteBefore
        /// make BlitToTargetPass's defaultBlit work once
        /// </summary>
        /// <param name="p"></param>
        public void BlitToTargetBlitOnce(SRPPass p)
        {
            if (p is BlitToTargetPass blitToPass)
            {
                blitToPass.Feature.isBlitOnce = isBlitToTargetBlitOnce;
            }
        }
    }
}
