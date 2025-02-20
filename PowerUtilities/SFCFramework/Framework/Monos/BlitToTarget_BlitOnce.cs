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
    public class BlitToTarget_BlitOnce : MonoBehaviour
    {
        [Header("Functions")]
        [Tooltip("make BlitToTarget default blit work one time,then stop")]
        public bool isBlitToTargetBlitOnce  =true;

        public void OnEnable()
        {
            // check screen size event
            ScreenTools.OnScreenSizeChanged -= BlitOnce;
            ScreenTools.OnScreenSizeChanged += BlitOnce;

            SRPPass<BlitToTarget>.OnBeforeExecute -= OnBeforeExecute;
            SRPPass<BlitToTarget>.OnBeforeExecute += OnBeforeExecute;

            SRPPass<BlitToTarget>.OnEndExecute -= OnEndExecute;
            SRPPass<BlitToTarget>.OnEndExecute += OnEndExecute;

            SRPPass<BlitToTarget>.OnCanExecute -= OnCanExecute;
            SRPPass<BlitToTarget>.OnCanExecute += OnCanExecute;
        }
        public void OnDisable()
        {
            ScreenTools.OnScreenSizeChanged -= BlitOnce;
            SRPPass<BlitToTarget>.OnCanExecute -= OnCanExecute;
            SRPPass<BlitToTarget>.OnEndExecute -= OnEndExecute;
            SRPPass<BlitToTarget>.OnBeforeExecute -= OnBeforeExecute;
        }

        private void OnBeforeExecute(SRPPass<BlitToTarget> pass)
        {
            if(pass.Feature.isBlitOnce)
                pass.Feature.isEnable = isBlitToTargetBlitOnce;
        }


        private bool OnCanExecute(SRPPass<BlitToTarget> pass)
        {
            return true;
        }

        private void OnEndExecute(SRPPass<BlitToTarget> pass)
        {
            isBlitToTargetBlitOnce = false;
            pass.Feature.isEnable = false;
        }

        public void BlitOnce()
        {
            isBlitToTargetBlitOnce = true;
        }

    }
}
