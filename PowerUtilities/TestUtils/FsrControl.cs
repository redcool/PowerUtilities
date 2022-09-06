using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
namespace PowerUtilities.Test
{
    public class FsrControl : MonoBehaviour
    {
        public UniversalAdditionalCameraData cameraData;

        public Slider renderScaleSlider;
        public Slider msaaSlider;
        public void ChangeFSRLevel(int id)
        {
            if (!cameraData)
                return;

            //cameraData.amdFSR = (UniversalAdditionalCameraData.AMDFSR)id;
        }

        public void ChangeRenderScale()
        {
            if (!renderScaleSlider)
                return;

            UniversalRenderPipeline.asset.renderScale = renderScaleSlider.value;

        }

        public void UpdateFXAA()
        {
            cameraData.antialiasing =  cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing ? AntialiasingMode.None : AntialiasingMode.FastApproximateAntialiasing;
        }

        public void ChangeMSAA()
        {
            var e = msaaSlider.value;
            UniversalRenderPipeline.asset.msaaSampleCount = (int)Mathf.Pow(2, e);
        }
    }
}