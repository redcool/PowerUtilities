namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ProjectSettingsControl : MonoBehaviour
    {
        public Text fpsText;

        public int masterTextureMipmap = 0;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            ShowFPS();
        }

        void ShowFPS()
        {
            if (!fpsText)
                return;

            fpsText.text = (1f / Time.deltaTime).ToString("f2");
        }

        public void ToggleShadow()
        {
            QualitySettings.shadows = QualitySettings.shadows == ShadowQuality.Disable ? ShadowQuality.HardOnly : ShadowQuality.Disable;
        }

        public void ToggleTextureMipmapLevel()
        {
            masterTextureMipmap++;
            masterTextureMipmap %= 2;

            QualitySettings.masterTextureLimit = masterTextureMipmap;
        }

        public void ToggleShadowMaskMode()
        {
            QualitySettings.shadowmaskMode = QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask ? ShadowmaskMode.Shadowmask : ShadowmaskMode.DistanceShadowmask;
        }
    }
}