using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// Control BlitToTarget
    /// 1 blitOnce
    /// 2 blitOnce again when screen size changed
    /// </summary>
    public class BlitToTarget_BlitOnce : SRPPassCameraMono
    {
        [Header("Functions")]
        [Tooltip("make BlitToTarget default blit work one time,then stop")]
        public bool isBlitToTargetBlitOnce;



        public override  void OnEnable()
        {
            base.OnEnable();
            // check screen size event
            ScreenTools.OnScreenSizeChanged -= ScreenTools_OnScreenSizeChanged;
            ScreenTools.OnScreenSizeChanged += ScreenTools_OnScreenSizeChanged;

        }
        private void ScreenTools_OnScreenSizeChanged()
        {
            // Debug.Log(Screen.orientation);
            isBlitToTargetBlitOnce = true;
        }

        /// <summary>
        /// add to OnPassExecuteBefore
        /// make BlitToTargetPass's defaultBlit work once
        /// </summary>
        /// <param name="pass"></param>
        public void BlitToTargetBlitOnce(SRPPass pass)
        {
            if (! IsPassNameMatch(pass))
            {
                return;
            }

            if (pass is BlitToTargetPass blitToPass)
            {
                if (isBlitToTargetBlitOnce)
                {
                    isBlitToTargetBlitOnce = false;

                    blitToPass.Feature.isEnable = true;
                    blitToPass.Feature.isBlitOnce = true;
                }
            }
        }

        public override void DefaultExecuteBefore(SRPPass pass)
        {
            BlitToTargetBlitOnce(pass);
        }

        public void ClickBlitOnce()
        {
            isBlitToTargetBlitOnce = true;
        }

    }
}
